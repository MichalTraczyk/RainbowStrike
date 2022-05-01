using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WeaponInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI weaponNameT;
    public Image Icon;
    public Slider damageSlider;
    public Slider accuracySlider;
    public Slider rateOfFireSlider;

    public GameObject parentObj;

    public void Setup(string weaponName, float damage,float accuracy,float rateOfFire,Sprite icon)
    {
        parentObj.SetActive(true);
        weaponNameT.text = weaponName;
        Icon.sprite = icon;

        damageSlider.value = damage;

        accuracySlider.value = 0.8f - accuracy;

        rateOfFireSlider.value = rateOfFire;
    }
    public void TurnOff()
    {
        parentObj.SetActive(false);
    }
}
