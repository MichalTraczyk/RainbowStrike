using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrenadeShopButton : MonoBehaviour
{
    private PlayerUI ui;
    private Button button;
    [SerializeField]private int price;
    [SerializeField] private string grenade;

    [SerializeField] TextMeshProUGUI priceText;
    
    // Start is called before the first frame update
    void Start()
    {
        ui = GetComponentInParent<PlayerUI>();
        button = GetComponent<Button>();

        priceText.text = price.ToString();

        button.onClick.AddListener(OnClick);

    }
    void OnClick()
    {
        ui.OnGrenadeButtonClicked(price, grenade);
    }
}
