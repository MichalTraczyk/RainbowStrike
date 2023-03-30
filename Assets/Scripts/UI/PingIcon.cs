using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PingIcon : Marker
{
    private float lifetime = 5;
    private float t;
    private Vector3 pingPosition;
    [SerializeField] Image img;


    private Vector3 targetPos;
    private float dist;
    private int d;
    public void Setup(Vector3 pos, Sprite icon = null)
    {
        pingPosition = pos;
        if (icon != null)
            img.sprite = icon;
    }

    public override void UpdatePosition(Vector3 currentPos)
    {
        targetPos = parentUI.getPingPosition(pingPosition);
        if(targetPos.z < 0)
        {
            img.gameObject.SetActive(false);
            distanceText.gameObject.SetActive(false);
        }
        else
        {
            img.gameObject.SetActive(true);
            distanceText.gameObject.SetActive(true);

            dist = Vector3.Distance(currentPos, pingPosition);
            d = (int)dist;
            distanceText.text = d.ToString() + " m";
            transform.position = targetPos;
        }
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (t > lifetime)
        {
            parentUI.OnPingDestroy(this);
        }
    }
}
