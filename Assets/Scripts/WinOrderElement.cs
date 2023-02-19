using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinOrderElement : MonoBehaviour
{

    public GameObject blueElement;
    public GameObject redElement;
    private void Awake()
    {
        blueElement.SetActive(false);
        redElement.SetActive(false);
    }
    public void Setup(Team t)
    {
        if(t == Team.Red)
        {
            redElement.SetActive(true);
        }
        else
        {
            blueElement.SetActive(true);
        }
    }
}
