using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GlobalUIManager : MonoBehaviour
{
    
    public static GlobalUIManager Instance;
    [Header("Game winning")]
    [SerializeField] GameObject playerStatsListPrefab;
    [SerializeField] Transform playerListParent;
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] GameObject GameWinParent;
    //DO ZROBIENIA
    [Header("Round Winner")]
    [SerializeField] GameObject winnerUI;
    [SerializeField] Transform winnerUIParent;

    [Header("Kill UI")]
    [SerializeField] GameObject killedParent;
    [SerializeField] GameObject killedMessagePrefab;

    [Header("Tabela wynikow")]
    [SerializeField] GameObject tabela;
    [SerializeField] GameObject playersStatsItemPrefab;
    [SerializeField] Transform redParent;
    [SerializeField] Transform blueParent;
    [SerializeField] WinOrderElement[] winOrderElements;

    [Header("Rounds")]
    [SerializeField] TextMeshProUGUI TimeLeft;
    [SerializeField] TextMeshProUGUI redRounds;
    [SerializeField] TextMeshProUGUI blueRounds;


    [SerializeField] GameObject bombPlantedImage;
    //SETTINGS AND PAUSE MENU
    [Header("PauseAndSettingsMenu")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsParent;

    [Header("Countdown")]
    [SerializeField] GameObject countdownBackground;
    [SerializeField] TextMeshProUGUI countdownText;
    public bool paused { get; private set; }


    //REMAINING ROUNDS UI
    //DO ZROBIENIA
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    private void Start()
    {
        UpdateRoundsUI();
        SetRoundText();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !paused)
        {
            tabela.SetActive(true);
            UpdateTabeleWynikow();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            tabela.SetActive(false);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(paused)
            {
                ClosePause();
            }
            else
            {
                OpenPause();
                MenuManager.Instance.OpenMenu("SoundSettings");
            }
        }
        SetRoundText();
    }
    

    //================================
    //GAME EVENTS AND GAMEPLAY UI
    //================================
    public void StartCountdown(int s)
    {
        GetComponent<Animator>().Play("Countdown");
        StartCoroutine(countdown(s));
    }
    IEnumerator countdown(int seconds)
    {
        countdownBackground.SetActive(true);
        yield return null;
        for(int i = seconds;i>0;i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        countdownBackground.SetActive(false);
    }

    public void UpdateRoundsUI()
    {
        redRounds.text = GameManager.Instance.redTeamPoints.ToString();
        blueRounds.text = GameManager.Instance.blueTeamPoints.ToString();
    }
    private void SetRoundText()
    {
        GameState currGameState = GameManager.Instance.currentGameState;
        switch (currGameState)
        {
            case GameState.Warmup:
                TimeLeft.text = "";
                break;
            case GameState.WarmupEnd:
                TimeLeft.text = ":)";
                break;
            case GameState.Round:

                int secs = Mathf.RoundToInt(GameManager.Instance.roundRemainingTime);
                int minutes = secs / 60;

                int seconds = secs % 60;

                TimeLeft.text = minutes.ToString() + ":";
                if ((secs % 60) < 10)
                    TimeLeft.text += "0";
                TimeLeft.text += seconds.ToString();

                /*
                string mins = "";
                if (minutes < 10)
                    mins += "0";
                mins += minutes.ToString();

                string ss = "";
                if (seconds < 10)
                    ss += "0";
                ss += seconds.ToString();


                TimeLeft.text = mins + ":" + ss;*/


                break;
            case GameState.PlantedBomb:

                break;
            default:
                TimeLeft.text = "==:==";
                break;
        }
    }
    public void BombPlanted()
    {
        bombPlantedImage.SetActive(true);
    }
    public void BombDefused()
    {
        bombPlantedImage.SetActive(false);
    }
    public void WinGame(Team t,List<playerStatsStruct> sortedPlayerStats)
    {

        GameWinParent.SetActive(true);

        foreach (playerStatsStruct s in sortedPlayerStats)
        {
            GameObject go = Instantiate(playerStatsListPrefab, playerListParent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector3(rt.sizeDelta.x, 40);

            go.GetComponent<StatsListItem>().Setup(s);

            if (s.Team == Team.Red)
            {
                go.GetComponent<Image>().color = Color.red;
            }
            else
            {
                go.GetComponent<Image>().color = Color.blue;
            }

            //Enable win UI
            MenuManager.Instance.OpenMenu("WinUI");

            //Set text on top
            winText.text = "Team " + t.ToString() + " wins!";
        }
    }


    void UpdateTabeleWynikow()
    {
        foreach (Transform child in redParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in blueParent)
        {
            Destroy(child.gameObject);
        }


        playerStatsStruct[] stats = GameManager.Instance.GetPlayerStatsList();


        foreach (playerStatsStruct s in stats)
        {
            Transform parent = redParent;
            if (s.Team == Team.Blue)
                parent = blueParent;

            GameObject ob = Instantiate(playersStatsItemPrefab, parent);
            ob.GetComponent<StatsListItem>()?.Setup(s);
            RectTransform rt = ob.GetComponent<RectTransform>();

            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 40);
        }

        Team[] winOrder = GameManager.Instance.getWinOrder();
        for (int i = 0; i < winOrder.Length; i++)
        {
            winOrderElements[i].Setup(winOrder[i]);
        }
    }

    public void ShowKillMessage(string killerNickname, string killedNickname, bool headShot, Team killerTeam, string weaponName)
    {
        Instantiate(killedMessagePrefab, killedParent.transform).GetComponent<KillInfoListItem>()?.Setup(killerNickname, killedNickname, headShot, killerTeam, weaponName);
    }

    public void ShowRoundWinner(Team winner)
    {
        Instantiate(winnerUI, winnerUIParent).GetComponent<RoundWinner>().Setup(winner);
    }

    //=================================
    //SETTINGS AND PAUSE MENU
    //================================

    public void OpenPause()
    {
        paused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pausePanel.SetActive(true);
    }
    public void ClosePause()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        paused = false;
        PlayerSettings.Instance.Save();
        pausePanel.SetActive(false);
        ExitSettings();
    }

    public void ExitSettings()
    {
        settingsParent.SetActive(false);
        MenuManager.Instance.OpenMenu("GameplaySettings");
    }
    public void OpenSettings()
    {
        settingsParent.SetActive(true);
        MenuManager.Instance.OpenMenu("GameplaySettings");
    }
}
