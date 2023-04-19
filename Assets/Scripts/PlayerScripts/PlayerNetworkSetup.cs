using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using System;
using Photon.Realtime;

public class PlayerNetworkSetup : MonoBehaviour
{
    public GameObject playerThirdpersonMesh;
    public GameObject UI;
    public GameObject PlayerHands;
    PhotonView PV;
    public PlayerManager playerManager { get; private set; }

    public Material blueTeamMat;
    public Material redTeamMat;

    public GameObject firstPersonCam;

    public bool isSpectatingThis { get; private set; }

    public GameObject[] hardcoreModeDeactivate;

    public CinemachineVirtualCamera playerCam;
    public Transform localShotCollidersParent;

    public Team thisPlayerTeam { get; private set; }
    public string thisPlayerNickname { get; private set; }
    // Start is called before the first frame update
    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        int Id = (int)PV.InstantiationData[0];
        PhotonView managerObj = PhotonView.Find(Id);
        playerManager = managerObj.GetComponent<PlayerManager>();
        playerManager.SetPlayerGameObject(this.gameObject);

    }
    void UpdateSettings()
    {
        playerCam.m_Lens.FieldOfView = PlayerSettings.Instance.FieldOfView;
    }

    void Start()
    {
        PlayerSettings.Instance.OnSettingsChanged += UpdateSettings;
        UpdateSettings();
        if (PV.IsMine)
        {
            playerThirdpersonMesh.layer = LayerMask.NameToLayer("DontSee");
            PlayerHands.layer = LayerMask.NameToLayer("LocalWeapon");

            Team t = playerManager.localPlayerTeam;
            PV.RPC("RPC_SetEverything", RpcTarget.All, t, PhotonNetwork.NickName);

            if(GameManager.Instance.currentGameState == GameState.RoundPrepare)
            {
                GetComponent<PlayerMove>().DisablePlayer();
            }

            if(GameManager.Instance.hardcoreMode)
            {
                foreach(GameObject ob in hardcoreModeDeactivate)
                {
                    ob.SetActive(false);
                }
            }

            Collider[] localShotColliders = localShotCollidersParent.GetComponentsInChildren<Collider>();
            foreach (Collider c in localShotColliders)
            {
                c.enabled = false;
            }
            GameManager.Instance.RefreshIcons();

            GameManager.Instance.ChangeDeadState(PV.Owner, false);
        }
        else
        {
            PlayerHands.layer = LayerMask.NameToLayer("DontSee");
            UI.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        PlayerSettings.Instance.OnSettingsChanged -= UpdateSettings;
        GameManager.Instance.RefreshIcons();
    }
    public void SetSpectatorFpsView()
    {
        isSpectatingThis = true;
        GetComponent<PlayerShooting>().StartSpectating();
        UI.SetActive(true);

        playerThirdpersonMesh.layer = LayerMask.NameToLayer("DontSee");
        PlayerHands.layer = LayerMask.NameToLayer("LocalWeapon");
    }
    public void StopSpectating()
    {
        isSpectatingThis = false;
        GetComponent<PlayerShooting>().StopSpectating();

        UI.SetActive(false);
        PlayerHands.layer = LayerMask.NameToLayer("DontSee");
        playerThirdpersonMesh.layer = 0;
    }

    [PunRPC]
    void RPC_SetEverything(Team t,string nickname)
    {
        thisPlayerTeam = t;
        thisPlayerNickname = nickname;

        if(t==Team.Red)
        {
            playerThirdpersonMesh.GetComponent<SkinnedMeshRenderer>().material = redTeamMat;
        }
        else if(t==Team.Blue)
        {
            playerThirdpersonMesh.GetComponent<SkinnedMeshRenderer>().material = blueTeamMat;
        }
    }

    public void UpdateOutline()
    {
        PV.RPC("RPC_CheckOutline", RpcTarget.All, playerManager.localPlayerTeam);
    }
    [PunRPC]
    void RPC_CheckOutline(Team t)
    {
        GetComponent<Outline>().enabled = (t == PlayerManager.Instance.localPlayerTeam);
    }
}
