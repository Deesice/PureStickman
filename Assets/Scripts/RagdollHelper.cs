using UnityEngine;
using System.Collections.Generic;
using System;

/*
A helper component that enables blending from Mecanim animation to ragdolling and back. 

To use, do the following:

Add "GetUpFromBelly" and "GetUpFromBack" bool inputs to the Animator controller
and corresponding transitions from any state to the get up animations. When the ragdoll mode
is turned on, Mecanim stops where it was and it needs to transition to the get up state immediately
when it is resumed. Therefore, make sure that the blend times of the transitions to the get up animations are set to zero.

TODO:

Make matching the ragdolled and animated root rotation and position more elegant. Now the transition works only if the ragdoll has stopped, as
the blending code will first wait for mecanim to start playing the get up animation to get the animated hip position and rotation. 
Unfortunately Mecanim doesn't (presently) allow one to force an immediate transition in the same frame. 
Perhaps there could be an editor script that precomputes the needed information.

*/

public class RagdollHelper : MonoBehaviour {
	public void EnableRagdoll()
    {
		if (state != RagdollState.ragdolled)
		{
			//Transition from animated to ragdolled
			SetKinematic(false); //allow the ragdoll RigidBodies to react to the environment
			anim.enabled = false; //disable animation
			state = RagdollState.ragdolled;
		}
	}
	public void DisableRagdoll(string boolName, bool value)
	{
		if (state == RagdollState.ragdolled)
		{
			DisableRagdoll();
			anim.SetBool(boolName, value);
		}
	}
	public void DisableRagdoll(string triggerName)
    {
		if (state == RagdollState.ragdolled)
		{
			DisableRagdoll();
			anim.SetTrigger(triggerName);
		}
	}
	public void DisableRagdoll()
    {
		if (state == RagdollState.ragdolled)
		{
			//Transition from ragdolled to animated through the blendToAnim state
			SetKinematic(true); //disable gravity etc.
			ragdollingEndTime = Time.time; //store the state change time
			anim.enabled = true; //enable animation
			state = RagdollState.blendToAnim;

			//Store the ragdolled position for blending
			foreach (BodyPart b in bodyParts)
			{
				b.storedRotation = b.transform.rotation;
				b.storedPosition = b.transform.position;
			}

			//Remember some key positions
			ragdolledFeetPosition = 0.5f * (GetBoneTransform(HumanBodyBones.LeftToes).position + GetBoneTransform(HumanBodyBones.RightToes).position);
			ragdolledHeadPosition = GetBoneTransform(HumanBodyBones.Head).position;
			ragdolledHipPosition = GetBoneTransform(HumanBodyBones.Hips).position;
		}
	}
	public bool IsLiesOnBelly => GetBoneTransform(HumanBodyBones.Hips).forward.y <= 0;
	public bool ragdolled => state != RagdollState.animated;
    //Possible states of the ragdoll
	enum RagdollState
	{
		animated,	 //Mecanim is fully in control
		ragdolled,   //Mecanim turned off, physics controls the ragdoll
		blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
	}
	
	//The current state
	RagdollState state=RagdollState.animated;
	
	//How long do we blend when transitioning from ragdolled to animated
	[SerializeField] float ragdollToMecanimBlendTime=0.5f;
	const float mecanimToGetUpTransitionTime=0.05f;
	
	//A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
	float ragdollingEndTime=-100;
	
	//Declare a class that will hold useful information for each body part
	public class BodyPart
	{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
		public BodyPart(Transform t)
        {
			transform = t;
        }
	}
	//Additional vectores for storing the pose the ragdoll ended up in.
	Vector3 ragdolledHipPosition,ragdolledHeadPosition,ragdolledFeetPosition;
	
	//Declare a list of body parts, initialized in Start()
	List<BodyPart> bodyParts=new List<BodyPart>();
	
	//Declare an Animator member variable, initialized in Start to point to this gameobject's Animator component.
	Animator anim;

	//A helper function to set the isKinematc property of all RigidBodies in the children of the 
	//game object that this script is attached to
	public bool isKinematicNow { get; private set; }
	public void SetKinematic(bool newValue)
	{
		if (isKinematicNow == newValue)
			return;

		isKinematicNow = newValue;
		//For each of the components in the array, treat the component as a Rigidbody and set its isKinematic property
		foreach (var c in bodies)
		{
			c.isKinematic=newValue;
			//c.detectCollisions = !newValue;
		}
	}
	List<Rigidbody> bodies = new List<Rigidbody>();
	RaycastHit[] hits;
	[SerializeField] int raycastBufferSize = 20;
	
	// Initialization, first frame of game
	void Start ()
	{
		if (raycastBufferSize < 1)
			raycastBufferSize = 1;
		hits = new RaycastHit[raycastBufferSize];

		var myRigidbody = GetComponent<Rigidbody>();
		foreach (var rb in GetComponentsInChildren<Rigidbody>())
			if (rb != myRigidbody && !rb.GetComponent<PieceOfMeat>())
				bodies.Add(rb);
		//Set all RigidBodies to kinematic so that they can be controlled with Mecanim
		//and there will be no glitches when transitioning to a ragdoll
		isKinematicNow = false;
		SetKinematic(true);
		
		//For each of the transforms, create a BodyPart instance and store the transform 
		foreach (var c in GetComponentsInChildren<Transform>())
		{
			bodyParts.Add(new BodyPart(c));
		}
		
		//Store the Animator component
		anim=GetComponent<Animator>();
	}
  //  private void FixedUpdate()
  //  {
		//if (isKinematicNow)
		//	foreach (var c in bodies)
		//		c.Sleep();
  //  }
    void LateUpdate()
	{
		//Blending from ragdoll back to animated
		if (state==RagdollState.blendToAnim)
		{
			if (Time.time<=ragdollingEndTime+mecanimToGetUpTransitionTime)
			{
				//If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
				//character to the best match with the ragdoll
				Vector3 animatedToRagdolled=ragdolledHipPosition-GetBoneTransform(HumanBodyBones.Hips).position;
				Vector3 newRootPosition=transform.position + animatedToRagdolled;
					
				//Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
				//RaycastHit[] hits=Physics.RaycastAll(new Ray(newRootPosition,Vector3.down));
				var count = Physics.RaycastNonAlloc(new Ray(newRootPosition, Vector3.down), hits);
				newRootPosition.y=0;
				for (int i = 0; i < count; i++)
				{
					var hit = hits[i];
					if (!hit.transform.IsChildOf(transform))
					{
						newRootPosition.y=Mathf.Max(newRootPosition.y, hit.point.y);
					}
				}
				transform.position=newRootPosition;
				
				//Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
				Vector3 ragdolledDirection=ragdolledHeadPosition-ragdolledFeetPosition;
				ragdolledDirection.y=0;

				Vector3 meanFeetPosition=0.5f*(GetBoneTransform(HumanBodyBones.LeftFoot).position + GetBoneTransform(HumanBodyBones.RightFoot).position);
				Vector3 animatedDirection=GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
				animatedDirection.y=0;
										
				//Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
				//hence setting the y components of the vectors to zero. 
				transform.rotation*=Quaternion.FromToRotation(animatedDirection.normalized,ragdolledDirection.normalized);
			}
			//compute the ragdoll blend amount in the range 0...1
			float ragdollBlendAmount=1.0f-(Time.time-ragdollingEndTime-mecanimToGetUpTransitionTime)/ragdollToMecanimBlendTime;
			ragdollBlendAmount=Mathf.Clamp01(ragdollBlendAmount);
			
			//In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
			//To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
			//and slerp all the rotations towards the ones stored when ending the ragdolling
			foreach (BodyPart b in bodyParts)
			{
				if (b.transform!=transform){ //this if is to prevent us from modifying the root of the character, only the actual body parts
					//position is only interpolated for the hips
					if (b.transform==GetBoneTransform(HumanBodyBones.Hips))
						b.transform.position=Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
					//rotation is interpolated for all body parts
					b.transform.rotation=Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
				}
			}
			
			//if the ragdoll blend amount has decreased to zero, move to animated state
			if (ragdollBlendAmount==0)
			{
				state=RagdollState.animated;
				return;
			}
		}
	}
	
	Transform GetBoneTransform(HumanBodyBones humanBodyBone)
    {
		switch (humanBodyBone)
        {
			case HumanBodyBones.LeftToes:
				return leftToes;
			case HumanBodyBones.RightToes:
				return rightToes;
			case HumanBodyBones.Head:
				return head;
			case HumanBodyBones.Hips:
				return hips;
			case HumanBodyBones.LeftFoot:
				return leftFoot;
			case HumanBodyBones.RightFoot:
				return rightFoot;
			default:
				throw new NotImplementedException();
        }
	}
	[Header("Bone transforms")]
	[SerializeField] Transform leftToes;
	[SerializeField] Transform rightToes;
	[SerializeField] Transform leftFoot;
	[SerializeField] Transform rightFoot;
	[SerializeField] Transform hips;
	[SerializeField] Transform head;
}
