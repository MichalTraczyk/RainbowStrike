using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI playersText;
    public RoomInfo info;
    public void Setup(RoomInfo _info)
    {
        info = _info;
        int maxPlayers = Convert.ToInt32(info.MaxPlayers);
        string s = info.PlayerCount + " / " + maxPlayers;
        playersText.text = s;
        text.text = _info.Name;
    }
    public void OnClick()
    {
        GameLauncher.Instance.JoinRoom(info);
    }
}
