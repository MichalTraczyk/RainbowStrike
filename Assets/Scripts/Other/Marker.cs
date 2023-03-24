using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public float lifetime = 5;
    private float t;

    private void Update()
    {
        t += Time.deltaTime;
        if(t>lifetime)
        {
            GetComponentInParent<PlayerUI>().OnPingDestroy(this.GetComponent<RectTransform>());
        }
    }
}
