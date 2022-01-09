using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    Player player;
    [SerializeField] TextMeshProUGUI nickname;
    public void Setup(Player _player)
    {
        player = _player;
        nickname.text = _player.NickName;
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(this.gameObject);
        }
    }
    public override void OnLeftRoom()
    {
        Destroy(this.gameObject);
    }
}
