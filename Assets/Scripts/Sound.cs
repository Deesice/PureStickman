using UnityEngine;
[CreateAssetMenu(menuName = "Custom/Sound", fileName = "NewSound")]
public class Sound : ScriptableObject
{
    [SerializeField] AudioClip[] clips;
    [SerializeField] float minPitch = 1;
    [SerializeField] float maxPitch = 1;
    [Range(0, 1)]
    [SerializeField] float volume = 1;
    int lastSelected;
    public AudioClip Play(AudioSource source)
    {
        return Play(source, Random.value);
    }
    public AudioClip Play(AudioSource source, float pitchValue)
    {
        if (clips.Length > 1)
        {
            int selected = lastSelected;
            while (selected == lastSelected)
                selected = Random.Range(0, clips.Length);
            lastSelected = selected;
        }
        else
        {
            lastSelected = 0;
        }
        source.pitch = Mathf.Lerp(minPitch, maxPitch, pitchValue);
        source.PlayOneShot(clips[lastSelected], volume);
        return clips[lastSelected];
    }
    public float AverageClipTime()
    {
        if (clips.Length == 0)
            return 0;
        float time = 0;
        foreach (var c in clips)
            time += c.length;
        return time / clips.Length;
    }
}
