using UnityEngine;

public class SpinAnimation : MonoBehaviour, IPool
{
    [SerializeField] float spinSpeed;
    [SerializeField] float jugleSpeed;
    [SerializeField] float jugleAplitude;
    [SerializeField] Vector3 initialPosition;

    public void OnTakeFromPool()
    {
        initialPosition = transform.position;
    }

    private void Awake()
    {
        OnTakeFromPool();
    }
    private void Update()
    {
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
        if (Mathf.Abs(jugleSpeed) > 0)
        {
            transform.position = initialPosition +
                Mathf.Sin(Time.time * jugleSpeed) * jugleAplitude * transform.up;
        }
    }
}
