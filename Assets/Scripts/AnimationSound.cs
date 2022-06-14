using UnityEngine;

public class AnimationSound : MonoBehaviour
{
    [SerializeField] Sound[] sounds;
    [SerializeField] AudioSource source;
    [Range (0,1)]
    [SerializeField] float defaultPitchValue;
    float lastPlayTime;
    [SerializeField] float minPauseBetweenPlays;
    public void Play(int soundIdx)
    {
        if (Time.time - lastPlayTime < minPauseBetweenPlays)
            return;

        lastPlayTime = Time.time;
        sounds[soundIdx].Play(source, defaultPitchValue);
    }
}
