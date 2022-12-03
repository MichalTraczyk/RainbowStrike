using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[SelectionBase]
public class Destructible : MonoBehaviourPunCallbacks
{
    [SerializeField]public bool soft { get; private set; }
    List<Rigidbody> pieces = new List<Rigidbody>();
    [SerializeField]LayerMask wallLayers;
    public float cooldown = 0.1f;
    PhotonView PV;
    float t = 0;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        foreach(Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            pieces.Add(rb);
        }
    }
    private void Update()
    {
        t += Time.deltaTime;
    }
    [PunRPC]
    void RPC_HitWall(Vector3 pos, float range, float force,bool hard)
    {
        Debug.Log("Hit wall!");
        Collider[] c = Physics.OverlapSphere(pos, range, wallLayers);
        foreach (Collider col in c)
        {
            Rigidbody r = col.GetComponent<Rigidbody>();
            if (pieces.Contains(r))
            {
                Debug.Log("adding force!");
                r.isKinematic = false;
                pieces.Remove(r);
                r.AddExplosionForce(force, pos, range);
                Destroy(r.gameObject, 2);
            }
        }
    }

    public void HitWall(Vector3 pos, float range,float force,bool hard)
    {
        if (t < cooldown)
            return;
        t = 0;
        PV.RPC("RPC_HitWall", RpcTarget.All, pos, range, force,hard);

    }



}
