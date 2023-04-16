using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public enum Scope
{
    Iron,
    Advanced,
    Holo,
    Sniper
}
public enum WeaponType
{
    Single,
    AsFastAsClick,
    Burst,
    FullAuto,
    Shotgun,
    Melee
}
public enum WeaponSlot
{
    Primary,
    Secondary
}
[System.Serializable]
public class WeaponInfo
{
    public GameObject prefab;
    public Vector3 AimOffset;
    public Vector3 AimRotation;
    public Transform shotCam;
}
public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public PlayerShooting owner;
    public string weaponName;
    [Header("Weapon stats")]
    public WeaponSlot slot;
    public WeaponType weaponType;
    public int bulletsInMagazine = 30;
    public int currentAmmoOverall;
    public int currentAmmoMagazine;
    public float reloadTime = 3;
    public float noAmmoReloadTime = 3;
    public float bulletSpread = 0.02f;
    public float baseDamage = 40;
    public AnimationCurve damageCurve;
    public float fireDecay = 0.1f;
    public Scope currentScope = Scope.Iron;
    //public WeaponStats stats;
    [Header("Weapon Positions")]
    public Vector3 hipfireOffset;

    [HideInInspector]
    public Transform shotCam;

    //[Header("read aim")]
    public Vector3 aimOffset;
    public Vector3 aimRotation;

    public WeaponInfo IronSight;
    public WeaponInfo HoloScope;
    public WeaponInfo advancedScope;
    public WeaponInfo sniperScope;


    [Header("Nice look")]
    public Sprite icon;
    public ParticleSystem bulletParticles;
    public GameObject weaponMesh;
    public ParticleSystem MuzzleFlash;
    public ParticleSystem MuzzleFlashLocal;
    public GameObject hitEnemyParticles;

    public Animator animator;


    public GameObject wallBulletImpact;

    public Transform leftHandIK;
    public Transform rightHandIK;

    public GameObject handsMesh;

    [Header("Audio")]
    public AudioClip shotClip;
    public AudioClip reloadClip_NoAmmo;
    public AudioClip reloadClip_Ammo;


    private PhotonView PV;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    public void SetScope(Scope s, PlayerShooting h, Player p)
    {
        WeaponInfo w = new WeaponInfo();

        if (sniperScope.prefab != null)
            sniperScope.prefab.SetActive(false);
        if (HoloScope.prefab != null)
            HoloScope.prefab.SetActive(false);
        if (advancedScope.prefab != null)
            advancedScope.prefab.SetActive(false);

        switch (s)
        {
            case Scope.Iron:
                currentScope = Scope.Iron;
                w = IronSight;
                break;
            case Scope.Advanced:
                currentScope = Scope.Advanced;
                w = advancedScope;
                break;
            case Scope.Holo:
                currentScope = Scope.Holo;
                w = HoloScope;
                break;
            case Scope.Sniper:
                currentScope = Scope.Sniper;
                w = sniperScope;
                break;
        }
        if (w.prefab != null)
            w.prefab.SetActive(true);

        aimOffset = w.AimOffset;
        aimRotation = w.AimRotation;
        shotCam = w.shotCam;

        if (w.shotCam == null)
            h.shotCam = h.defaultShotCam;
        else
            h.shotCam = w.shotCam;


        if (p != PhotonNetwork.LocalPlayer)
        {
            if (w.shotCam != null)
                w.shotCam.gameObject.SetActive(false);
        }
        else
        {
            SetLayersToLocalWeapon();
        }
    }
    public void SetLayersToLocalWeapon()
    {
        MyExstenstions.SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("LocalWeapon"));
        gameObject.layer = LayerMask.NameToLayer("LocalWeapon");
        MuzzleFlashLocal.gameObject.layer = 0;
    }


    [PunRPC]
    void RPC_DestroyThis()
    {
        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }

    }
    public void DestroyThis()
    {
        PV.RPC("RPC_DestroyThis", RpcTarget.All);
    }
}