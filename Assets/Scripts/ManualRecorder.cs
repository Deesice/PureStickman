#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor;

public class ManualRecorder : MonoBehaviour
{
    [SerializeField] KeyCode startRecordKey;
    [SerializeField] float time;
    float timeStamp;
    RecorderWindow recorderWindow;
    void Start()
    {
        recorderWindow = (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
        timeStamp = Time.unscaledTime + 1000000;
    }
    void Update()
    {
        if (Input.GetKeyDown(startRecordKey) && !recorderWindow.IsRecording())
        {
            Record();
        }
        else if (Time.unscaledTime - timeStamp > time || Input.GetKeyDown(startRecordKey))
        {
            timeStamp = Time.unscaledTime + 1000000;
            if (recorderWindow.IsRecording())
                recorderWindow.StopRecording();
        }
    }
    public void Record()
    {
        recorderWindow.StartRecording();
        timeStamp = Time.unscaledTime;
    }
}
#endif
