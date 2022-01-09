using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public struct KillInfo
{
    public string KillerNickname;
    public string KilledNickname;
    public bool HeadShot;
    public Team KillerTeam;
    public string WeaponName;

    public KillInfo(string killerNickname, string killedNickname, bool headShot, Team killerTeam, string weaponName)
    {
        KillerNickname = killerNickname;
        KilledNickname = killedNickname;
        HeadShot = headShot;
        KillerTeam = killerTeam;
        WeaponName = weaponName;
    }
}

public class KillInfoListItem : MonoBehaviour
{
    public TextMeshProUGUI killer;
    public TextMeshProUGUI killed;
    public Image weaponIcon;
    public GameObject headshotIcon;
    private void Start()
    {
        Destroy(this.gameObject, 4);
    }
    public void Setup(string killerNickname, string killedNickname, bool headShot, Team killerTeam, string weaponName)
    {
        killer.text = killerNickname;

        killed.text = killedNickname;
        if (killerTeam == Team.Red)
        {
            killer.color = Color.red;
        }
        else if (killerTeam == Team.Blue)
        {
            killer.color = Color.blue;
        }

        headshotIcon.SetActive(headShot);
        if(weaponName == "Bomb")
        {

        }
        else if (weaponName == "Grenade")
        {
        }
        else if(weaponName == "")
        {

        }
        else
        {
            Sprite icon = FindObjectOfType<WeaponManager>()?.GetWeaponByName(weaponName).GetComponent<Weapon>()?.icon;
            if (icon != null)
                weaponIcon.sprite = icon;
        }

        /*
        Debug.Log("4");
        killer.text = killinfo.KillerNickname;
        killed.text = killinfo.KilledNickname;
        if (killinfo.KillerTeam == Team.Red)
        {
            killer.color = Color.red;
        }
        else if (killinfo.KillerTeam == Team.Blue)
        {
            killer.color = Color.blue;
        }

        if (killinfo.HeadShot)
        {
            headshotIcon.SetActive(true);
        }


        if (killinfo.WeaponName != "Grenade")
        {
            if (killinfo.WeaponName != "")
            {
                Sprite icon = FindObjectOfType<WeaponManager>()?.GetWeaponByName(killinfo.WeaponName).GetComponent<Weapon>()?.icon;
                if (icon != null)
                    weaponIcon.sprite = icon;
            }
        }*/


    }
}
