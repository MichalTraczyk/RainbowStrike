using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState
{
    Warmup,
    WarmupEnd,
    Round,
    RoundPrepare,
    RoundEnd,
    PlantedBomb
}
[System.Serializable]
public class PlayerStats
{
    public Player player;
    public Team team;
    public int deaths;
    public int kills;
    public int headshots;
    public int assists;
    public int money;
}
public struct playerStatsStruct
{
    public string Nickname;
    public Team Team;
    public int Deaths;
    public int Kills;
    public int Assists;
    public int Money;
    public int Score;

    public playerStatsStruct(string nickname, int kills, int deaths, int assists, Team team, int money,int score)
    {
        Nickname = nickname;
        Kills = kills;
        Deaths = deaths;
        Assists = assists;
        Team = team;
        Money = money;
        Score = score;
    }

}
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [Header("Game settings")]
    public float timeBeforeRoundStart;
    public float roundTime;
    public float timeAfterRoundIsOver;
    public float timeToPlant;
    public int winRoundMoney;
    public int loseRoundMoney;
    public int TargetRounds;

    //Private variables
    private List<PlayerStats> playerStats = new List<PlayerStats>();
    private PhotonView PV;
    int remainingRedPlayers;
    int remainingBluePlayers;
    
    //Round Winner
    //public GameObject winnerUI;
    public Transform winnerUIParent;

    //=======================================================
    //READABLE VARIABLES
    //=======================================================
    [SerializeField]
    public GameState currentGameState { get; private set; }
    public int redTeamPoints { get; private set; }
    public int blueTeamPoints { get; private set; }
    public Team currentTerroTeam { get; private set; }
    public float roundRemainingTime { get; private set; }

    public bool hardcoreMode { get; private set; }
    public List<PlayerManager> redTeamPlayerManagers { get; private set; }
    public List<PlayerManager> blueTeamPlayerManagers { get; private set; }


    public Transform bombSpawnPos;
    int readyPlayers = 0;

    List<Team> wonRoundsOrder = new List<Team>();

    private void Start()
    {
        currentGameState = GameState.Warmup;
        roundRemainingTime = roundTime;
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        hardcoreMode = (bool)properties["HardcoreMode"];

    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);


        redTeamPlayerManagers = new List<PlayerManager>();
        blueTeamPlayerManagers = new List<PlayerManager>();


        PV = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if (currentGameState == GameState.Round)
            roundRemainingTime -= Time.deltaTime;

        if (roundRemainingTime <= 0 && PhotonNetwork.IsMasterClient && currentGameState == GameState.Round)
        {
            if (currentTerroTeam == Team.Red)
            {
                Win(Team.Blue, "rip czas");
            }
            else
            {
                Win(Team.Red, "rip czas");
            }
            currentGameState = GameState.RoundEnd;
        }
    }

    //======================================================================
    //ON KILL EVENTS
    //======================================================================
    [PunRPC]
    void RPC_AddKillMessage(string killerNickname, string killedNickname, bool headShot, Team killerTeam, string weaponName)
    {
        GlobalUIManager.Instance.ShowKillMessage(killerNickname, killedNickname, headShot, killerTeam, weaponName);
        //PlayerManager.Instance.AddKillMessage(killerNickname, killedNickname, headShot, killerTeam, weaponName);
    }
    [PunRPC]
    void RPC_ChangeRemainingPlayers(Player deadPlayer)
    {
        Team t = getStatsByPlayer(deadPlayer).team;
        if (t == Team.Red)
            remainingRedPlayers--;
        else
            remainingBluePlayers--;

        if(PV.IsMine)
        {
            CheckOnPlayerDieWin();
        }
    }
    public void AddRemainingPlayer(Team playerTeam)
    {
        PV.RPC("RPC_AddPlayerCount", RpcTarget.All, playerTeam);
    }
    [PunRPC]
    void RPC_AddPlayerCount(Team t)
    {
        if (t == Team.Red)
        {
            remainingRedPlayers++;
        }
        else
        {
            remainingBluePlayers++;
        }
    }
    //=======================================================================
    //MANAGING GAME STATES
    //Master client decides when something happen
    //=======================================================================


    void Win(Team team, string becauseOff)
    {
        PV.RPC("RPC_OnWinMaster", RpcTarget.MasterClient, team);
    } 
    [PunRPC]
    void RPC_OnWinMaster(Team winnerTeam)
    {
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            PlayerStats stats = getStatsByPlayer(p);
            if(stats.team == winnerTeam)
            {
                ChangeMoney(p, winRoundMoney);
            }
            else
            {
                ChangeMoney(p, loseRoundMoney);
            }
        }
        PV.RPC("RPC_OnWinAll", RpcTarget.All, winnerTeam);
        Invoke("ResetRound", timeAfterRoundIsOver);
    }

    [PunRPC]
    void RPC_OnWinAll(Team team)
    {
        wonRoundsOrder.Add(team);
        GlobalUIManager.Instance.ShowRoundWinner(team);
        currentGameState = GameState.RoundEnd;
        if (team == Team.Red)
        {
            redTeamPoints++;
        }
        else
        {
            blueTeamPoints++;
        }

        if(PhotonNetwork.IsMasterClient)
        {

            if (redTeamPoints >= TargetRounds)
            {
                PV.RPC("RPC_WinGame",RpcTarget.All,Team.Red);
                CancelInvoke();
            }
            else if (blueTeamPoints >= TargetRounds)
            {
                PV.RPC("RPC_WinGame", RpcTarget.All, Team.Blue);
                CancelInvoke();
            }

            if (redTeamPoints + blueTeamPoints == TargetRounds - 1)
            {
                Team t = Team.Null;

                Debug.Log("Po�owa!!");
                Debug.Log("Old terro team " + currentTerroTeam);

                if (currentTerroTeam == Team.Red)
                {
                    t = Team.Blue;
                }
                else
                {
                    t = Team.Red;
                }
                Debug.Log("new terro team: " + t);

                PV.RPC("RPC_ChangeTerroTeam", RpcTarget.All, t);
            }

        }

        GlobalUIManager.Instance.UpdateRoundsUI();
    }

    [PunRPC]
    void RPC_ChangeTerroTeam(Team newTerroTeam)
    {
        currentTerroTeam = newTerroTeam;
        PlayerManager.Instance.ResetHp();
        PlayerManager.Instance.DeleteAllWeapons();
        ResetMoney();
    }

    [PunRPC]
    void RPC_WinGame(Team winnerTeam)
    {
        Debug.Log("Winowow");
        PlayerManager.Instance.DisablePlayer();

        PlayerStats[] playerS = playerStats.ToArray();

        //Sort array of player stats
        playerS = MyExstenstions.SortArray(playerS);


        //Create list of structs
        List<playerStatsStruct> sortedStruct = new List<playerStatsStruct>();
        foreach (PlayerStats s in playerS)
        {
            playerStatsStruct structStats = new playerStatsStruct(
                s.player.NickName,
                s.kills,
                s.deaths,
                s.assists,
                s.team,
                s.money,
                s.kills * 2 + s.assists
                );
            sortedStruct.Add(structStats);
        }

        //Setup UI
        GlobalUIManager.Instance.WinGame(winnerTeam, sortedStruct);

        SaveStats(winnerTeam);
    }

    void SaveStats(Team winnerTeam)
    {
        int kills = PlayerPrefs.GetInt("Kills");
        int HS = PlayerPrefs.GetInt("Headshot");
        int gamesPlayed = PlayerPrefs.GetInt("GamesPlayed");
        int gamesWon = PlayerPrefs.GetInt("GamesWon");

        PlayerStats localStats = getStatsByPlayer(PhotonNetwork.LocalPlayer);

        Debug.Log("Saving kills: ");
        Debug.Log("Saved: " + kills);
        Debug.Log("local: " + localStats.kills);

        Debug.Log("headshots: ");
        Debug.Log("Saved: " + HS);
        Debug.Log("Local: " + localStats.headshots);

        Debug.Log("GamesPlayed: ");
        Debug.Log("Saved: " + gamesPlayed);

        PlayerPrefs.SetInt("Kills", kills + localStats.kills);
        PlayerPrefs.SetInt("Headshot", HS + localStats.headshots);
        PlayerPrefs.SetInt("GamesPlayed", gamesPlayed + 1);

        if(winnerTeam == localStats.team)
        {
            PlayerPrefs.SetInt("GamesWon", gamesWon + 1);
        }

    }
    public void OnLeaveButtonClick()
    {
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    void ResetRound()
    {
        SpawnNewBomb();
        PV.RPC("RPC_ResetRoundAll", RpcTarget.All);

        GameObject[] weaponsToTake = GameObject.FindGameObjectsWithTag("ToTakeWeapon");
        foreach (GameObject go in weaponsToTake)
        {
            go.GetComponent<ToTakeWeapon>().DestroyThis();
        }

        Invoke("StartRound", timeBeforeRoundStart);
    }
    [PunRPC]
    void RPC_ResetRoundAll()
    {
        GlobalUIManager.Instance.StartCountdown(Mathf.RoundToInt(timeBeforeRoundStart));
        GlobalUIManager.Instance.BombDefused();
        currentGameState = GameState.RoundPrepare;

        //teleport or spawn new player localy
        PlayerManager.Instance.GenerateNewPlayer();
        PlayerManager.Instance.OnRoundReset();

        //PlayerManager.Instance.DeleteBomb();
        roundRemainingTime = roundTime;
    }

    void StartRound()
    {
        PV.RPC("RPC_StartRoundAlll", RpcTarget.All);
    }
    [PunRPC]
    void RPC_StartRoundAlll()
    {
        PlayerManager.Instance.EnablePlayer();
        PlayerManager.Instance.OnRoundStart();
        currentGameState = GameState.Round;
    }

    void SpawnNewBomb()
    {
        //Find bomb  that lays on the ground and destroy it
        GameObject bombToTake = GameObject.FindGameObjectWithTag("BombToTake");
        if (bombToTake != null)
            bombToTake.GetComponent<BombToTake>().DestroyThis();

        //Find bomb that was defused? and destroy it
        GameObject bomb = GameObject.FindGameObjectWithTag("Bomb");
        if (bomb != null)
            bomb.GetComponent<Bomb>().DestroyThis();


        //Spawne new bomb on terro site
        string p = Path.Combine("PhotonPrefabs","BombToTake");
        PhotonNetwork.Instantiate(p, bombSpawnPos.position, Quaternion.identity);
    }


    //==========================================
    //Bomb
    //=====================================
    public void OnBombPlanted()
    {
        PV.RPC("RPC_SetBombPlanted", RpcTarget.All);
    }
    [PunRPC]
    void RPC_SetBombPlanted()
    {
        currentGameState = GameState.PlantedBomb;
        GlobalUIManager.Instance.BombPlanted();
    }
    //=======================================================
    //WIN CHECKS
    //=====================================================
    public void OnPlayerKilled(Player p, KillInfo info)
    {
        PV.RPC("RPC_ChangeRemainingPlayers", RpcTarget.All, p);
        PV.RPC("RPC_AddKillMessage", RpcTarget.All, info.KillerNickname, info.KilledNickname, info.HeadShot, info.KillerTeam, info.WeaponName);

    }

    void CheckOnPlayerDieWin()
    {
        Debug.Log("Killed player");
        if (currentGameState == GameState.Round)
        {
            if (remainingRedPlayers <= 0)
            {
                Win(Team.Blue, "red players rip");
            }
            else if (remainingBluePlayers <= 0)
            {
                Win(Team.Red, "blue players rip");
            }
        }
    }
    public void OnBombBum()
    {
        Win(currentTerroTeam, "Bomb bum!");
    }


    public void OnBombDefused()
    {
        if(currentTerroTeam == Team.Red)
        {
            Win(Team.Blue, "Bomb defused:0");
        }
        else
        {
            Win(Team.Red, "Bomb defused:0");
        }
    }

    //=======================================================
    //SPECTATING
    //=======================================================
    public void AddManagerToList(Team t,PlayerManager p)
    {
        if(t == Team.Red)
        {
            redTeamPlayerManagers.Add(p);
        }
        else
        {
            blueTeamPlayerManagers.Add(p);
        }
    }
    //===========================================
    //END WARMUP
    //===========================================
    public void OnPlayerSetTeam()
    {
        PV.RPC("RPC_OnPlayerSetTeam", RpcTarget.All);
    }




    [PunRPC]
    void RPC_OnPlayerSetTeam()
    {
        readyPlayers++;
        if(PhotonNetwork.IsMasterClient)
        {
            if(readyPlayers >= PhotonNetwork.PlayerList.Length)
            {
                Invoke("EndWarmup", 5);
                PV.RPC("RPC_WarmupEndStateChange", RpcTarget.All);
            }
        }
    }


    [PunRPC]
    void RPC_WarmupEndStateChange()
    {
        currentGameState = GameState.WarmupEnd;
        Invoke("deleteLocalWeapons", 2);
    }
    void deleteLocalWeapons()
    {
        PlayerManager.Instance.DeleteAllWeapons();
    }

    public void EndWarmup()
    {
        PV.RPC("RPC_EndWarmup", RpcTarget.All);
        ResetRound();
    }
    [PunRPC]
    void RPC_EndWarmup()
    {
        ResetAllStats();
        PlayerManager.Instance.OnEndWarmup();
    }

    //==============================================================================
    //STATS ADDING HANDLE
    //==============================================================================
    public void AddKill(Player player)
    {
        PV.RPC("RPC_AddKill", RpcTarget.All, player);
    }
    public void AddAssist(Player player)
    {
        PV.RPC("RPC_AddAssist", RpcTarget.All, player);
    }
    public void AddDeath(Player player)
    {
        PV.RPC("RPC_AddDeath", RpcTarget.All, player);
    }
    public void ChangeMoney(Player player, int money)
    {
        PV.RPC("RPC_ChangeMoney", RpcTarget.All, player, money);
    }
    public void AddHeadshot(Player player)
    {
        PV.RPC("RPC_AddHEadshot", RpcTarget.All, player);

    }
    //RPCS
    [PunRPC]
    void RPC_AddKill(Player p)
    {
        PlayerStats psToUpdate = getStatsByPlayer(p);
        psToUpdate.kills++;
    }
    [PunRPC]
    void RPC_ChangeMoney(Player p, int mon)
    {
        PlayerStats psToUpdate = getStatsByPlayer(p);
        psToUpdate.money += mon;
    }
    [PunRPC]
    void RPC_AddAssist(Player p)
    {
        PlayerStats psToUpdate = getStatsByPlayer(p);
        psToUpdate.assists++;
    }
    [PunRPC]
    void RPC_AddDeath(Player p)
    {
        PlayerStats psToUpdate = getStatsByPlayer(p);
        psToUpdate.deaths++;
    }
    [PunRPC]
    void RPC_AddHEadshot(Player p)
    {
        PlayerStats psToUpdate = getStatsByPlayer(p);
        psToUpdate.headshots++;
    }


    public playerStatsStruct[] GetPlayerStatsList()
    {
        List<playerStatsStruct> stats = new List<playerStatsStruct>();

        foreach (PlayerStats ps in playerStats)
        {
            int score = ps.kills * 2 + ps.assists;
            stats.Add(new playerStatsStruct(
                ps.player.NickName,
                ps.kills,
                ps.deaths,
                ps.assists,
                ps.team,
                ps.money,
                score
                ));
        }
        return stats.ToArray();

    }
    public void AddPlayerstats(Player p, Team t)
    {
        PlayerStats stats = new PlayerStats();
        stats.player = p;
        stats.team = t;
        stats.money = 16000;
        playerStats.Add(stats);
    }
    public void ResetAllStats()
    {
        foreach (PlayerStats ps in playerStats)
        {
            ps.kills = 0;
            ps.deaths = 0;
            ps.assists = 0;
            ps.money = 800;
        }
    }
    public void ResetMoney()
    {
        foreach (PlayerStats ps in playerStats)
        {
            ps.money = 800;
        }
    }
    public PlayerStats getStatsByPlayer(Player p)
    {
        foreach (PlayerStats ps in playerStats)
        {
            if (ps.player == p)
                return ps;
        }
        return null;
    }
    public Team[] getWinOrder()
    {
        return wonRoundsOrder.ToArray();
    }
    //Removing someone from scoreboard if they leave
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerStats ps = getStatsByPlayer(otherPlayer);
        playerStats.Remove(ps);
    }

}

