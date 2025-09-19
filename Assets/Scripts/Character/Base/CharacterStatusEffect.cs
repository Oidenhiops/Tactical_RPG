using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatusEffect : MonoBehaviour
{
    public Character character;
    public SerializedDictionary<StatusEffectBaseSO, StatusEffectInfo> statusEffects = new SerializedDictionary<StatusEffectBaseSO, StatusEffectInfo>();
    public void Start()
    {
        PlayerManager.Instance.actionsManager.OnEndTurn += DiscountStatusEffects;
    }
    public void OnDestroy()
    {
        PlayerManager.Instance.actionsManager.OnEndTurn -= DiscountStatusEffects;
    }
    public void DiscountStatusEffects()
    {
        if (character.isCharacterPlayer && PlayerManager.Instance.actionsManager.isPlayerTurn)
        {
            foreach (KeyValuePair<StatusEffectBaseSO, StatusEffectInfo> statusEffect in statusEffects)
            {
                statusEffect.Key.DiscountEffect(character);
            }
        }
        else if (!character.isCharacterPlayer && !PlayerManager.Instance.actionsManager.isPlayerTurn)
        {
            foreach (KeyValuePair<StatusEffectBaseSO, StatusEffectInfo> statusEffect in statusEffects)
            {
                statusEffect.Key.DiscountEffect(character);
            }
        }
    }
    [Serializable] public class StatusEffectInfo
    {
        public int amount;
    }
}
