using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : MonoBehaviour
{
    PlayerManager[] sameTeamManagers;
    int currentIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerManager.Instance.localPlayerTeam == Team.Red)
        {
            sameTeamManagers = GameManager.Instance.redTeamPlayerManagers.ToArray();
        }
        else
        {
            sameTeamManagers = GameManager.Instance.blueTeamPlayerManagers.ToArray();
        }
        ChangeSpectate();
    }
    void ChangeSpectate()
    {
        bool everybodyDead = true;

        foreach(PlayerManager pm in sameTeamManagers)
        {
            if(pm != PlayerManager.Instance)
            {
                pm.StopSpectatingThis();
                if (pm.isAlive)
                    everybodyDead = false;
            }
        }

        if (everybodyDead)
            return;

        if(sameTeamManagers[currentIndex].isAlive)
        {
            sameTeamManagers[currentIndex].StartSpectatingThis();
        }
        else
        {
            AddIndex();
            ChangeSpectate();
        }
    }
    void AddIndex()
    {
        currentIndex++;
        if (currentIndex >= sameTeamManagers.Length)
        {
            currentIndex = 0;
        }
        if (currentIndex < 0)
        {
            currentIndex = sameTeamManagers.Length - 1;
        }
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            AddIndex();
            ChangeSpectate();
        }
    }

    private void OnDestroy()
    {
        foreach (PlayerManager pm in sameTeamManagers)
        {
            if (pm != PlayerManager.Instance)
            {
                pm.StopSpectatingThis();
            }
        }
    }
}
