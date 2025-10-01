using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "ScriptableObjects/StatusEffect/StatusEffectDefendSO", order = 1)]
public class StatusEffectDefendSO : StatusEffectBaseSO
{
    public override void ApplyEffect(Character character)
    {
        character.characterStatusEffect.statusEffects[this] = maxStats;
        character.characterData.statistics[CharacterData.TypeStatistic.Def].buffValue.Add(this, statusEffectStatistics[CharacterData.TypeStatistic.Def].baseValue);
        character.characterData.statistics[CharacterData.TypeStatistic.Def].RefreshValue();
        character.characterData.statistics[CharacterData.TypeStatistic.Def].SetMaxValue();
    }
    public override void ReloadEffect(Character character)
    {
        character.characterStatusEffect.statusEffects[this] = maxStats;
        character.characterData.statistics[CharacterData.TypeStatistic.Def].buffValue[this] = statusEffectStatistics[CharacterData.TypeStatistic.Def].baseValue;
        character.characterData.statistics[CharacterData.TypeStatistic.Def].RefreshValue();
        character.characterData.statistics[CharacterData.TypeStatistic.Def].SetMaxValue();
    }
    public override void DiscountEffect(Character character)
    {
        if (character.characterStatusEffect.statusEffects[this] - 1 == 0)
        {
            character.characterData.statistics[CharacterData.TypeStatistic.Def].buffValue.Remove(this);
            character.characterStatusEffect.statusEffects.Remove(this);
        }
        else
        {
            character.characterStatusEffect.statusEffects[this]--;
            if (character.characterData.statistics[CharacterData.TypeStatistic.Def].buffValue.TryGetValue(this, out int buff))
            {
                buff -= buff / 2;
                character.characterData.statistics[CharacterData.TypeStatistic.Def].buffValue[this] = buff;
            }
        }
        character.characterData.statistics[CharacterData.TypeStatistic.Def].RefreshValue();
        character.characterData.statistics[CharacterData.TypeStatistic.Def].SetMaxValue();
    }
}
