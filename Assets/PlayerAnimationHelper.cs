using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHelper : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Animator animator;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
    }
    public void PlayLocalAndOnlineAnimation(string local,string network)
    {
        PV.RPC("RPC_PlayLocalAndOnlineAnimation",RpcTarget.All,local,network);
    }
    [PunRPC]
    void RPC_PlayLocalAndOnlineAnimation(string l, string n)
    {
        if (PV.IsMine && !l.Equals(""))
            animator.Play(l);
        
        else if(!PV.IsMine && !n.Equals(""))
            animator.Play(n);
        
    }
}
