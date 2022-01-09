using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SmokeGrenade : Grenade
{
    public Transform particles;
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
        PV.RPC("RPC_Trigger", RpcTarget.All);
        Invoke("DisableThis", destroyTimeAfterTrigger);
    }
    [PunRPC]
    void RPC_Trigger()
    {
        particles.GetComponent<ParticleSystem>().Play();
    }
    void DisableThis()
    {
        PV.RPC("RPC_Disable", RpcTarget.All);
        PhotonNetwork.Destroy(this.gameObject);
    }
    [PunRPC]
    void RPC_Disable()
    {
        particles.parent = null;
        particles.GetComponent<ParticleSystem>().Stop();
    }
    // Update is called once per frame
    void Update()
    {
        particles.eulerAngles = new Vector3(-90, 0, -180);
    }
}
