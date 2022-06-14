using UnityEngine;

public class TestBehaviour : MonoBehaviour
{
#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {        
        if (Input.GetMouseButtonDown(1))
        {
            float min = 1000000;
            Enemy minEnemy = null;
            foreach (var e in FindObjectsOfType<Enemy>())
            {
                if (!e.IsAlive)
                    continue;

                var m = e.ToNearestTarget().magnitude;
                if (m < min)
                {
                    min = m;
                    minEnemy = e;
                }
            }
            minEnemy?.AddDamage(DamageType.Head);
        }
    }
#endif
}
