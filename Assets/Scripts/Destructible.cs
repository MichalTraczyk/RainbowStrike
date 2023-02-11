using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.Events;

[SelectionBase]
public abstract class Destructible : MonoBehaviourPunCallbacks
{
    [SerializeField]public bool soft { get; private set; }

    protected List<Rigidbody> pieces = new List<Rigidbody>();
    protected List<Rigidbody> allPieces = new List<Rigidbody>();

    [SerializeField] protected LayerMask wallLayers;
    public float cooldown = 0.1f;
    protected PhotonView PV;

    float t;
    protected UnityEvent onReset;
    protected UnityEvent onHit;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (onReset == null)
            onReset = new UnityEvent();
        if (onHit == null)
            onHit = new UnityEvent();

        foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            pieces.Add(rb);
        }
        allPieces = new List<Rigidbody>(pieces);
    }
    protected void HitWallOnParent(Vector3 pos, float range, float force)
    {
        Collider[] c = Physics.OverlapSphere(pos, range, wallLayers);
        foreach (Collider col in c)
        {
            Rigidbody r = col.GetComponent<Rigidbody>();
            if (pieces.Contains(r))
            {
                RemovePart(r, pos, range, force);
            }
        }
    }
    void RemovePart(Rigidbody r,Vector3 pos,float range, float force)
    {
        r.isKinematic = false;
        pieces.Remove(r);
        StartCoroutine(ResetPart(r, r.position,r.rotation));
        r.AddExplosionForce(force, pos, range);
        onHit.Invoke();
    }
    IEnumerator ResetPart(Rigidbody part,Vector3 startPos,Quaternion startRot)
    {
        yield return new WaitForSeconds(2);
        part.isKinematic = true;
        part.transform.position = startPos;
        part.transform.rotation = startRot;
        part.gameObject.SetActive(false);
    }
    public virtual void Update()
    {
        t += Time.deltaTime;
    }
    public void HitWall(Vector3 pos, float range,float force,bool hard)
    {
        if (t < cooldown)
            return;
        t = 0;
        PV.RPC("RPC_HitWall", RpcTarget.All, pos, range, force,hard);
    }
    protected void DestroyAll()
    {
        Debug.Log("owo");
        List<Rigidbody> rbs = new List<Rigidbody>(pieces);
        foreach(Rigidbody r in rbs)
        {
            RemovePart(r, transform.position, 2, 400);
        }
    }
    protected void Repair()
    {
        pieces = new List<Rigidbody>(allPieces);
        foreach(Rigidbody r in pieces)
        {
            r.gameObject.SetActive(true);
            r.isKinematic = true;
        }
        onHit.Invoke();
    }
    public abstract void ResetCompletly();

    protected void StartInteraction()
    {
        //Play Animation
        PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerShooting>().HideWeapons();

        PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerMove>().DisablePlayer();
        PlayerManager.Instance.currentPlayerGameObject.GetComponent<Animator>().SetBool("Reinforcing", true);

    }
}
