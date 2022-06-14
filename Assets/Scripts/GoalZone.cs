using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] Transform hightlight;
    [SerializeField] float insideHighlightYPos;
    [SerializeField] float outsideHighlightYPos;
    [SerializeField] float reactSpeed;
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    void Update()
    {
        var playerInside = Mathf.Abs(Player.HipsPosition.z - transform.position.z) <= radius;

        if (playerInside)
        {
            GoalBase.Instance.AddValue(GoalType.StayAtPosition, Time.deltaTime);
            GoalBase.Instance.AddValue(GoalType.Chase, GoalBase.Instance.TargetValue);
        }

        hightlight.localPosition = Vector3.Lerp(hightlight.localPosition,
            new Vector3(0, playerInside ? insideHighlightYPos : outsideHighlightYPos, 0),
            Time.deltaTime * reactSpeed);
    }
    private void Start()
    {
        switch (DifficultyManager.GetGoalType())
        {
            case GoalType.StayAtPosition:
                Compass.AddCompass(transform, CompassType.Stay);
                break;
            case GoalType.Chase:
                Compass.AddCompass(transform, CompassType.Run);
                break;
        }
    }
}
