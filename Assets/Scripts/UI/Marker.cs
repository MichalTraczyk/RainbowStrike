using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public abstract class Marker : MonoBehaviour
{
    protected PlayerUI parentUI;
    [SerializeField] protected TextMeshProUGUI distanceText;
    private void Start()
    {
        parentUI = PlayerManager.Instance.currentPlayerGameObject.GetComponent<PlayerUI>();
    }
    public abstract void UpdatePosition(Vector3 currentPos);

}
