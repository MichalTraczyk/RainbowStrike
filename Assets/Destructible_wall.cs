using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Destructible_wall : Destructible
{

    //Reinforcing 
    bool isBeingReinforced;
    bool isReinforced;
    public float timeToReinforce;
    float reinforceTimer;
    Player playerWhoIsReinforcing;

    public override void Update()
    {
        base.Update();
        if (isBeingReinforced)
        {
            reinforceTimer += Time.deltaTime;
            if (reinforceTimer >= timeToReinforce)
            {
                Reinforce();
            }
        }
    }
    //Reinforcing 
    public void StartReinforcing()
    {
        if (isBeingReinforced)
            return;

        //Play Animation
        PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerShooting>().HideWeapons();

        PlayerManager.Instance.currentPlayerGameObject.GetComponent<Animator>().SetBool("Reinforcing", true);
        PV.RPC("RPC_StartReinforcing", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }


    [PunRPC]
    void RPC_StartReinforcing(Player p)
    {
        isBeingReinforced = true;
        playerWhoIsReinforcing = p;
    }


    public void StopReinforcing()
    {
        Debug.Log("Trying to stop?");
        if (playerWhoIsReinforcing != PhotonNetwork.LocalPlayer)
            return;

        PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerShooting>().ShowWeapons();
        PlayerManager.Instance.currentPlayerGameObject.GetComponent<Animator>().SetBool("Reinforcing", false);
        PV.RPC("RPC_StopReinforcing", RpcTarget.All);
    }
    [PunRPC]
    void RPC_StopReinforcing()
    {
        playerWhoIsReinforcing = null;
        isBeingReinforced = false;
        reinforceTimer = 0;
    }


    public void Reinforce()
    {
        PV.RPC("RPC_Reinforce", RpcTarget.All);
        StopReinforcing();
    }
    [PunRPC]
    void RPC_Reinforce()
    {
        Destroy(GetComponent<Interactable>());
        isReinforced = true;
    }

    [PunRPC]
    void RPC_HitWall(Vector3 pos, float range, float force, bool hard)
    {
        RPC_HitWallOnParent(pos, range, force, hard);
    }


}
