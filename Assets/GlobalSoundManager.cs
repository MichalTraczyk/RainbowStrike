using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSoundManager : MonoBehaviour
{
    public static GlobalSoundManager Instance;
    public float soundFXVolume { get; private set; }
    public float footstepsVolume { get; private set; }
    public float gunVolume { get; private set; }

    [SerializeField] AudioSource source;

    public AudioClip healAudio;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        UpdateVolume();
    }
    public void UpdateVolume()
    {
        soundFXVolume = PlayerPrefs.GetFloat("soundFXVolume", 0.5f);
        footstepsVolume = PlayerPrefs.GetFloat("footstepsVolume", 0.5f);
        gunVolume = PlayerPrefs.GetFloat("gunVolume", 0.5f);
    }

    public void PlayGlobalSound(AudioClip clip)
    {
        source.PlayOneShot(clip, soundFXVolume);
    }
    
}
