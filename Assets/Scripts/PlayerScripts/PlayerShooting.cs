using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
public class PlayerShooting : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] public Transform shotCam;
    [SerializeField] public Transform defaultShotCam;
    [SerializeField] private GameObject handGFX;
    [SerializeField] private GameObject normalCamera;
    [SerializeField] private GameObject aimCamera;
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private LayerMask shotLayers;
    [SerializeField] private GameObject pingPrefab;
    public Transform gunPos;
    public AudioClip hitmarkerSound;

    //Leaning
    [Header("Lean assignables")]
    [SerializeField] private Transform leanContainer;
    [SerializeField] private Vector3 leanOffsetPosition;
    [SerializeField] private Vector3 leanOffsetRotation;
    Vector3 desiredLeanPos;
    Vector3 desiredLeanRot;

    Vector3 defaultCamPosition;
    Vector3 defaultCamRotation;
    Vector3 currentLeanPos;
    Vector3 currentLeanRot;

    int currentLean = 0;
    float leanT;

    public float force = 50;


    [Header("Weapons")]
    public Weapon currentWeapon;
    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    public CinemachineImpulseSource shootingImpulse;

    [SerializeField]
    private string currentGrenade;


    public bool isReloading { get; private set; }

    //Private variables
    bool canShot = true;
    bool aiming;
    //Vector3 positionToLerp;
    float czas;

    //Refrences
    PlayerMove moveController;
    MouseLook mouseLookScript;
    PhotonView PV;
    Animator animator;
    PlayerNetworkSetup networkSetup;
    PlayerInteractHandle interact;
    PlayerUI playerUI;
    // Start is called before the first frame update
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        moveController = GetComponent<PlayerMove>();
        mouseLookScript = GetComponent<MouseLook>();
        animator = GetComponent<Animator>();
        networkSetup = GetComponent<PlayerNetworkSetup>();
        interact = GetComponent<PlayerInteractHandle>();
        defaultCamPosition = leanContainer.localPosition;
        defaultCamRotation = leanContainer.localRotation.eulerAngles;
        playerUI = GetComponent<PlayerUI>();
        
    }
    private void Start()
    {
        //aimVolume = GameObject.FindGameObjectWithTag("AimVolume").GetComponent<Volume>();
    }
    public void UpdateUI()
    {
        PV.RPC("RPC_UpdateUI", RpcTarget.All);
    }
    [PunRPC]
    void RPC_UpdateUI()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon == primaryWeapon)
            {
                playerUI.UpdateWeaponUI(primaryWeapon, secondaryWeapon);
            }
            else if (currentWeapon == secondaryWeapon)
            {
                playerUI.UpdateWeaponUI(secondaryWeapon, primaryWeapon);
            }
        }
        else
        {
            playerUI.UpdateWeaponUI(null, null);
        }
    }
    public void CancelReload()
    {
        StopAllCoroutines();
        isReloading = false;
    }
    // Update is called once per frame
    void Update()
    {
        Aiming();
        Lean();
        if (!PV.IsMine)
            return;
        if (GlobalUIManager.Instance.paused)
            return;

        //Grenade input
        if (Input.GetKeyDown(KeyCode.F) && currentGrenade != "" && interact.currInteract == null)
        {
            StartCoroutine(ThrowGrenade());
        }


        //marker input
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SpawnMarker();
        } 

        if (currentWeapon == null)
            return;
        FiringInput();

        //Reload inpit
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            UpdateUI();
        }
        //lean left
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PV.RPC("RPC_AddLean", RpcTarget.All, -1);
        }
        //lean right
        if (Input.GetKeyDown(KeyCode.E))
        {
            PV.RPC("RPC_AddLean", RpcTarget.All, 1);
        }

        AimingInput();
        AnimatorUpdate();
        czas += Time.deltaTime;
    }
    void SpawnMarker()
    {
        RaycastHit hit;
        Vector3 shotPos = aimCamera.transform.position + aimCamera.transform.forward * 0.5f;
        if (Physics.Raycast(shotPos, aimCamera.transform.forward, out hit, Mathf.Infinity, shotLayers))
        {
            PV.RPC("RPC_SpawnMarker", RpcTarget.All, PlayerManager.Instance.localPlayerTeam, hit.point);
        }
    }
    [PunRPC]
    void RPC_SpawnMarker(Team senderTeam, Vector3 pos)
    {
        if (PlayerManager.Instance.currentPlayerGameObject != null &&  senderTeam == PlayerManager.Instance.localPlayerTeam )
        {
            GameObject go = Instantiate(pingPrefab);
            PingIcon pi = go.GetComponent<PingIcon>();
            pi.Setup(pos);
            PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerUI>().AddPing(pi);
        }
    }
    void Lean()
    {
        if (leanT < 1)
        {
            leanContainer.localPosition = Vector3.Lerp(currentLeanPos, desiredLeanPos, leanT * 3);
            leanContainer.localEulerAngles = AngleLerp(currentLeanRot, desiredLeanRot, leanT * 3);
            leanT += Time.deltaTime;
        }
    }

    [PunRPC]
    void RPC_AddLean(int lean)
    {
        if (lean == currentLean)
            currentLean = 0;
        else
            currentLean = lean;

        leanT = 0;
        currentLeanPos = leanContainer.localPosition;
        currentLeanRot = leanContainer.localEulerAngles;

        desiredLeanPos = defaultCamPosition + new Vector3(leanOffsetPosition.x * currentLean, leanOffsetPosition.y * Mathf.Abs(currentLean), leanOffsetPosition.z * Mathf.Abs(currentLean));
        desiredLeanRot = defaultCamRotation + new Vector3(leanOffsetRotation.x * Mathf.Abs(currentLean), leanOffsetRotation.y * Mathf.Abs(currentLean), leanOffsetRotation.z * currentLean);
    }

    public void ResetLean()
    {
        PV.RPC("RPC_ResetLean", RpcTarget.All);
    }
    [PunRPC]
    void RPC_ResetLean()
    {
        currentLean = 0;
        leanContainer.localPosition = defaultCamPosition;
        leanContainer.localEulerAngles = defaultCamRotation;
    }

    #region GRENADES
    IEnumerator ThrowGrenade()
    {
        StartCoroutine(FirstPersonGrenadeAnim());
        PV.RPC("RPC_3rdPersonAnim", RpcTarget.All);

        yield return new WaitForSeconds(0.4f);
        SpawnNewGrenade();
    }

    [PunRPC]
    void RPC_3rdPersonAnim()
    {
        if (!PV.IsMine)
        {
            animator.Play("ThrowGrenade3rdPerson");
        }
    }

    IEnumerator FirstPersonGrenadeAnim()
    {
        HideWeapons();
        animator.Play("ThrowGrenade1");
        yield return new WaitForSeconds(1.5f);

        ShowWeapons();
    }
    public void HideWeapons()
    {
        canShot = false;
        handGFX.SetActive(true);
        gunPos.gameObject.SetActive(false);
    }
    public void ShowWeapons()
    {
        gunPos.gameObject.SetActive(true);
        handGFX.SetActive(false);
        canShot = true;
    }

    void SpawnNewGrenade()
    {
        object[] senderData = new object[]
        {
            PhotonNetwork.LocalPlayer
        };
        string grenadePath = Path.Combine("PhotonPrefabs", "Weapons", "Grenades", currentGrenade);

        PhotonNetwork.Instantiate(grenadePath, aimCamera.transform.position + aimCamera.transform.forward, aimCamera.transform.rotation, 0, senderData);
        PV.RPC("RPC_SetGrenade", RpcTarget.All, "");
    }



    #endregion

    #region AIMING
    void AimingInput()
    {
        if (currentWeapon == null)
            return;
        if (Input.GetMouseButton(1) && !aiming)
        {
            StartAiming();
        }
        if (Input.GetMouseButtonUp(1) || moveController.currentMoveState == MoveState.Running)
        {
            StopAiming();
        }
    }
    void Aiming()
    {
        if (currentWeapon == null)
            return;
        if (aiming)
        {
            gunPos.transform.localPosition = Vector3.Lerp(gunPos.transform.localPosition, currentWeapon.aimOffset, Time.deltaTime * 20);
            Vector3 angle = AngleLerp(gunPos.transform.localRotation.eulerAngles, currentWeapon.aimRotation, Time.deltaTime * 20);
            gunPos.transform.localEulerAngles = angle;
        }
        else
        {
            gunPos.transform.localPosition = Vector3.Lerp(gunPos.transform.localPosition, currentWeapon.hipfireOffset, Time.deltaTime * 20);
            Vector3 angle = AngleLerp(gunPos.transform.localRotation.eulerAngles, Vector3.zero, Time.deltaTime * 20);
            gunPos.transform.localEulerAngles = angle;
        }

    }
    public void HardStopAiming()
    {
        if(currentWeapon != null)
        {
            gunPos.transform.localPosition = currentWeapon.hipfireOffset;
            gunPos.transform.localEulerAngles = Vector3.zero;
        }

        if (PV.IsMine)
        {
            normalCamera.SetActive(true);
            aimCamera.SetActive(false);
        }
        if (currentWeapon != null)
            AnimatorUpdate();

        aiming = false;
        FindObjectOfType<WeaponCamera>().aiming = false;
        PV.RPC("RPC_ChangeAimingState", RpcTarget.All, aiming);

    }
    public void StopAiming()
    {
        if (currentWeapon == null)
            return;
        if (PV.IsMine)
        {
            FindObjectOfType<WeaponCamera>().aiming = false;
            normalCamera.SetActive(true);
            aimCamera.SetActive(false);
            //aimVolume.weight = 0;
        }
        aiming = false;
        PV.RPC("RPC_ChangeAimingState", RpcTarget.All, aiming);
    }
    public void StartAiming()
    {
        if (currentWeapon == null || isReloading)
            return;
        if (PV.IsMine)
        {
            FindObjectOfType<WeaponCamera>().aiming = true;
            normalCamera.SetActive(false);
            aimCamera.SetActive(true);
        }
        aiming = true;

        PV.RPC("RPC_ChangeAimingState", RpcTarget.All,aiming);
    }

    [PunRPC]
    void RPC_ChangeAimingState(bool state)
    {
        if(!PV.IsMine)
        {
            aiming = state;
            if (networkSetup.isSpectatingThis)
            {
                normalCamera.SetActive(!aiming);
                aimCamera.SetActive(aiming);
            }
        }
    }

    #endregion
    Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
    {
        float xLerp = Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t);
        float yLerp = Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t);
        float zLerp = Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t);
        Vector3 Lerped = new Vector3(xLerp, yLerp, zLerp);
        return Lerped;
    }
    public void ProceduralAnimation(float x, float y)
    {
        if (currentWeapon == null)
            return;
        if(aiming)
        {
            currentWeapon.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            Vector3 targetAngle = new Vector3(y * 7, -x * 7, 0);
            Vector3 angle = AngleLerp(currentWeapon.transform.localRotation.eulerAngles, targetAngle, Time.fixedDeltaTime * 5);
            currentWeapon.transform.localEulerAngles = angle;
        }
    }

    [PunRPC]
    void RPC_SetAnimationTrigger(string trigger)
    {
        if (PV.IsMine)
            return;
        currentWeapon.animator.SetTrigger(trigger);
    }
    IEnumerator Reload()
    {
        if (currentWeapon.currentAmmoOverall <= 0 || currentWeapon.currentAmmoMagazine == currentWeapon.bulletsInMagazine || currentWeapon.weaponType == WeaponType.Melee || isReloading || currentWeapon == null)
            yield break;
        //UpdateMultipliers();
        StopAiming();
        if (currentWeapon.currentAmmoMagazine == 0)
        {
            currentWeapon.animator.SetTrigger("ReloadNoAmmo");
            PV.RPC("RPC_SetAnimationTrigger", RpcTarget.All, "ReloadNoAmmo");
            PV.RPC("RPC_ReloadSoundEffect", RpcTarget.All,true);
        }
        else
        {
            currentWeapon.animator.SetTrigger("ReloadAmmo");
            PV.RPC("RPC_SetAnimationTrigger", RpcTarget.All, "ReloadAmmo");
            PV.RPC("RPC_ReloadSoundEffect", RpcTarget.All, false);
        }


        isReloading = true;

        int neededAmmo = currentWeapon.bulletsInMagazine - currentWeapon.currentAmmoMagazine;


        if (currentWeapon.weaponType == WeaponType.Shotgun)
        {
            int r = 0;

            if (currentWeapon.currentAmmoOverall >= neededAmmo)
            {
                r = neededAmmo;
            }
            else
            {
                r = currentWeapon.currentAmmoOverall;
            }
            currentWeapon.animator.SetInteger("ammoToReloadLeft", r);
            yield return new WaitForSeconds((currentWeapon.reloadTime) * r);
        }
        else
        {
            if (currentWeapon.currentAmmoOverall >= neededAmmo)
            {
                yield return new WaitForSeconds(currentWeapon.reloadTime);
            }
            else
            {
                yield return new WaitForSeconds(currentWeapon.noAmmoReloadTime);
            }
        }
        currentWeapon.animator.ResetTrigger("Shot");
        currentWeapon.animator.ResetTrigger("ReloadNoAmmo");
        currentWeapon.animator.ResetTrigger("ReloadAmmo");

        isReloading = false;

        if (currentWeapon.currentAmmoOverall >= neededAmmo)
        {
            currentWeapon.currentAmmoMagazine += neededAmmo;
            currentWeapon.currentAmmoOverall -= neededAmmo;
        }
        else
        {
            currentWeapon.currentAmmoMagazine += currentWeapon.currentAmmoOverall;
            currentWeapon.currentAmmoOverall = 0;
        }

        PV.RPC("RPC_UpdateAmmoOnOthers", RpcTarget.All, currentWeapon.currentAmmoMagazine, currentWeapon.currentAmmoOverall);

        UpdateUI();

    }
    [PunRPC]
    void RPC_UpdateAmmoOnOthers(int currentAmmo,int overallAmmo)
    {
        if (PV.IsMine)
            return;

        currentWeapon.currentAmmoMagazine = currentAmmo;
        currentWeapon.currentAmmoOverall = overallAmmo;

    }
    #region shooting
    void Shoot()
    {
        if (moveController.currentMoveState == MoveState.Running)
            return;
        if (!canShot)
            return;
        if (currentWeapon.currentAmmoMagazine <= 0)
        {
            if (!isReloading)
                StartCoroutine(Reload());
            return;
        }

        if (isReloading)
        {
            currentWeapon.animator.ResetTrigger("Shot");
            return;
        }

        currentWeapon.currentAmmoMagazine -= 1;

        currentWeapon.animator.SetTrigger("Shot");
        if (currentWeapon.weaponType == WeaponType.Shotgun)
        {
            for (int i = 0; i < 5; i++)
            {
                GenerateShot();
            }
        }
        else
        {
            GenerateShot();

        }


        float spreadx = UnityEngine.Random.Range(-currentWeapon.bulletSpread, currentWeapon.bulletSpread);
        float spready = UnityEngine.Random.Range(-currentWeapon.bulletSpread, currentWeapon.bulletSpread);

        shootingImpulse.GenerateImpulse();
        if (aiming)
        {
            mouseLookScript.AddRecoil(spreadx * 7, spready * 7);
        }
        else
        {
            mouseLookScript.AddRecoil(Mathf.Abs(spreadx * 3), spready * 3);
        }

    }

    void GenerateShot()
    {
        Vector3 aimpos = shotCam.forward;

        float spreadx = UnityEngine.Random.Range(-currentWeapon.bulletSpread, currentWeapon.bulletSpread);
        float spready = UnityEngine.Random.Range(-currentWeapon.bulletSpread, currentWeapon.bulletSpread);

        
        if (!aiming || currentWeapon.weaponType == WeaponType.Shotgun)
        {
            aimpos.x += spreadx;
            aimpos.y += spready;
        }
        Vector3 shotPos = shotCam.transform.position + shotCam.transform.forward * 0.1f;
        RaycastHit hit;

        PV.RPC("RPC_OnShoot", RpcTarget.All);
        //PV.RPC("RPC_ShotSoundEffect", RpcTarget.All);

        bool hitEnemy = false;
        if (Physics.Raycast(shotPos, aimpos, out hit, Mathf.Infinity, shotLayers))
        {
            DestructiblePart wall = hit.transform.GetComponent<DestructiblePart>();
            if(wall != null)
            {
                wall.Hit(hit.point, 0.05f, force);
            }
            PlayerCollider enemyCollider = hit.transform.GetComponent<PlayerCollider>();
            if (enemyCollider != null)
            {
                float distance = Vector3.Distance(transform.position, hit.point);

                float damage = currentWeapon.baseDamage * currentWeapon.damageCurve.Evaluate(distance);

                if (damage < 0)
                    damage = 0;
                //PlayHitParticlesClientRpc(hit.point.x, hit.point.y, hit.point.z);
                bool hitHead = false;

                enemyCollider.Damage(damage, PhotonNetwork.LocalPlayer, currentWeapon.weaponName,transform.position,out hitHead);
                hitEnemy = true;

                if (hitHead)
                    ShowHitmarker();

            }

            PV.RPC("RPC_HitParticles", RpcTarget.All,hitEnemy,hit.point,hit.normal);
        }


        UpdateUI();
    }

    void ShowHitmarker()
    {
        playerUI.ShowHitmarker();
        GetComponent<PlayerAudioManager>().PlayOtherSound(hitmarkerSound);
    }

    [PunRPC]
    void RPC_OnShoot()
    {
        if(!PV.IsMine)
            currentWeapon.currentAmmoMagazine -= 1;

        //play muzzle flash
        if (PV.IsMine || networkSetup.isSpectatingThis)
        {
            currentWeapon.MuzzleFlashLocal.Play();
        }
        else
        {
            currentWeapon.MuzzleFlash.Play();
        }

        //Play sound
        ShotSoundEffect();
    }
    [PunRPC]
    void RPC_HitParticles(bool hitEnemy,Vector3 position,Vector3 normal)
    {
        currentWeapon.bulletParticles.transform.LookAt(position);
        currentWeapon.bulletParticles.Play();

        GameObject objToSpawn = currentWeapon.wallBulletImpact;
        if(objToSpawn == null)
        {
            objToSpawn = WeaponManager.Instance.defaultHitWallParticles;
        }

        if(hitEnemy)
        {
            objToSpawn = currentWeapon.hitEnemyParticles;
            if (objToSpawn == null)
                objToSpawn = WeaponManager.Instance.defaultHitEnemyParticles;
        }

        Instantiate(objToSpawn,position,Quaternion.LookRotation(normal));
    }



    #endregion
    #region audio
    void ShotSoundEffect()
    {
        GetComponent<PlayerAudioManager>().FireSound(currentWeapon.shotClip);
    }

    [PunRPC]
    void RPC_ReloadSoundEffect(bool noAmmoReload)
    {
        if(noAmmoReload)
            GetComponent<PlayerAudioManager>().PlayOtherSound(currentWeapon.reloadClip_NoAmmo,SoundType.Gun);
        else
            GetComponent<PlayerAudioManager>().PlayOtherSound(currentWeapon.reloadClip_Ammo, SoundType.Gun);

    }
    #endregion
    #region input
    void FiringInput()
    {
        if (currentWeapon == null)
            return;
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Single:
                SingleFireInput();
                break;
            case WeaponType.Shotgun:
                SingleFireInput();
                break;
            case WeaponType.AsFastAsClick:
                AsFastAsClickFireInput();
                break;
            case WeaponType.Burst:
                BurstFireInput();
                break;
            case WeaponType.FullAuto:
                FullAutoInput();
                break;
            case WeaponType.Melee:
                MeleeInput();
                break;

        }
    }
    void SingleFireInput()
    {
        if (Input.GetMouseButtonDown(0) && czas > currentWeapon.fireDecay)
        {
            Shoot();
            czas = 0;
        }
    }
    void AsFastAsClickFireInput()
    {
        if (Input.GetMouseButtonDown(0) && czas > 0.1f)
        {
            Shoot();
            czas = 0;
        }
    }
    void BurstFireInput()
    {
        if (Input.GetMouseButton(0) && czas > currentWeapon.fireDecay)
        {
            StartCoroutine(burstFire());
            czas = 0;
        }
    }
    IEnumerator burstFire()
    {
        for (int i = 0; i < 3; i++)
        {
            Shoot();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FullAutoInput()
    {
        if (Input.GetMouseButton(0) && czas > currentWeapon.fireDecay)
        {
            Shoot();
            czas = 0;
        }
    }
    void MeleeInput()
    {
        /*
        if(Input.GetMouseButtonDown(0) && czas > currentWeapon.fireDecay)
        {
            currentWeapon.animator.SetTrigger("Shot");
            currentWeapon.animator.SetInteger("AttackType", Random.Range(0, currentWeapon.stats.attackAnimations));
            Invoke("MeleeFire",0.4f);
            czas = 0;
        }*/
    }
    #endregion
    #region Spectating
    public void StartSpectating()
    {
        if (currentWeapon != null)
        {
            if(currentWeapon.shotCam != null)
                shotCam.gameObject.SetActive(true);

            MyExstenstions.SetLayerRecursively(currentWeapon.gameObject, LayerMask.NameToLayer("LocalWeapon"));
            currentWeapon.MuzzleFlashLocal.gameObject.layer = 0;
        }

        if (aiming)
            aimCamera.SetActive(true);
        else
            normalCamera.SetActive(true);


    }
    public void StopSpectating()
    {
        
        if (currentWeapon != null)
        {
            if(currentWeapon.shotCam != null)
                shotCam.gameObject.SetActive(false);

            MyExstenstions.SetLayerRecursively(currentWeapon.gameObject, 0);
            currentWeapon.handsMesh.layer = LayerMask.NameToLayer("DontSee");
        }


        aimCamera.SetActive(false);
        normalCamera.SetActive(false);
    }
    #endregion
    public void AnimatorUpdate()
    {
        if (currentWeapon != null)
        {
            currentWeapon.animator.SetBool("isAiming", aiming);
            currentWeapon.animator.SetBool("isRunning", (moveController.currentMoveState == MoveState.Running));
            currentWeapon.animator.SetBool("isWalking", moveController.isWalking());
            currentWeapon.animator.SetBool("isCrouching", (moveController.currentMoveState == MoveState.Crouching));
        }

    }
    public void SetGrenade(string gren)
    {
        PV.RPC("RPC_SetGrenade", RpcTarget.All, gren);
    }

    [PunRPC]
    void RPC_SetGrenade(string g)
    {
        currentGrenade = g;
        playerUI.UpdateGrenade(g);
    }
}
