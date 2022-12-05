using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[SelectionBase]
public class Destructible : MonoBehaviourPunCallbacks
{
    [SerializeField]public bool soft { get; private set; }
    List<Rigidbody> pieces = new List<Rigidbody>();
    [SerializeField]LayerMask wallLayers;
    public float cooldown = 0.1f;
    PhotonView PV;

    //Reinforcing 
    bool isBeingReinforced;
    bool isReinforced;
    public float timeToReinforce;
    float reinforceTimer;
    Player playerWhoIsReinforcing;

    float t = 0;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        foreach(Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            pieces.Add(rb);
        }
    }
    private void Update()
    {
        t += Time.deltaTime;

        if(isBeingReinforced)
        {
            reinforceTimer += Time.deltaTime;
            if(reinforceTimer>=timeToReinforce)
            {
                Reinforce();
            }
        }
    }
    [PunRPC]
    void RPC_HitWall(Vector3 pos, float range, float force,bool hard)
    {
        Debug.Log("Hit wall!");
        Collider[] c = Physics.OverlapSphere(pos, range, wallLayers);
        foreach (Collider col in c)
        {
            Rigidbody r = col.GetComponent<Rigidbody>();
            if (pieces.Contains(r))
            {
                Debug.Log("adding force!");
                r.isKinematic = false;
                pieces.Remove(r);
                r.AddExplosionForce(force, pos, range);
                Destroy(r.gameObject, 2);
            }
        }
    }

    public void HitWall(Vector3 pos, float range,float force,bool hard)
    {
        if (t < cooldown)
            return;
        t = 0;
        PV.RPC("RPC_HitWall", RpcTarget.All, pos, range, force,hard);
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
        PV.RPC("RPC_StopReinforcing",RpcTarget.All);
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




}
