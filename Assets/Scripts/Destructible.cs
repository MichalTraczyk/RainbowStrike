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


    public GameObject notDamagedWallGO;
    public GameObject partsParent;
    private bool hitAlready;

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
        ShowGoodWall();
    }
    protected void HitWallOnParent(Vector3 pos, float range, float force)
    {
        if(!hitAlready)
        {
            HideGoodWall();
        }
        Debug.Log("Szukam!");
        Collider[] c = Physics.OverlapSphere(pos, range, wallLayers);
        Debug.Log("Znalazlem: " + c.Length);
        foreach (Collider col in c)
        {
            Rigidbody r = col.GetComponent<Rigidbody>();
            if (pieces.Contains(r))
            {
                RemovePart(r, pos, range, force);
            }
        }
    }

    void HideGoodWall()
    {
        notDamagedWallGO.SetActive(false);
        partsParent.SetActive(true);
        hitAlready = true;
    }
    protected void ShowGoodWall()
    {
        hitAlready = false;
        notDamagedWallGO.SetActive(true);
        partsParent.SetActive(false);
    }

    void RemovePart(Rigidbody r,Vector3 pos,float range, float force)
    {
        Debug.Log("3");
        r.isKinematic = false;
        pieces.Remove(r);

        StartCoroutine(ResetPart(r, r.position,r.rotation));
        r.AddExplosionForce(force, pos, range,0,ForceMode.Acceleration);
        onHit.Invoke();
    }
    IEnumerator ResetPart(Rigidbody part,Vector3 startPos,Quaternion startRot)
    {
        yield return new WaitForSeconds(2);

        part.isKinematic = true;
        part.transform.position = startPos;
        part.transform.rotation = startRot;

        if(GameManager.Instance.currentGameState == GameState.RoundEnd || GameManager.Instance.currentGameState == GameState.RoundPrepare)
            part.gameObject.SetActive(true);
        else
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
        HideGoodWall();
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
        }

        onHit.Invoke();
        ShowGoodWall();
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
