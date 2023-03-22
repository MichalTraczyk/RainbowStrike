using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour
{
    public AudioSource rain;
    float baseVolume;
    bool inside = false;
    private void Awake()
    {
        UpdateSettings();
    }
    public void UpdateSettings()
    {
        baseVolume = PlayerPrefs.GetFloat("soundFXVolume", 0.5f);
        UpdateCurrentVolume();
    }
    void UpdateCurrentVolume()
    {
        if(inside)
        {
            rain.volume = baseVolume / 5;
        }
        else
        {
            rain.volume = baseVolume;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == PlayerManager.Instance.currentPlayerGameObject)
        {
            inside = true;
            UpdateCurrentVolume();
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerManager.Instance.currentPlayerGameObject)
        {
            inside = false;
            UpdateCurrentVolume();
        }
    }
}
