using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowPart : MonoBehaviour
{
    Window parentScript;

    private void Awake()
    {
        parentScript = GetComponentInParent<Window>();
        parentScript.addPart(this);
    }
    public void HitThis()
    {
        parentScript.OnHit(this);
    }
}
