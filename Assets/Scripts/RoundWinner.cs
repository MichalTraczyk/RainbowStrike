using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RoundWinner : MonoBehaviour
{
    public TextMeshProUGUI winText;
    public Image img;
    private void Start()
    {
        Destroy(this.gameObject, GameManager.Instance.timeAfterRoundIsOver);
    }
    public void Setup(Team winnerTeam)
    {
        winText.text = winnerTeam.ToString() + " wins!";
        if(winnerTeam == Team.Red)
        {
            img.color = Color.red;
        }
        else
        {
            img.color = Color.blue;
        }
    }

}
