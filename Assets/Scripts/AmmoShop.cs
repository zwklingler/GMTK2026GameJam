using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoShop : MonoBehaviour
{
    public int cost;
    [SerializeField] PlayerWeapon weapon;
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI priceText;

    void Awake()
    {
        priceText.text = cost.ToString() + " pts";
    }

    public void TryBuy()
    {
        if(GameManager.instance.Points >= cost)
        {
            GameManager.instance.Points -= cost;

            weapon.ammo++;
        }
    }

    void Update()
    {
        button.interactable = GameManager.instance.Points >= cost;
    }
}
