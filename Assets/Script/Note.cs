using UnityEngine;

public class Note
{
    public AudioClip clip;
    public AudioSource audioSource;

    [SerializeField]
    private readonly float speedOfSoundAttenuation = 1.0f;

    public void OnAttenuateIfNeeded()
    {
        if (audioSource.volume >= 0.05)
        {
            audioSource.volume = audioSource.volume - speedOfSoundAttenuation * Time.deltaTime;
        }
    }
}