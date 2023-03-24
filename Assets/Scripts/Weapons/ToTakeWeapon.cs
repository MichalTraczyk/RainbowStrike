using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ToTakeWeapon : MonoBehaviour
{
    public string weaponName;
    Scope scope = Scope.Iron;
    public GameObject adv;
    public GameObject holo;
    public GameObject sniper;
    PhotonView PV;
    int magazineAmmo =-1;
    int maxAmmo = -1;


    private void Start()
    {
        string txt = "Press F to take the " + weaponName;
        GetComponent<Interactable>().MessageTosShow = txt;
        PV = GetComponent<PhotonView>();

        bool addForce = false;
        object[] data = PV.InstantiationData;
        if(data != null)
        {
            magazineAmmo = (int)data[0];
            maxAmmo = (int)data[1];
            scope = (Scope)data[2];
            addForce = (bool)data[3];
        }
        if(addForce)
        {
            //Weapon was throw off to someone
            GetComponent<Rigidbody>().velocity = transform.forward* 5;
       
            //Add random torque so weapon looks cool uwu
            GetComponent<Rigidbody>().AddTorque(50, 25, 50);
        }
        switch (scope)
        {
            case Scope.Iron:
                break;
            case Scope.Advanced:
                adv.SetActive(true);
                break;
            case Scope.Holo:
                holo.SetActive(true);
                break;
            case Scope.Sniper:
                sniper.SetActive(true);
                break;
        }
    }
    public void PickUpWeapon()
    {
        GetComponent<Interactable>().owner.GetComponent<PlayerWeaponSwap>().TrySetWeapon(weaponName, 0, PhotonNetwork.LocalPlayer, scope, magazineAmmo, maxAmmo);
        DestroyThis();
    }    
    [PunRPC]
    void RPC_DestroyThis()
    {
        if(PV.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }

    }
    public void DestroyThis()
    {
        PV.RPC("RPC_DestroyThis", RpcTarget.All);
    }
}
