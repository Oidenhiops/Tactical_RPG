using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatusEffect : MonoBehaviour
{
    public CharacterBase character;
    public SerializedDictionary<StatusEffectBaseSO, int> statusEffects = new SerializedDictionary<StatusEffectBaseSO, int>();
    public void Start()
    {
        if(BattlePlayerManager.Instance) BattlePlayerManager.Instance.actionsManager.OnEndTurn += DiscountStatusEffects;
    }
    public void OnDestroy()
    {
        if(BattlePlayerManager.Instance) BattlePlayerManager.Instance.actionsManager.OnEndTurn -= DiscountStatusEffects;
    }
    public void DiscountStatusEffects()
    {
        if (character.isCharacterPlayer && BattlePlayerManager.Instance.actionsManager.isPlayerTurn)
        {
            foreach (KeyValuePair<StatusEffectBaseSO, int> statusEffect in statusEffects)
            {
                if (!statusEffect.Key.isPermanent)
                {
                    statusEffect.Key.DiscountEffect(character);
                }
                else
                {
                    statusEffect.Key.ReApplyEffect(character);
                }
            }
        }
        else if (!character.isCharacterPlayer && !BattlePlayerManager.Instance.actionsManager.isPlayerTurn)
        {
            foreach (KeyValuePair<StatusEffectBaseSO, int> statusEffect in statusEffects)
            {
                if (!statusEffect.Key.isPermanent)
                {
                    statusEffect.Key.DiscountEffect(character);
                }
                else
                {
                    statusEffect.Key.ReApplyEffect(character);
                }
            }
        }
    }
}
