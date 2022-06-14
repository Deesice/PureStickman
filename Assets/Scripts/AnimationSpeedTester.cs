using UnityEngine;

public class AnimationSpeedTester : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float timeScale;
    [SerializeField] int targetFrameRate;
    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
    }
    void Update()
    {
        Time.timeScale = timeScale;
        transform.position += new Vector3(0, 0, Time.deltaTime * speed);
    }
}
