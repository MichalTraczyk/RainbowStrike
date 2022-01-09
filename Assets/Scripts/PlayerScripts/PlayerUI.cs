using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;

public class PlayerUI : MonoBehaviour
{
    public GameObject shop;
    //DO PRZENIESIENIA: COUNTDOWN
    [Header("Countdown")]
    public GameObject coundownPanel;
    [Header("Weapon UI")]
    public TextMeshProUGUI currentAmmo;
    public TextMeshProUGUI overallAmmo;
    public Image PrimaryWeaponIcon;
    public Image SecondaryWeaponIcon;
    public Image grenadeIcon;

    [Header("Hp")]
    public TextMeshProUGUI hpText;
    public Slider hpSlider;
    [Header("Interact")]
    public TextMeshProUGUI interactText;


    //DO PRZENIESIENIA BOMB PLANTED
    [Header("Bomb")]
    public Image hasBombIcon;
    public Slider BombPlantSlider;


    [Header("Shop")]
    public WeaponInfoPanel weaponInfoPanel;

    [Header("Other")]
    public GameObject hitmarker;

    //Refrences
    private PhotonView PV;
    private PlayerWeaponSwap weaponSwap;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        weaponSwap = GetComponent<PlayerWeaponSwap>();
    }
    private void Start()
    {
        hpSlider.maxValue = 100;
        hpSlider.value = 100;
    }
    private void Update()
    {
        if (!PV.IsMine || GlobalUIManager.Instance.paused)
            return;
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (shop.activeInHierarchy)
            {
                CloseShop();
            }
            else if (!shop.activeInHierarchy && (GameManager.Instance.currentGameState == GameState.RoundPrepare || GameManager.Instance.currentGameState == GameState.Warmup))
            {
                OpenShop();
            }
        }
    }
    public void UpdateInteractMessage(string msg)
    {
        interactText.text = msg;
    }
    void OpenShop()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GetComponent<MouseLook>().enabled = false;
        shop.SetActive(true);
    }
    public void CloseShop()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        GetComponent<MouseLook>().enabled = true;
        shop.SetActive(false);
    }
    public void UpdateGrenade(string grenade)
    {
        grenadeIcon.sprite = WeaponManager.Instance.GetGrenadeIcon(grenade);

        if (grenadeIcon.sprite == null)
            grenadeIcon.color = new Color(1, 1, 1, 0);
        else
            grenadeIcon.color = Color.white;
    }
    public void UpdateWeaponUI(Weapon w,Weapon secondary)
    {
        if(w!=null)
        {
            currentAmmo.text = w.currentAmmoMagazine.ToString();
            overallAmmo.text = w.currentAmmoOverall.ToString();
            PrimaryWeaponIcon.color = Color.white;
            PrimaryWeaponIcon.sprite = w.icon;
        }
        else
        {
            currentAmmo.text = "";
            overallAmmo.text = "";
            PrimaryWeaponIcon.color = new Color(0,0,0,0);
        }

        if(secondary != null)
        {
            SecondaryWeaponIcon.color = Color.white;
            SecondaryWeaponIcon.sprite = secondary.icon;
        }
        else
        {
            SecondaryWeaponIcon.color = new Color(0, 0, 0, 0);
        }
    }
    public void UpdateHp(int hp)
    {
        hpText.text = hp.ToString();
        hpSlider.value = hp;
    }

    #region planting
    public void OnBombisteEnterExit(bool onBombsite)
    {
        if (onBombsite)
        {
            hasBombIcon.color = Color.white;
        }
        else
        {
            hasBombIcon.color = Color.red;
        }
    }
    public void OnBombPickup(bool v)
    {
        hasBombIcon.gameObject.SetActive(v);
    }
    public void StopPlanting()
    {
        BombPlantSlider.gameObject.SetActive(false);
    }
    public void StartPlanting(float startTimeToPlant)
    {
        BombPlantSlider.gameObject.SetActive(true);
        BombPlantSlider.maxValue = startTimeToPlant;
    }
    public void SetPlantingTime(float value)
    {
        BombPlantSlider.value = value;
    }
    #endregion
    #region Weapon buying
    public void OnHealClicked()
    {
        if (500 > GameManager.Instance.getStatsByPlayer(PhotonNetwork.LocalPlayer).money)
            return;
        if (GetComponent<PlayerHp>().ResetHp())
        {
            GameManager.Instance.ChangeMoney(PhotonNetwork.LocalPlayer, -500);
            GlobalSoundManager.Instance.PlayGlobalSound(GlobalSoundManager.Instance.healAudio);
        }
        //GetComponent<Player>().HealServerRpc();
    }

    public void OnWeaponBuyButtonClicked(string weapon, int price)
    {
        weaponSwap.TrySetWeapon(weapon, price, PhotonNetwork.LocalPlayer);
    }

    public void OnMouseHoverOverButton(string weaponName)
    {
        Weapon w = WeaponManager.Instance.GetWeaponByName(weaponName).GetComponent<Weapon>();

        weaponInfoPanel.Setup(w.weaponName, w.baseDamage, w.bulletSpread, w.fireDecay, w.icon);


    }
    public void OnMouseOutOverButton()
    {
        weaponInfoPanel.TurnOff();
    }
    public void OnSetScopeButtonClicked(string scopeName)
    {
        Scope s = Scope.Iron;
        switch (scopeName)
        {
            case "Iron":
                s = Scope.Iron;
                break;
            case "Advanced":
                s = Scope.Advanced;
                break;
            case "Holo":
                s = Scope.Holo;
                break;
            case "Sniper":
                s = Scope.Sniper;
                break;
        }
        weaponSwap.SetScope(s,PhotonNetwork.LocalPlayer);
    }

    public void OnGrenadeButtonClicked(int price, string grenade)
    {
        weaponSwap.TrySetGrenade(grenade, price);
    }
    #endregion


    public void ShowHitmarker()
    {
        StopCoroutine(showHitmarker());

        StartCoroutine(showHitmarker());
    }

    IEnumerator showHitmarker()
    {
        hitmarker.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        hitmarker.SetActive(false);
    }
}
