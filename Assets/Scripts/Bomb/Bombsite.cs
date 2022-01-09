using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bombsite : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerBombPlant p = other.GetComponent<PlayerBombPlant>();
        if (p != null)
        {
            p.EnterBombsite();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerBombPlant p = other.GetComponent<PlayerBombPlant>();
        if (p != null)
        {
            p.ExitBombsite();
        }
    }
}
