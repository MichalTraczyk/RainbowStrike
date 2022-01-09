using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onKeyPressed;
    public UnityEvent onKeyrelased;
    public string MessageTosShow;
    public PlayerInteractHandle owner;
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
