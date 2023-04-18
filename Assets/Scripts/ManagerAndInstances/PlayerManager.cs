
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

    [SerializeField] GameObject teamChoseUI;
    [SerializeField] GameObject waitingUI;
    [SerializeField] GameObject spectatorManagerPrefab;
    
    
    public GameObject currentPlayerGameObject { get; private set; }
    public Team localPlayerTeam { get; private set; }
    
    
    private int spawnIndex;
    private PhotonView PV;
    private GameObject currSpectatorManager;

    public bool isAlive { get; private set; }
    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        if(PV.IsMine && Instance == null)
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

        //no need to show UI of team chose
        teamChoseUI.SetActive(false);
        localPlayerTeam = team;

        //Make an RPC call to game manager that we want to be added to thescoreboard
        PV.RPC("RPC_InitScoreboard", RpcTarget.All, PhotonNetwork.LocalPlayer, localPlayerTeam);

        //get position where player should spawn
        spawnIndex = SpawnManager.Instance.GetSpawnIndex(localPlayerTeam);

        //Spawn player if its warmup
        if(GameManager.Instance.currentGameState == GameState.Warmup)
            CreateController();

        //Call GameManager that client is ready
        GameManager.Instance.OnPlayerSetTeam();
    }


    [PunRPC]
    void RPC_InitScoreboard(Player p,Team t)
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
        //Stop spectating when starting to play
        if (currSpectatorManager != null)
            Destroy(currSpectatorManager);

        //Signal everyone that we are not dead anymore
        PV.RPC("RPC_ChangeAliveState", RpcTarget.All, true);
        //Signal GameManager to count us when checking if the round should finish
        GameManager.Instance.AddRemainingPlayer(localPlayerTeam);
        //Create new player with spawn info of PV who spawned it
        currentPlayerGameObject = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Player"), 
            getSpawnpoint(), 
            Quaternion.identity, 
            0,
            new object[] { PV.ViewID });
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
        if (localPlayerTeam == GameManager.Instance.currentTerroTeam)
            position = SpawnManager.Instance.redTeamSpawnpoints[spawnIndex].position;
        else
            position = SpawnManager.Instance.blueeamSpawnpoints[spawnIndex].position;
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
