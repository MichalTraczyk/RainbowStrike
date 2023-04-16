using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.IO;

public class PlayerWeaponSwap : MonoBehaviourPunCallbacks
{

    public PlayerShooting shootingHandle;

    PhotonView PV;
    // Start is called before the first frame update
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    private void Start()
    {
        TakeStartWeapon();
    }
    void Update()
    {
        if (!PV.IsMine || GlobalUIManager.Instance.paused)
            return;
        if (shootingHandle.currentWeapon == null)
            return;

        if (Input.GetKeyDown(KeyCode.X) && !shootingHandle.isReloading)
        {
            SwapWeapon();
            shootingHandle.CancelReload();
        }
        if (Input.GetKeyDown(KeyCode.G) && GetComponent<PlayerBombPlant>().hasBomb == false)
        {
            DropWeapon(true);
            shootingHandle.CancelReload();
        }
    }
    public void TakeStartWeapon()
    {
        if (shootingHandle.currentWeapon != null)
            return;
        if (PV.IsMine)
            TrySetWeapon("Makarov", 0, PhotonNetwork.LocalPlayer);
    }
    public void DropWeapon(bool addForce = false)
    {
        if (shootingHandle.currentWeapon == null)
            return;
        WeaponSlot s;
        if (shootingHandle.currentWeapon == shootingHandle.primaryWeapon)
            s = WeaponSlot.Primary;
        else
            s = WeaponSlot.Secondary;
        PV.RPC("RPC_DropWeaponOwner", RpcTarget.All, s,addForce);
        GetComponent<PlayerShooting>().UpdateUI();
    }
    public void DeleteMyWeapons()
    {
        if (shootingHandle.primaryWeapon != null)
            shootingHandle.primaryWeapon.DestroyThis();
        if (shootingHandle.secondaryWeapon != null)
            shootingHandle.secondaryWeapon.DestroyThis();

        GetComponent<PlayerShooting>().UpdateUI();
        shootingHandle.CancelReload();
    }
    [PunRPC]
    void RPC_DropWeaponOwner(WeaponSlot slot,bool addForce)
    {
        if (!PV.IsMine)
            return;

        
        string name;
        int ammoMag;
        int ammoOffMag;
        Scope s;
        //Destroy current weapon and save ammo to variables
        if(slot == WeaponSlot.Primary)
        {
            name = shootingHandle.primaryWeapon.weaponName;
            ammoMag = shootingHandle.currentWeapon.currentAmmoMagazine;
            ammoOffMag = shootingHandle.primaryWeapon.currentAmmoOverall;
            s = shootingHandle.primaryWeapon.currentScope;
            PhotonNetwork.Destroy(shootingHandle.primaryWeapon.gameObject);
        }
        else
        {
            name = shootingHandle.secondaryWeapon.weaponName;
            ammoMag = shootingHandle.secondaryWeapon.currentAmmoMagazine;
            ammoOffMag = shootingHandle.secondaryWeapon.currentAmmoOverall;
            s = shootingHandle.secondaryWeapon.currentScope;
            PhotonNetwork.Destroy(shootingHandle.secondaryWeapon.gameObject);
        }




        //Update shooting script current weapon
        if (slot == WeaponSlot.Primary)
        {
            if (shootingHandle.primaryWeapon == shootingHandle.currentWeapon)
                shootingHandle.currentWeapon = null;
            shootingHandle.primaryWeapon = null;
        }
        else
        {
            if (shootingHandle.secondaryWeapon == shootingHandle.currentWeapon)
                shootingHandle.currentWeapon = null;
            shootingHandle.secondaryWeapon = null;
        }
        //Swap weapon after dropping it
        SwapWeapon();

        //Create data to send to weapon
        string weaponName = name + "_ToTake";
        string weaponPath = Path.Combine("PhotonPrefabs", "Weapons", weaponName);
        object[] data = new object[]
        {
            ammoMag,
            ammoOffMag,
            s,
            addForce
        };
        //Update ui
        GetComponent<PlayerShooting>().UpdateUI();
        //Spawn weapon
        PhotonNetwork.Instantiate(weaponPath, shootingHandle.gunPos.position, shootingHandle.gunPos.rotation, 0, data);

    }
    public void TrySetWeapon(string weaponName, int price, Player player, Scope scope = Scope.Iron, int ammo = -1,int overallAmmo = -1)
    {
        PV.RPC("RPC_TrySetWeaponOwner", PV.Owner, weaponName, price, player, scope, ammo, overallAmmo);
    }
    [PunRPC]
    void RPC_TrySetWeaponOwner(string weaponName, int price, Player player, Scope scope, int ammo, int overallAmmo)
    {
        //if (!PV.IsMine)
           // return;
        if (price > GameManager.Instance.getStatsByPlayer(PhotonNetwork.LocalPlayer).money)
            return;

        GameManager.Instance.ChangeMoney(PhotonNetwork.LocalPlayer, -price);
        GetComponent<PlayerUI>().RefreshMoneyText();
        WeaponSlot s = WeaponManager.Instance.GetWeaponByName(weaponName).GetComponent<Weapon>().slot;


        if ((s == WeaponSlot.Primary && shootingHandle.primaryWeapon != null) || (s == WeaponSlot.Secondary && shootingHandle.secondaryWeapon != null))
            PV.RPC("RPC_DropWeaponOwner", RpcTarget.All, s,false);


        string path = Path.Combine("PhotonPrefabs", "Weapons", weaponName);
        GameObject t = PhotonNetwork.Instantiate(path, Vector3.zero, Quaternion.identity);
        int viewIdd = t.GetComponent<PhotonView>().ViewID;

        PV.RPC("RPC_SetParent", RpcTarget.All, viewIdd, scope, player, ammo, overallAmmo);
    }

    [PunRPC]
    void RPC_SetParent(int viewId,Scope scp,Player player,int ammo,int overallAmmo)
    {
        Transform t = PhotonView.Find(viewId).transform;
        t.SetParent(shootingHandle.gunPos);

        float size = WeaponManager.Instance.size;
        t.localScale = new Vector3(size, size, size);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        shootingHandle.gunPos.localPosition = t.GetComponent<Weapon>().hipfireOffset;

        if (!PV.IsMine)
        {
            t.GetComponent<Weapon>().handsMesh.layer = LayerMask.NameToLayer("DontSee");
        }
        else
        {
            t.GetComponent<Weapon>().SetLayersToLocalWeapon();
        }

        WeaponSlot s = t.GetComponent<Weapon>().slot;
        if(s == WeaponSlot.Primary)
        {
            if(shootingHandle.primaryWeapon != null)
                Destroy(shootingHandle.primaryWeapon.gameObject);

            shootingHandle.primaryWeapon = t.GetComponent<Weapon>();
            shootingHandle.currentWeapon = shootingHandle.primaryWeapon;

            if (shootingHandle.secondaryWeapon != null)
                shootingHandle.secondaryWeapon.transform.gameObject.SetActive(false);
        }
        else
        {
            if (shootingHandle.secondaryWeapon != null)
                Destroy(shootingHandle.secondaryWeapon.gameObject);
            shootingHandle.secondaryWeapon = t.GetComponent<Weapon>();
            shootingHandle.currentWeapon = shootingHandle.secondaryWeapon;

            if (shootingHandle.primaryWeapon != null)
                shootingHandle.primaryWeapon.transform.gameObject.SetActive(false);
        }

        if(ammo != -1 && overallAmmo != -1)
        {
            shootingHandle.currentWeapon.currentAmmoMagazine = ammo;
            shootingHandle.currentWeapon.currentAmmoOverall = overallAmmo;
        }

        shootingHandle.UpdateUI();
        UpdateIK();
        UpdateShotCam();
        if(PV.IsMine && scp != Scope.Iron)
        {
          SetScope(scp, player);
        }
    }
    public void SetScope(Scope s,Player p)
    {
        PV.RPC("RPC_SetScope", RpcTarget.All, s, p);
    }
    [PunRPC]
    void RPC_SetScope(Scope s, Player p)
    {
        if (shootingHandle.currentWeapon != null)
            shootingHandle.currentWeapon.SetScope(s, shootingHandle, p);
    }
    #region weapon swapping
    void SwapWeapon()
    {
        PV.RPC("RPC_WeaponSwap", RpcTarget.All);
    }
    [PunRPC]
    void RPC_WeaponSwap()
    {
        if (shootingHandle.currentWeapon == shootingHandle.primaryWeapon && shootingHandle.secondaryWeapon != null)
        {
            shootingHandle.secondaryWeapon.transform.gameObject.SetActive(true);
            shootingHandle.currentWeapon = shootingHandle.secondaryWeapon;
            if (shootingHandle.primaryWeapon != null)
                shootingHandle.primaryWeapon.transform.gameObject.SetActive(false);
        }
        else if (shootingHandle.currentWeapon == shootingHandle.secondaryWeapon && shootingHandle.primaryWeapon != null)
        {
            //Turn on gfx
            shootingHandle.primaryWeapon.transform.gameObject.SetActive(true);
            //Set shooting variables
            shootingHandle.currentWeapon = shootingHandle.primaryWeapon;
            //Disable secondary weapon
            if (shootingHandle.secondaryWeapon != null)
                shootingHandle.secondaryWeapon.transform.gameObject.SetActive(false);
        }
        else if(shootingHandle.currentWeapon == null)
        {
            if(shootingHandle.primaryWeapon != null)
            {
                //Turn on gfx
                shootingHandle.primaryWeapon.transform.gameObject.SetActive(true);

                //Set shooting variables
                shootingHandle.currentWeapon = shootingHandle.primaryWeapon;
            }
            else if(shootingHandle.secondaryWeapon != null)
            {
                shootingHandle.secondaryWeapon.transform.gameObject.SetActive(true);
                shootingHandle.currentWeapon = shootingHandle.secondaryWeapon; 
            }
        }

        shootingHandle.StopAiming();
        UpdateShotCam();
        UpdateIK();
        shootingHandle.UpdateUI();
    }
    public void UpdateIK()
    {
        GetComponent<PlayerIKHandle>().SetTarget();
    }
    #endregion
    public void UpdateShotCam()
    {
        if (shootingHandle.currentWeapon == null)
            return;
        if (shootingHandle.currentWeapon.shotCam == null)
            shootingHandle.shotCam = shootingHandle.defaultShotCam;
        else
            shootingHandle.shotCam = shootingHandle.currentWeapon.shotCam;
    }


    public void TrySetGrenade(string grenade,int price)
    {
        if (price > GameManager.Instance.getStatsByPlayer(PhotonNetwork.LocalPlayer).money)
            return;


        GameManager.Instance.ChangeMoney(PhotonNetwork.LocalPlayer, -price);
        shootingHandle.SetGrenade(grenade);
        GetComponent<PlayerUI>().RefreshMoneyText();

    }
}
