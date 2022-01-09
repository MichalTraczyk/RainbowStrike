using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIKHandle : MonoBehaviour
{
    PlayerShooting weaponHandle;
    public Transform IKtargetR;
    public Transform IKtargetL;
    public Transform spineTarget;

    Transform RightHandPos;
    Transform LeftHandPos;
    public Transform gunPos;


    private void Awake()
    {
        weaponHandle = GetComponent<PlayerShooting>();
    }
    void Update()
    {
        if (RightHandPos != null)
        {
            IKtargetR.position = RightHandPos.position;
            IKtargetR.rotation = RightHandPos.rotation;
        }

        if (LeftHandPos != null)
        {
            IKtargetL.position = LeftHandPos.position;
            IKtargetL.rotation = LeftHandPos.rotation;
        }
        spineTarget.position = gunPos.position;

    }
    public void SetTarget()
    {
        if (weaponHandle.currentWeapon == null)
            return;
        LeftHandPos = weaponHandle.currentWeapon.leftHandIK;
        RightHandPos = weaponHandle.currentWeapon.rightHandIK;
    }
}
