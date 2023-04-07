
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager Instance;
    PhotonView PV;
    public GameObject teamChoseUI;
    public GameObject waitingUI;
    public GameObject currentPlayerGameObject { get; private set; }
    public Team localPlayerTeam { get; private set; }
    int spawnIndex;
    [SerializeField] GameObject spectatorManagerPrefab;
    private GameObject currSpectatorManager;
    public bool isAlive { get; private set; }
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        if(PV.IsMine)
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        localPlayerTeam = Team.Null;

        waitingUI.SetActive(PV.IsMine);

        GameManager.Instance.OnPlayerJoinedScene();
        //teamChoseUI.SetActive(PV.IsMine);
    }
    
    public void SetRedTeam()
    {
        SetTeam(Team.Red);
    }
    public void SetBlueTeam()
    {
        SetTeam(Team.Blue);
    }
    public void SetTeam(Team team)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        teamChoseUI.SetActive(false);
        localPlayerTeam = team;

        PV.RPC("RPC_IninScoreboard", RpcTarget.All, PhotonNetwork.LocalPlayer, localPlayerTeam);
        spawnIndex = SpawnManager.Instance.GetSpawnIndex(localPlayerTeam);
        if(GameManager.Instance.currentGameState == GameState.Warmup)
            CreateController();

        GameManager.Instance.OnPlayerSetTeam();
    }
    [PunRPC]
    void RPC_IninScoreboard(Player p,Team t)
    {
        if (!PV.IsMine)
            localPlayerTeam = t;

        
        GameManager.Instance.AddManagerToList(t, this);


        GameManager.Instance.AddPlayerstats(p,t);
    }
    public void Die()
    {
        Invoke("DestroyCurrentPlayer", 4);
    }
    void DestroyCurrentPlayer()
    {
        if(currentPlayerGameObject != null)
            PhotonNetwork.Destroy(currentPlayerGameObject);

        if (GameManager.Instance.currentGameState == GameState.Warmup || GameManager.Instance.currentGameState == GameState.WarmupEnd)
        {
            CreateController();
        }
        else
        {
            currSpectatorManager =  Instantiate(spectatorManagerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
    void CreateController()
    {
        if (currSpectatorManager != null)
            Destroy(currSpectatorManager);

        PV.RPC("RPC_ChangeAliveState", RpcTarget.All, true);
        GameManager.Instance.AddRemainingPlayer(localPlayerTeam);
        currentPlayerGameObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), getSpawnpoint(), Quaternion.identity, 0, new object[] { PV.ViewID }) ;
    }

    public void changeAliveState(bool newAliveState)
    {
        PV.RPC("RPC_ChangeAliveState", RpcTarget.All, newAliveState);
    }
    [PunRPC]
    void RPC_ChangeAliveState(bool newState)
    {
        isAlive = newState;
    }
    public void GenerateNewPlayer()
    {
        if (currentPlayerGameObject != null)
        {
            if (!isAlive)
            {
                CancelInvoke("DestroyCurrentPlayer");
                PhotonNetwork.Destroy(currentPlayerGameObject);
                CreateController();
            }
            else
            {
                currentPlayerGameObject.GetComponent<PlayerMove>().DisablePlayer();
                currentPlayerGameObject.transform.position = getSpawnpoint();
            }
        }
        else
        {
            CreateController();
        }
    }

    public void DisablePlayer()
    {
        if(currentPlayerGameObject != null)
        {
            currentPlayerGameObject.GetComponent<PlayerMove>().DisablePlayer();
        }
    }
    public void EnablePlayer()
    {
        if (currentPlayerGameObject != null)
        {
            currentPlayerGameObject.GetComponent<PlayerMove>().EnablePlayer();
        }
    }
    public void OnEndWarmup()
    {
        ResetHp();
    }
    public void DeleteAllWeapons()
    {
        if (currentPlayerGameObject != null)
            currentPlayerGameObject.GetComponent<PlayerWeaponSwap>().DeleteMyWeapons();
    }
    public void OnRoundReset()
    {
        DeleteBomb();
        ResetLean();
        currentPlayerGameObject.GetComponent<PlayerNetworkSetup>().UpdateOutline();
        currentPlayerGameObject.GetComponent<PlayerWeaponSwap>().TakeStartWeapon();
    }
    public void OnRoundStart()
    {
        if (currentPlayerGameObject != null)
            currentPlayerGameObject.GetComponent<PlayerUI>().CloseShop();
    }
    void ResetLean()
    {
        if (currentPlayerGameObject != null)
            currentPlayerGameObject.GetComponent<PlayerShooting>().ResetLean();
    }
    public void DeleteBomb()
    {
        if (currentPlayerGameObject != null)
            currentPlayerGameObject.GetComponent<PlayerBombPlant>().NoBombHereOwo();
    }
    public void ResetHp()
    {
        if (currentPlayerGameObject != null)
            currentPlayerGameObject.GetComponent<PlayerHp>().ResetHp();
    }
    Vector3 getSpawnpoint()
    {
        Vector3 position;
        //Vector3 position = SpawnManager.Instance.GetSpawnpoint(localPlayerTeam, spawnIndex);
        if (localPlayerTeam == GameManager.Instance.currentTerroTeam)
        {
            position = SpawnManager.Instance.redTeamSpawnpoints[spawnIndex].position;
        }
        else
        {
            position = SpawnManager.Instance.blueeamSpawnpoints[spawnIndex].position;

        }
        return position;
    }

    public void StartSpectatingThis()
    {
        Debug.Log("starting spectating!");
        if (currentPlayerGameObject != null)
        {
            currentPlayerGameObject.GetComponent<PlayerNetworkSetup>().SetSpectatorFpsView();
        }
    }

    public void StopSpectatingThis()
    {
        if (currentPlayerGameObject != null)
        {
            currentPlayerGameObject.GetComponent<PlayerNetworkSetup>().StopSpectating();
        }
    }

    public void SetPlayerGameObject(GameObject obj)
    {
        if (PV.IsMine)
            return;

        currentPlayerGameObject = obj;
    }
    public void onAllPlayersJoin()
    {
        waitingUI.SetActive(false);
        teamChoseUI.SetActive(true);
    }

    void RemovePlayerGameObject()
    {
        if (PV.IsMine)
            return;

        currentPlayerGameObject = null;
    }

    public void RefreshNicknameIcons()
    {
        if(currentPlayerGameObject!= null)
            currentPlayerGameObject.GetComponent<PlayerUI>().RefreshNicknameIcons();
    }

    public override void OnLeftRoom()
    {
        GameManager.Instance.RefreshIcons();
    }
}
