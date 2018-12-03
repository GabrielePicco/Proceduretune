using System.Linq;
using UnityEngine;

public class Note
{
    public AudioClip clip;
    public AudioSource audioSource;
    public GameObject sprite;
    public string bundle;
    [SerializeField]
    private readonly float speedOfSoundAttenuation = 1.0f;


    /**
     * Reduce the volume to attenuate the sound
     */
    public void OnAttenuateIfNeeded()
    {
        if (audioSource.volume >= 0.05)
        {
            audioSource.volume = audioSource.volume - speedOfSoundAttenuation * Time.deltaTime;
        }
    }

    public override string ToString()
    {
        return clip.name.Split('.').Last();
    }

}