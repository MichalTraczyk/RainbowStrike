using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour
{
    public Animator animator;
    public AudioClip shellInject;
    public AudioClip reload;
    public void RemoveAmmoLeft()
    {
        animator.SetInteger("ammoToReloadLeft", animator.GetInteger("ammoToReloadLeft") - 1);
    }
    public void PlayReloadEffect()
    {
        GetComponent<PhotonView>().RPC("RPC_PlayReloadSound", RpcTarget.All);
    }
    public void PlayShellInjectEffect()
    {
        GetComponent<PhotonView>().RPC("RPC_PlayShellSound", RpcTarget.All);
    }


    [PunRPC]
    void RPC_PlayReloadSound()
    {
        GetComponentInParent<PlayerAudioManager>().PlayOtherSound(reload,SoundType.Gun);
    }
    [PunRPC]
    void RPC_PlayShellSound()
    {
        GetComponentInParent<PlayerAudioManager>().FireSound(shellInject);
    }
}
