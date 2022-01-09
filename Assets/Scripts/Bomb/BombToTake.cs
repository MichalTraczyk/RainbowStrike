using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class BombToTake : MonoBehaviour
{
    PhotonView PV;
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }
    void Setup()
    {
        PV = GetComponent<PhotonView>();
        Team t = PlayerManager.Instance.localPlayerTeam;
        if (t != GameManager.Instance.currentTerroTeam)
        {
            Destroy(GetComponent<Interactable>());
        }
    }
    public void AskFortakeBomb()
    {
        GetComponent<Interactable>().owner.GetComponent<PlayerBombPlant>().PickupBomb();
        PV.RPC("RPC_DestroyThis", RpcTarget.All);
    }

    public void DestroyThis()
    {
        PV.RPC("RPC_DestroyThis", RpcTarget.All);
    }
    [PunRPC]
    void RPC_DestroyThis()
    {
        if(PV.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }


}
