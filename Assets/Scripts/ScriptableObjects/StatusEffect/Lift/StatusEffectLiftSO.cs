using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "ScriptableObjects/StatusEffect/StatusEffectLiftSO", order = 1)]
public class StatusEffectLiftSO : StatusEffectBaseSO
{
    public override void ApplyEffect(Character character)
    {
        if (GetParentCount(character.gameObject.transform) <= 1)
        {
            float porcent = (float)statusEffectStatistics[CharacterData.TypeStatistic.Hp].baseValue / 100;
            character.lastAction = ActionsManager.TypeAction.Lift;
            character.TakeDamage(character, Mathf.CeilToInt(character.characterData.statistics[CharacterData.TypeStatistic.Hp].maxValue * porcent), "Lift");
        }
        else
        {
            character.lastAction = ActionsManager.TypeAction.Lift;
        }
    }
    public int GetParentCount(Transform obj)
    {
        int count = 0;
        Transform current = obj.parent;

        while (current != null)
        {
            count++;
            current = current.parent;
        }

        return count;
    }
    public override void ReApplyEffect(Character character)
    {
        ApplyEffect(character);
    }
}
