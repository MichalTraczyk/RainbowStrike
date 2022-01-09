using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WeaponButton : MonoBehaviour
{
    public string weaponName;
    public int price;
    public Image image;
    public TextMeshProUGUI priceText;
    PlayerUI ui;
    private void Start()
    {
        ui = GetComponentInParent<PlayerUI>();
        priceText.text = price.ToString();
        Weapon w = WeaponManager.Instance.GetWeaponByName(weaponName).GetComponent<Weapon>();
        image.sprite = w.icon;
    }

    public void OnClick()
    {
        ui.OnWeaponBuyButtonClicked(weaponName, price);
    }
    public void OnHover()
    {
        ui.OnMouseHoverOverButton(weaponName);
    }
    public void OnOutHover()
    {
        ui.OnMouseOutOverButton();
    }
}
