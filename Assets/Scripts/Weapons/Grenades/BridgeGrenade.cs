using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class BridgeGrenade : Grenade
{
    public Transform particles;
    public float baseDmgPerPart;
    public float dmgLoss;
    public float radius;
    public LayerMask playerColliderLayers;
    public GameObject AudioContainer;
    public AudioClip explosionGrenade;
    private void Start()
    {
        if (!PV.IsMine)
            return;
        Throw();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (PV.IsMine)
        {
            Invoke("Trigger", delay);
        }
    }
    public override void Trigger()
    {
        PV.RPC("RPC_PlayParticles", RpcTarget.All);
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, playerColliderLayers);
        foreach (Collider c in colliders)
        {
            PlayerCollider pc = c.GetComponent<PlayerCollider>();
            if (pc != null)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                int damage = Mathf.RoundToInt(baseDmgPerPart - dmgLoss * dist);
                damage = Mathf.Clamp(damage, 0, 100);
                pc.Damage(damage, sender, "Grenade", transform.position);
            }
        }

        if (PV.IsMine)
        {
            Invoke("DisableThis", destroyTimeAfterTrigger);
        }
    }

    [PunRPC]
    void RPC_PlayParticles()
    {
        particles.parent = null;
        particles.rotation = Quaternion.identity;
        particles.GetComponent<ParticleSystem>().Play();

        GameObject audio = Instantiate(AudioContainer, transform.position, Quaternion.identity);
        audio.GetComponent<AudioSource>().clip = explosionGrenade;
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
