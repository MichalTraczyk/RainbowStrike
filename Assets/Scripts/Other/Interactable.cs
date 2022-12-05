using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public Team[] teamsThatCan= {Team.Red,Team.Blue};
    public UnityEvent onKeyPressed;
    public UnityEvent onKeyrelased;
    public string MessageTosShow;
    public PlayerInteractHandle owner;

    private void Start()
    {
        if(!Contains(teamsThatCan, PlayerManager.Instance.localPlayerTeam))
        {
            Destroy(this);
        }
    }
    private void OnDestroy()
    {
        if (owner != null)
        {
            PlayerInteractHandle h = owner.GetComponent<PlayerInteractHandle>();
            if (h != null)
            {
                if (h.currInteract == this)
                    h.currInteract = null;
                h.UpdateUI();
            }
        }
    }
    bool Contains(Team[] arr, Team t)
    {
        foreach(Team team in arr)
        {
            if (team == t)
                return true;
        }
        return false;
    }
}
