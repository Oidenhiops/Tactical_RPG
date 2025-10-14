using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicHealSO", menuName = "ScriptableObjects/Skills/BasicHealSO", order = 1)]
public class BasicHealSkillSO : SkillsBaseSO
{
    public override void UseSkill(Character characterMakeSkill, Character characterToMakeSkill)
    {
        int amountHeal = Math.Clamp(Mathf.RoundToInt(characterToMakeSkill.characterData.statistics[CharacterData.TypeStatistic.Hp].maxValue * (characterMakeSkill.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue / 100)), 1, 10000);
        characterToMakeSkill.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue += amountHeal;
        characterToMakeSkill.characterData.statistics[CharacterData.TypeStatistic.Hp].RefreshValue();
        FloatingText floatingText = Instantiate(floatingTextPrefab, characterToMakeSkill.transform.position, Quaternion.identity).GetComponent<FloatingText>();
        _ = floatingText.SendText(amountHeal.ToString(), Color.green, false);
    }
    public override void DiscountMpAfterUseSkill(Character characterMakeSkill)
    {
        characterMakeSkill.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue -= characterMakeSkill.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue;
        characterMakeSkill.characterData.statistics[CharacterData.TypeStatistic.Sp].RefreshValue();
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
