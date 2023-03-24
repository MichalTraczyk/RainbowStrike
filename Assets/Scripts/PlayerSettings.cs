using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;
    //Gameplay settings
    public float FieldOfView { get; private set; }
    public float mouseSensitivityX { get; private set; }
    public float mouseSensitivityY { get; private set; }

    private int gfxSettings = 2;

    //Gameplay
    [SerializeField] Slider FOVSlider;
    [SerializeField] Slider mouseXSlider;
    [SerializeField] Slider mouseYSlider;

    //Audio settings
    [SerializeField] Slider GunVolumeSlider;
    [SerializeField] Slider SFXSlider;
    [SerializeField] Slider FootstepsSlider;


    [SerializeField] TextMeshProUGUI gxfSettingsText;


    public Action OnSettingsChanged;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    private void Start()
    {
        FieldOfView = PlayerPrefs.GetFloat("FOV",60);
        mouseSensitivityX = PlayerPrefs.GetFloat("MouseX",7);
        mouseSensitivityY = PlayerPrefs.GetFloat("MouseY",7);


        gfxSettings = PlayerPrefs.GetInt("GFXSettings", 2);
        OnSettingsButtonClicked(0);
        UpdateGfxSettings();
        SlidersSetup();
    }

    void SlidersSetup()
    {
        //Audio sliders
        GunVolumeSlider.value = PlayerPrefs.GetFloat("gunVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("soundFXVolume");
        FootstepsSlider.value = PlayerPrefs.GetFloat("footstepsVolume");

        //Gameplay sliders
        FOVSlider.value = FieldOfView;
        mouseXSlider.value = mouseSensitivityX;
        mouseYSlider.value = mouseSensitivityY;
    }

    public void Save()
    {
        OnSettingsChanged?.Invoke();

        PlayerPrefs.SetFloat("FOV", FieldOfView);
        PlayerPrefs.SetFloat("MouseX", mouseSensitivityX);
        PlayerPrefs.SetFloat("MouseY", mouseSensitivityY);

        PlayerPrefs.SetInt("GFXSettings", gfxSettings);

        UpdateGfxSettings();
    }
    //=================================
    //Gameplay settings
    //=================================
    public void SliderChangedMouseXSensitivity(float newValue)
    {
        mouseSensitivityX = newValue;
    }
    public void SliderChangedMouseYSensitivity(float newValue)
    {
        mouseSensitivityY = newValue;
    }
    public void SliderChangedFieldOfView(float newValue)
    {
        FieldOfView = newValue;
    }

    public void OnSettingsButtonClicked(int dir)
    {
        gfxSettings += dir;
        gfxSettings = Mathf.Clamp(gfxSettings, 0, 2);

        string text = "HIGH";
        if (gfxSettings == 0)
            text = "LOW";
        else if (gfxSettings == 1)
            text = "MEDIUM";

        gxfSettingsText.text = text;

    }
    void UpdateGfxSettings()
    {
        QualitySettings.SetQualityLevel(gfxSettings, true);
    }
    //================================
    //Audio settings
    //================================
    public void SliderChangedFootsteps(float newValue)
    {
        PlayerPrefs.SetFloat("footstepsVolume", newValue);
        GlobalSoundManager.Instance.UpdateVolume();
    }
    public void SliderChangedGunFX(float newValue)
    {
        PlayerPrefs.SetFloat("gunVolume", newValue);
        GlobalSoundManager.Instance.UpdateVolume();
    }
    public void SliderChangedOther(float newValue)
    {
        PlayerPrefs.SetFloat("soundFXVolume", newValue);
        GlobalSoundManager.Instance.UpdateVolume();
        FindObjectOfType<RainController>().UpdateSettings();
    }

}
