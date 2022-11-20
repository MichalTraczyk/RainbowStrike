using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowPart : MonoBehaviour
{
    Barricade parentScript;

    private void Awake()
    {
        parentScript = GetComponentInParent<Barricade>();
        parentScript.addPart(this);
    }
    public void HitThis()
    {
        parentScript.OnHit(this);
    }
}
