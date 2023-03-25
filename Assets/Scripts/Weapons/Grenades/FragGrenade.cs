using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FragGrenade : Grenade
{
    public Transform particles;
    public float baseDmgPerPart;
    public float dmgLoss;
    public LayerMask colliderLayers;
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, colliderLayers);
        foreach (Collider c in colliders)
        {
            PlayerCollider pc = c.GetComponent<PlayerCollider>();

            if (pc != null)
            {
                bool behindWall;
                behindWall = CheckIfBehindCover(transform.position, pc.transform.position);

                float dist = Vector3.Distance(transform.position, c.transform.position);
                int damage = Mathf.RoundToInt(baseDmgPerPart - dmgLoss * dist);
                damage = Mathf.Clamp(damage, 0, 100);

                Debug.Log("Trying to damage!");
                pc.Damage(damage, sender, "Frag", transform.position);
            }

            DestructiblePart wall = c.transform.GetComponent<DestructiblePart>();
            if (wall != null && !CheckIfBehindCover(wall.transform.position,transform.position))
            {
                wall.Hit(transform.position, radius/3,500);
            }
        }

        Invoke("DisableThis", destroyTimeAfterTrigger);
        
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
