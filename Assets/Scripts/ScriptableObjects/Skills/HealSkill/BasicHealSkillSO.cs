using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicHealSO", menuName = "ScriptableObjects/Skills/BasicHealSO", order = 1)]
public class BasicHealSkillSO : SkillsBaseSO
{
    public override void UseSkill(Character character)
    {
        int amountHeal = Math.Clamp(Mathf.RoundToInt(character.characterData.statistics[CharacterData.TypeStatistic.Hp].maxValue * (character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue / 100)), 1, 10000);

        character.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue += amountHeal;
        character.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue -= character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue;

        character.characterData.statistics[CharacterData.TypeStatistic.Hp].RefreshValue();
        character.characterData.statistics[CharacterData.TypeStatistic.Sp].RefreshValue();
    }
    public override void LevelUpSkill(Character character)
    {
        character.characterData.skills[typeSkill][skillId].level++;

        int costValue = Mathf.CeilToInt(character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue * 1.5f);
        character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue = costValue;

        character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Hp].baseValue += 1;

        character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Exp].maxValue = Mathf.CeilToInt(character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Exp].maxValue * 2.2f);
        character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Exp].currentValue = 0;
    }
}
