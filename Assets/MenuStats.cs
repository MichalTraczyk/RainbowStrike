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


        killsText.text = kills.ToString();

        if(kills == 0)
        {
            HSText.text = "-";
        }
        else
        {
            HSText.text = Mathf.RoundToInt((HS / kills) * 100).ToString();
        }

        GamesPlayedText.text = gamesPlayed.ToString();


        if(gamesPlayed ==0)
        {
            WinratioText.text = "-";
        }
        else
        {
            WinratioText.text = Mathf.RoundToInt((gamesWon / gamesPlayed) * 100).ToString();
        }

    }
}
