using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectBanner : MonoBehaviour
{
    public Image statusEffectSprite;
    public TMP_Text statusEffectAmount;
    public void SetData(StatusEffectBaseSO statusEffectBaseSO, int amount)
    {
        statusEffectSprite.sprite = statusEffectBaseSO.icon;
        statusEffectAmount.text = amount.ToString();
    }
}
