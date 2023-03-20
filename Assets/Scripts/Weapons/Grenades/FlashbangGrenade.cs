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

        Debug.Log("1o");
        if(dist < radius)
        {
            Debug.Log("2o");
            float w = 1 - (dist - 8) / radius;

            Debug.Log("3o");
            WeaponManager.Instance.AddFlashBangEffect(w, 5);
            Debug.Log("4o");
            //PlayerAudioManager.Instance.PlayOtherSound(flashbangClip,SoundType.Gun);
            Debug.Log("5o");
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
