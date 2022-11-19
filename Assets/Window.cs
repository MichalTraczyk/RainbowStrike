using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Window : MonoBehaviourPunCallbacks
{
    PhotonView PV;
    public List<WindowPart> parts;
    public int neededToDestroy;
    int startCount;
    public GameObject jumpCollider;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    private void Start()
    {
        startCount = parts.Count;
    }
    public void OnHit(WindowPart p)
    {
        int index = parts.IndexOf(p);
        PV.RPC("RPC_OnHit",RpcTarget.All,index);
    }
    [PunRPC]
    void RPC_OnHit(int indexOf)
    {
        Debug.Log("hit!");
        WindowPart wp = parts[indexOf];
        wp.gameObject.SetActive(false);
        parts.RemoveAt(indexOf);

        if (parts.Count + neededToDestroy <= startCount)
        {
            foreach (WindowPart go in parts)
            {
                go.gameObject.SetActive(false);
                jumpCollider.SetActive(true);
            }
        }
    }


    public void addPart(WindowPart p)
    {
        parts.Add(p);
    }
    
}
