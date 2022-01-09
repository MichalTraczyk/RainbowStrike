using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System;

public class GameLauncher : MonoBehaviourPunCallbacks
{
    public static GameLauncher Instance;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField maxPlayersInputField;
    [SerializeField] Toggle hardcoreModeToggle;


    [SerializeField] TextMeshProUGUI errorText;
    [SerializeField] TextMeshProUGUI roomText;

    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playersListContent;

    [SerializeField] GameObject roomListPrefab;
    [SerializeField] GameObject playerListPrefab;
    [SerializeField] GameObject startButton;

    [SerializeField] TMP_InputField nicknameInputField;

    public GameObject settingsParent;
    int map = 1;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    public void ConnectButtonClicked()
    {
        if(nicknameInputField.text == "" || nicknameInputField.text.Length > 16)
        {
            return;
        }

        MenuManager.Instance.OpenMenu("Loading");
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("Title");
        PhotonNetwork.NickName = nicknameInputField.text;
    }
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return;

        //Room options
        RoomOptions roomOptions = new RoomOptions();

        //max players
        int maxPlayers = int.Parse(maxPlayersInputField.text);
        maxPlayers = Mathf.Clamp(maxPlayers,1, 10);
        roomOptions.MaxPlayers = Convert.ToByte(maxPlayers);

        //Hardocre mode
        bool hardcore = hardcoreModeToggle.isOn;
        ExitGames.Client.Photon.Hashtable customPropeties = new ExitGames.Client.Photon.Hashtable();
        customPropeties.Add("HardcoreMode", hardcore);
        roomOptions.CustomRoomProperties = customPropeties;



        PhotonNetwork.CreateRoom(roomNameInputField.text,roomOptions);
        MenuManager.Instance.OpenMenu("Loading");
    }
    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("Room");
        roomText.text = PhotonNetwork.CurrentRoom.Name;

        foreach (Transform t in playersListContent)
        {
            Destroy(t.gameObject);
        }

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListPrefab, playersListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }

        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        errorText.text = "Error: " + errorInfo.Info;
        MenuManager.Instance.OpenMenu("Error");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Error: " + message;
        MenuManager.Instance.OpenMenu("Error");
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }
    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("Title");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform t in roomListContent)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
        }

    }
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("Loading");

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListPrefab, playersListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }
    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(map);
    }
    public void OpenSettings()
    {
        settingsParent.SetActive(true);
        MenuManager.Instance.OpenMenu("GameplaySettings");
    }
    public void CloseSettings()
    {
        settingsParent.SetActive(false);
        PlayerSettings.Instance.Save();
        MenuManager.Instance.OpenMenu("Title");
    }
    public void OnMapChanged(int newMap)
    {
        map = newMap;
    }
}
