using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class FlashbangGrenade : Grenade
{
    public AudioClip flashbangClip;
    public Transform particles;
    private void Start()
    {
        if (!PV.IsMine)
            return;
        Throw();
    }
    public override void Trigger()
    {
        PV.RPC("RPC_GenerateFlashbang", RpcTarget.All);
        Invoke("DisableThis", destroyTimeAfterTrigger);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(PV.IsMine)
        {
            Invoke("Trigger", delay);
        }
    }
    [PunRPC]
    void RPC_GenerateFlashbang()
    {
        GenerateEffects();
        float dist = Vector3.Distance(transform.position, PlayerManager.Instance.currentPlayerGameObject.transform.position);

        if(dist < radius)
        {
            float w = 1 - (dist - 8) / radius;
            WeaponManager.Instance.AddFlashBangEffect(w, 5);
            PlayerAudioManager.Instance.PlayOtherSound(flashbangClip,SoundType.Gun);
            //GameSetup.Instance.AddFlashBangEffect(w, 5);
        }
    }
    void GenerateEffects()
    {
        particles.parent = null;
        particles.rotation = Quaternion.identity;
        particles.GetComponent<ParticleSystem>().Play();
    }


    void DisableThis()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
