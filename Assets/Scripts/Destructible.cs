using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[SelectionBase]
public class Destructible : MonoBehaviourPunCallbacks
{
    [SerializeField]public bool soft { get; private set; }
    protected List<Rigidbody> pieces = new List<Rigidbody>();
    [SerializeField] protected LayerMask wallLayers;
    public float cooldown = 0.1f;
    protected PhotonView PV;

    float t;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        foreach(Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            pieces.Add(rb);
        }
    }
    protected void RPC_HitWallOnParent(Vector3 pos, float range, float force,bool hard)
    {
        Debug.Log("Hit wall on rpc!");
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
    public virtual void Update()
    {
        t += Time.deltaTime;
    }
    public void HitWall(Vector3 pos, float range,float force,bool hard)
    {
        if (t < cooldown)
            return;
        Debug.Log("hit on wall");
        t = 0;
        PV.RPC("RPC_HitWall", RpcTarget.All, pos, range, force,hard);
    }
}
