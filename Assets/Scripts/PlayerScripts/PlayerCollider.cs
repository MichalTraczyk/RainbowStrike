using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class PlayerCollider : MonoBehaviour
{
    PlayerHp player;
    public float multiplier;
    public bool head = false;
    private void Awake()
    {
        player = GetComponentInParent<PlayerHp>();
    }

    public void Damage(float damage, Player killer, string weaponName,Vector3 killerPosition,out bool headshot)
    {
        headshot = head;
        if (player != null)
            player.TakeDamage(Mathf.RoundToInt(damage * multiplier), killer, weaponName,killerPosition,head);
    }
    public void Damage(float damage, Player killer, string weaponName, Vector3 killerPosition)
    {
        if (player != null)
            player.TakeDamage(Mathf.RoundToInt(damage * multiplier), killer, weaponName, killerPosition);
    }
}
