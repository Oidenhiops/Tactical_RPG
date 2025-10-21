using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items/ItemSO", order = 1)]
public class GeneralItemSO : ItemBaseSO
{
    public override void EquipItem(CharacterBase character, CharacterData.CharacterItem characterItem)
    {
        foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in characterItem.itemStatistics)
        {
            character.characterData.statistics[statistic.Key].itemValue += statistic.Value.baseValue;
            character.characterData.statistics[statistic.Key].RefreshValue();
            if (statistic.Key != CharacterData.TypeStatistic.Hp)
            {
                character.characterData.statistics[statistic.Key].SetMaxValue();
            }
        }
    }
    public override void DesEquipItem(CharacterBase character, CharacterData.CharacterItem characterItem)
    {
        foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in characterItem.itemStatistics)
        {
            character.characterData.statistics[statistic.Key].itemValue -= statistic.Value.baseValue;
            character.characterData.statistics[statistic.Key].RefreshValue();
        }
    }
}
