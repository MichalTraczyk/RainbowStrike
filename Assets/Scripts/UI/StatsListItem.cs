using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
public class StatsListItem : MonoBehaviour
{
    public TextMeshProUGUI Nickname;
    public TextMeshProUGUI Kills;
    public TextMeshProUGUI Deaths;
    public TextMeshProUGUI Assists;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Money;
    public GameObject isDeadObject;

    public void Setup(playerStatsStruct stats)
    {
        Nickname.text = stats.Nickname;
        Kills.text =stats.Kills.ToString();
        Deaths.text =stats.Deaths.ToString();
        Assists.text =stats.Assists.ToString();
        Score.text =stats.Score.ToString();
        isDeadObject.SetActive(stats.IsDead);

        if (stats.Team == PlayerManager.Instance.localPlayerTeam)
            Money.text = stats.Money.ToString();
        else
            Money.text = "";
    }
}
