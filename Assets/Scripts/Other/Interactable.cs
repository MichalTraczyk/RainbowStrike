using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool canTTUseIt = true;
    public bool canCTuseIt = true;
    
    public UnityEvent onKeyPressed;
    public UnityEvent onKeyrelased;
    public string MessageTosShow;
    public PlayerInteractHandle owner;
    public bool shouldBeEnabled = true;
    public void TeamCheck()
    {

        Team t = PlayerManager.Instance.localPlayerTeam;
        Team terroTeam = GameManager.Instance.currentTerroTeam;

        if (t == terroTeam)//if players is in terro
        {
            //and terro can use it
            if(canTTUseIt)
            {
                shouldBeEnabled = true;
            }
            else
            {
                shouldBeEnabled = false;
            }
        }          
        else if(t != terroTeam)//if player is playing CT
        {
            if (canCTuseIt)
            {
                shouldBeEnabled = true;
            }
            else
            {
                shouldBeEnabled = false;
            }
        }
        Debug.Log("team czek " + PlayerManager.Instance.localPlayerTeam + "should be destoreyd? " + shouldBeEnabled);

        Collider[] cols = GetComponents<Collider>();
        foreach(Collider c in cols)
        {
            c.enabled = shouldBeEnabled;
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
}
