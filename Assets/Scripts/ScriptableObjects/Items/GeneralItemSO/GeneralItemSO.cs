using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items/ItemSO", order = 1)]
public class GeneralItemSO : ItemBaseSO
{
    public override void EquipItem(Character character)
    {
        foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in itemStatistics)
        {
            character.characterData.statistics[statistic.Key].itemValue += statistic.Value.baseValue;
        }
    }
    public override void DesEquipItem(Character character)
    {
        foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in itemStatistics)
        {
            character.characterData.statistics[statistic.Key].itemValue -= statistic.Value.baseValue;
        }
    }
}
