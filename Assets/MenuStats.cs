using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MenuStats : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] TextMeshProUGUI HSText;
    [SerializeField] TextMeshProUGUI GamesPlayedText;
    [SerializeField] TextMeshProUGUI WinratioText;
    private void OnEnable()
    {
        SetupStats();
    }
    void SetupStats()
    {
        int kills = PlayerPrefs.GetInt("Kills");
        int HS = PlayerPrefs.GetInt("Headshot");
        int gamesPlayed = PlayerPrefs.GetInt("GamesPlayed");
        int gamesWon = PlayerPrefs.GetInt("GamesWon");

        Debug.Log("Kills: " + kills);
        Debug.Log("HS: " + HS);
        Debug.Log("played: " + gamesPlayed);
        Debug.Log("won: " + gamesWon);

        killsText.text = kills.ToString();

        if(kills == 0)
        {
            HSText.text = "-";
        }
        else
        {
            float HSPercentage = HS*100 / kills;
            HSText.text = Mathf.RoundToInt(HSPercentage).ToString();
        }

        GamesPlayedText.text = gamesPlayed.ToString();


        if(gamesPlayed ==0)
        {
            WinratioText.text = "-";
        }
        else
        {
            float winPrecentage = gamesWon * 100 / gamesPlayed;
            WinratioText.text = Mathf.RoundToInt(winPrecentage).ToString();
        }

    }
}
