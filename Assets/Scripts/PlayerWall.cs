using UnityEngine;

public class PlayerWall : MonoBehaviour
{
    [SerializeField] float wallWidth;
    static Transform target;
    private void Start()
    {
        target = FindObjectOfType<Player>().transform;
    }
    private void LateUpdate()
    {
        if (GoalBase.Instance.IsFailed)
            return;

        var myPos = transform.position;
        myPos.y = 0;
        var v = target.position;
        if ((v - myPos).sqrMagnitude < wallWidth * wallWidth)
        {
            v = myPos + (v - myPos).normalized * wallWidth;
            v.x = 0;
            v.y = 0;
            target.position = v;
        }
    }
    private void OnDrawGizmos()
    {
        if (!enabled)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(wallWidth * 2, 5, wallWidth * 2));
    }
}
