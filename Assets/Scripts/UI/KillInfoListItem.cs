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

        Debug.Log("Killer: " + killerNickname + " team: " + killerTeam);

        if (killerTeam == Team.Red)
        {
            killer.color = Color.red;
        }
        else if (killerTeam == Team.Blue)
        {
            killer.color = Color.blue;
        }

        headshotIcon.SetActive(headShot);


        Weapon w = FindObjectOfType<WeaponManager>()?.GetWeaponByName(weaponName).GetComponent<Weapon>();
        Sprite icon = null;
        if (w != null)
        {
            icon = w.icon;
        }

        if (icon != null)
            weaponIcon.sprite = icon;
        else
        {
            icon = FindObjectOfType<WeaponManager>()?.GetGrenadeIcon(weaponName);
            if (icon != null)
                weaponIcon.sprite = icon;
        }
    }
}
