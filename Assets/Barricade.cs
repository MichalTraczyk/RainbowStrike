using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Barricade : MonoBehaviourPunCallbacks
{
    PhotonView PV;
    public List<WindowPart> allParts;
    public List<WindowPart> activeParts;
    public int neededToDestroy;

    public bool destroyedAtStart = false;
    public bool jumpable = true;
    public GameObject jumpCollider;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    private void Start()
    {
        activeParts = new List<WindowPart>(allParts);

        if (destroyedAtStart)
            DestroyThis();


    }
    public void OnHit(WindowPart p)
    {
        int index = allParts.IndexOf(p);
        PV.RPC("RPC_OnHit",RpcTarget.All,index);
    }
    [PunRPC]
    void RPC_OnHit(int indexOf)
    {
        Debug.Log("hit!");
        WindowPart wp = activeParts[indexOf];
        wp.gameObject.SetActive(false);
        activeParts.RemoveAt(indexOf);
        Debug.Log("On hit: " + activeParts.Count + " " + neededToDestroy + "");
        if (activeParts.Count + neededToDestroy <= allParts.Count)
        {
            DestroyThis();
        }
    }
    void DestroyThis()
    {
        foreach (WindowPart go in activeParts)
        {
            go.gameObject.SetActive(false);
            if(jumpable)
                jumpCollider.SetActive(true);
        }
    }


    void Repair()
    {
        foreach (WindowPart go in activeParts)
        {
            go.gameObject.SetActive(true);
            if (jumpable)
                jumpCollider.SetActive(false);
        }
    }
    public void addPart(WindowPart p)
    {
        allParts.Add(p);
    }
    
}
