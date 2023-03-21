using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using Photon.Pun;
using Photon.Realtime;
public class PlayerHp : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    public const int startHp = 100;
    int currentHp = startHp;
    PlayerUI playerUI;

    List<Player> whoDamaged = new List<Player>();

    public GameObject dieCamera;
    public GameObject weaponParent;
    bool deadAlready;

    Volume dieVolume;
    Volume damageVolume;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerUI = GetComponent<PlayerUI>();
    }
    void Start()
    {
        dieVolume = GameObject.FindGameObjectWithTag("DyingVolume").GetComponent<Volume>();
        damageVolume = GameObject.FindGameObjectWithTag("DamageVolume").GetComponent<Volume>();

        //Disabling ragdoll at start
        Rigidbody[] allRBs = GetComponentsInChildren<Rigidbody>();
        for (int r = 0; r < allRBs.Length; r++)
        {
            allRBs[r].isKinematic = true;
        }

    }

    public bool ResetHp()
    {
        bool healded = true;
        if (currentHp == startHp)
            healded = false;
        else
        {
            StopAllCoroutines();
            dieVolume.weight = 0;
        }    
        currentHp = startHp;
        whoDamaged.Clear();
        playerUI.UpdateHp(currentHp);
        return healded;
    }
    
    public void TakeDamage(int damage, Player killer, string weaponName, Vector3 pos, bool headshot = false)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, killer, weaponName, pos,headshot);
    }
    [PunRPC]
    void RPC_TakeDamage(int damage, Player killer, string weaponName,Vector3 pos,bool headshot)
    {
        if (!PV.IsMine)
            return;
        if (GameManager.Instance.currentGameState == GameState.WarmupEnd)
            return;
        if (deadAlready)
            return;

        StartCoroutine(VolumeAnimation(damageVolume, 0.05f, 0.2f));

        currentHp -= damage;
        PV.RPC("RPC_UpdateUI", RpcTarget.All, currentHp);

        //Adding players to list who assisted in killing this
        if(killer != null)
        {
            if(!whoDamaged.Contains(killer))
                whoDamaged.Add(killer);
        }

        if (currentHp<=0)
        {
            //Throwing off current weapon
            GetComponent<PlayerWeaponSwap>().DropWeapon();

            //Throwing off bumb if player has it in eq
            GetComponent<PlayerBombPlant>().DropBomb();


            //Adding stats
            foreach(Player p in whoDamaged)
            {
                if(p!=killer)
                {
                    GameManager.Instance.AddAssist(p);
                }
            }
            if(killer != null)
            {
                GameManager.Instance.AddKill(killer);
                if(headshot)
                {
                    GameManager.Instance.AddHeadshot(killer);
                }
            }

            //Adding money
            GameManager.Instance.ChangeMoney(killer, 400);

            //Sending message about kill
            string killerNickname = killer.NickName;
            string killedNickanme = PhotonNetwork.LocalPlayer.NickName;
            Team myTeam = PlayerManager.Instance.localPlayerTeam;
            KillInfo thisKillInfo = new KillInfo(
                killerNickname,
                killedNickanme,
                headshot,
                myTeam,
                weaponName
                );
            GameManager.Instance.OnPlayerKilled(PhotonNetwork.LocalPlayer,thisKillInfo);


            //Changing stats
            GameManager.Instance.AddDeath(PhotonNetwork.LocalPlayer);

            //Destroying player and adding ragdoll
            Die(damage,pos);
        }
    }
    [PunRPC]
    void RPC_UpdateUI(int hp)
    {
        playerUI.UpdateHp(hp);
    }


    public void BombDamage(int damage, Vector3 pos)
    {
        TakeDamage(damage, null, "", pos);
    }
    void Die(int damage, Vector3 pos)
    {
        deadAlready = true;
        PlayerManager.Instance.changeAliveState(false);
        GetComponent<PlayerNetworkSetup>().playerManager.Die();
        StopAllCoroutines();
        StartCoroutine(dieVolumeAnimation());
        PV.RPC("RPC_StartRagdoll", RpcTarget.All, damage, pos);
        
    }
    [PunRPC]
    void RPC_StartRagdoll(int dmg,Vector3 pos)
    {
        if(PV.IsMine)
        {
            dieCamera.SetActive(true);
        }
        if(GetComponent<CharacterController>() != null)
            GetComponent<CharacterController>().enabled = false;

        GetComponent<PlayerMove>().enabled = false;
        GetComponent<Animator>().enabled = false;
        GetComponentInChildren<Rig>().weight = 0;
        GetComponent<MouseLook>().enabled = false;
        GetComponent<PlayerShooting>().enabled = false;
        GetComponent<PlayerWeaponSwap>().enabled = false;
        weaponParent.SetActive(false);

        Rigidbody[] allRBs = GetComponentsInChildren<Rigidbody>();
        for (int r = 0; r < allRBs.Length; r++)
        {
            PlayerCollider pc = allRBs[r].GetComponent<PlayerCollider>();
            if (pc != null)
                Destroy(pc);
            allRBs[r].isKinematic = false;
        }

        //Adding force on death
        Vector3 thisPos = transform.position;
        Vector3 v = pos - thisPos;
        Vector3 force = v.normalized * dmg * 140;
        force *= -1;
        allRBs[0].AddForce(force);

    }

    IEnumerator VolumeAnimation(Volume v,float attackTime,float dieTime)
    {
        while(v.weight < 0.99f)
        {
            v.weight += Time.deltaTime / attackTime;
            yield return new WaitForEndOfFrame();
        }
        while(v.weight > 0)
        {
            v.weight -= Time.deltaTime / dieTime;
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator dieVolumeAnimation()
    {
        while(dieVolume.weight< 0.99f)
        {
            dieVolume.weight = Mathf.Lerp(dieVolume.weight, 1, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
    }
    private void OnDestroy()
    {
        if (dieVolume != null)
            dieVolume.weight = 0;
        if (damageVolume != null)
            damageVolume.weight = 0;
    }
}
