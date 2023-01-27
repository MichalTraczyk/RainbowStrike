using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public abstract class Grenade : MonoBehaviour
{
    public float delay;
    //bool active;
    [SerializeField]
    protected float destroyTimeAfterTrigger = 0.1f;
    protected Player sender;
    protected PhotonView PV;
    LayerMask groundLayers;
    [SerializeField] protected float radius;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        sender = (Player)PV.InstantiationData[0];
    }
    protected bool CheckIfBehindCover(Vector3 pos1, Vector3 pos2)
    {
        Vector3 direction = pos2 - pos1;
        return Physics.Raycast(pos1,direction,radius,groundLayers);

    }
    public abstract void Trigger();

    protected void Throw()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * 20;
    }
}
