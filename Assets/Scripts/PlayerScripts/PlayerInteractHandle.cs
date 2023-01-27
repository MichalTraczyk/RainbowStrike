using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractHandle : MonoBehaviour
{
    public Interactable currInteract;
    public Transform rayStart;
    public LayerMask layers;
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currInteract != null)
            {
                currInteract.onKeyPressed.Invoke();
            }
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            if (currInteract != null)
                currInteract.onKeyrelased.Invoke();
        }

        RaycastHit hit;
        if (Physics.Raycast(rayStart.position, rayStart.forward, out hit, 2, layers))
        {
            if (currInteract == null)
            {
                if(hit.transform.GetComponent<Interactable>() != null)
                {
                    currInteract = hit.transform.GetComponent<Interactable>();
                    currInteract.owner = this;
                }
            }
        }
        else
        {
            if (currInteract != null)
            {
                currInteract.owner = null;
            }
            currInteract = null;
        }
        UpdateUI();
    }
    public void UpdateUI()
    {
        if (currInteract != null)
            GetComponent<PlayerUI>().UpdateInteractMessage(currInteract.MessageTosShow);
        else
            GetComponent<PlayerUI>().UpdateInteractMessage("");

    }
}
