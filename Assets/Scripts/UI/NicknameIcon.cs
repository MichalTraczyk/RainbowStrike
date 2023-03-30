using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NicknameIcon : Marker
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    private Transform parentPlayer;


    Vector3 p;
    Vector3 targetPosition;

    float dist;
    int d;

    public void Setup(Transform parent, string nickname)
    {
        parentPlayer = parent;
        nicknameText.text = nickname;
    }

    public override void UpdatePosition(Vector3 pos)
    {
        p = parentPlayer.position + Vector3.up * 2f;
        targetPosition = parentUI.getPingPosition(p);
        if (targetPosition.z < 0)
        {
            nicknameText.gameObject.SetActive(false);
            distanceText.gameObject.SetActive(false);
        }
        else
        {
            nicknameText.gameObject.SetActive(true);
            distanceText.gameObject.SetActive(true);

            transform.position = targetPosition;

            dist = Vector3.Distance(pos, p);
            d = (int)dist;
            distanceText.text = d.ToString() + " m";

            if (parentPlayer == null)
                parentUI.OnPingDestroy(this);
        }
    }
}