using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Destructible_Barricade : Destructible
{
    //Wall parameters
    public int neededToDestroy;
    public bool destroyedAtStart = false;
    public bool jumpable = true;
    public GameObject jumpCollider;

    //Repairing
    bool isBeingRepaired;
    float reinforceTimer;
    public float timeToReinforce;
    Player playerWhoIsReinforcing;

    public GameObject interactObject;
    private void Start()
    {
        if (destroyedAtStart)
            DestroyAll();

        jumpCollider.SetActive(jumpable);
        onReset.AddListener(OnReset);
        onHit.AddListener(OnHit);
        InteractCheck();
    }

    void OnReset()
    {
        interactObject.SetActive(true);
    }
    void OnHit()
    {
        if(pieces.Count <= 3 && jumpable)
        {
            jumpCollider.SetActive(true);
        }
        InteractCheck();
    }
    void InteractCheck()
    {
        interactObject.SetActive(pieces.Count + 1 <= allPieces.Count);
    }
    [PunRPC]
    void RPC_HitWall(Vector3 pos, float range, float force, bool hard)
    {
        if (pieces.Count + neededToDestroy <= allPieces.Count)
            DestroyAll();
        else
            RPC_HitWallOnParent(pos, range, force);
    }

    public override void Update()
    {
        base.Update();
        if (isBeingRepaired)
        {
            reinforceTimer += Time.deltaTime;
            if (reinforceTimer >= timeToReinforce)
            {
                RepairWindow();
            }
        }
    }
    //Reinforcing 
    public void StartRepairing()
    {
        if (isBeingRepaired)
            return;
        StartInteraction();
        PV.RPC("RPC_StartRepairing", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }


    [PunRPC]
    void RPC_StartRepairing(Player p)
    {
        isBeingRepaired = true;
        playerWhoIsReinforcing = p;
    }


    public void StopRepairing()
    {
        Debug.Log("Trying to stop?");
        if (playerWhoIsReinforcing != PhotonNetwork.LocalPlayer)
            return;

        PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerShooting>().ShowWeapons();
        PlayerManager.Instance.currentPlayerGameObject.GetComponent<Animator>().SetBool("Reinforcing", false);
        PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerMove>().EnablePlayer();
        PV.RPC("RPC_StopRepairing", RpcTarget.All);
    }
    [PunRPC]
    void RPC_StopRepairing()
    {
        playerWhoIsReinforcing = null;
        isBeingRepaired = false;
        reinforceTimer = 0;
    }


    public void RepairWindow()
    {
        PV.RPC("RPC_Repair", RpcTarget.All);
        StopRepairing();
    }
    [PunRPC]
    void RPC_Repair()
    {
        jumpCollider.SetActive(false);
        Repair();
    }




}
