using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class FlashbangGrenade : Grenade
{
    public AudioClip flashbangClip;
    public Transform particles;
    public GameObject AudioContainer;
    bool boomed = false;
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
        if(PV.IsMine&& !boomed)
        {
            boomed = true;
            Invoke("Trigger", delay);
        }
    }
    [PunRPC]
    void RPC_GenerateFlashbang()
    {
        Debug.Log("rzucam!");
        GenerateEffects();
        GameObject playerObj = PlayerManager.Instance.currentPlayerGameObject;
        float dist = Vector3.Distance(transform.position, playerObj.transform.position);
        

        if(dist < radius)
        {
            float w = 1 - (dist - 8) / radius;
            if (CheckIfBehindCover(transform.position, playerObj.transform.position + Vector3.up * 1.5f))
                w /= 10;
            WeaponManager.Instance.AddFlashBangEffect(w, 5);
            //PlayerAudioManager.Instance.PlayOtherSound(flashbangClip,SoundType.Gun);
        }
    }
    void GenerateEffects()
    {
        particles.parent = null;
        particles.rotation = Quaternion.identity;
        particles.GetComponent<ParticleSystem>().Play();


        GameObject audio = Instantiate(AudioContainer, transform.position, Quaternion.identity);
        audio.GetComponent<AudioSource>().clip = flashbangClip;
        audio.GetComponent<AudioSource>().Play();
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
