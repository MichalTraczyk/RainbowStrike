using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;

public class PlayerUI: MonoBehaviour
{
    public GameObject shop;
    [Header("Weapon UI")]
    [SerializeField] TextMeshProUGUI currentAmmo;
    [SerializeField] TextMeshProUGUI overallAmmo;
    [SerializeField] Image PrimaryWeaponIcon;
    [SerializeField] Image SecondaryWeaponIcon;
    [SerializeField] Image grenadeIcon;

    [Header("Hp")]
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] Slider hpSlider;
    [Header("Interact")]
    [SerializeField] TextMeshProUGUI interactText;


    //DO PRZENIESIENIA BOMB PLANTED
    [Header("Bomb")]
    [SerializeField] Image hasBombIcon;
    [SerializeField] Slider BombPlantSlider;


    [Header("Shop")]
    [SerializeField] WeaponInfoPanel weaponInfoPanel;


    [Header("Pings")]
    [SerializeField] Marker BombsiteAIcon;
    [SerializeField] Marker BombsiteBIcon;
    [SerializeField] GameObject pingIconPrefab;
    [SerializeField] RectTransform pingsParent;
    [SerializeField] GameObject hitmarker;

    private List<Marker> pingIcons = new List<Marker>();

    [Header("Other")]
    private Camera mainCam;

    //Refrences
    private PhotonView PV;
    private PlayerWeaponSwap weaponSwap;

    public GameObject nicknameIcon;


    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        weaponSwap = GetComponent<PlayerWeaponSwap>();
        mainCam = Camera.main;
    }
    private void Start()
    {
        hpSlider.maxValue = 100;
        hpSlider.value = 100;


        ((BombsiteIcon)BombsiteAIcon).Setup(GameObject.FindGameObjectWithTag("BombsiteA").transform.position);
        ((BombsiteIcon)BombsiteBIcon).Setup(GameObject.FindGameObjectWithTag("BombsiteB").transform.position);

        AddPing(BombsiteAIcon);
        AddPing(BombsiteBIcon);
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
        UpdatePings();
    }

    #region pings
    public void AddPing(Marker markerPrefab)
    {
        markerPrefab.transform.SetParent(pingsParent);
        pingIcons.Add(markerPrefab);
    }
    public void UpdatePings()
    {
        foreach(Marker m in pingIcons)
        {
            Debug.Log(m);
            m.UpdatePosition(transform.position);
        }
    }
    public void OnPingDestroy(Marker t)
    {
        if(pingIcons.Contains(t))
        {
            pingIcons.Remove(t);
            Destroy(t.gameObject);
        }
    }
    public Vector3 getPingPosition(Vector3 worldPos)
    {
        Vector3 pos = mainCam.WorldToViewportPoint(worldPos);
        pos.x *= Screen.width;
        pos.y *= Screen.height;
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
        return pos;
    }


    public void RefreshNicknameIcons()
    {
        List<Marker> markersToRemove = new List<Marker>();
        foreach(Marker m in pingIcons)
        {
            if(m.GetType() == typeof(NicknameIcon))
            {
                markersToRemove.Add(m);
            }
        }

        foreach(Marker m in markersToRemove)
        {
            pingIcons.Remove(m);
            Destroy(m.gameObject);
        }


        GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject go in gos)
        {
            if (go == this.gameObject)
                continue;

            PlayerNetworkSetup pns = go.GetComponent<PlayerNetworkSetup>();
            Team t = pns.thisPlayerTeam;

            if(t == PlayerManager.Instance.localPlayerTeam)
            {
                GameObject markerGO = Instantiate(nicknameIcon);
                NicknameIcon NI = markerGO.GetComponent<NicknameIcon>();
                NI.Setup(go.transform, pns.thisPlayerNickname);
                AddPing(NI);
            }
        }
    }
    #endregion
    #region updates
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
    #endregion


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

        weaponInfoPanel.Setup(w.weaponName, w.baseDamage, w.bulletSpread, 60/w.fireDecay, w.icon);


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

    #region hitmarker
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
    #endregion
}
