using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    public float delay;
    //bool active;
    [SerializeField]
    protected float destroyTimeAfterTrigger = 0.1f;
    protected Player sender;
    protected PhotonView PV;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        sender = (Player)PV.InstantiationData[0];
    }

    public virtual void Trigger()
    {
        ;
    }
    protected void Throw()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * 20;
    }
}
