using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SkillsBaseSO : ScriptableObject
{
    public int skillId;
    public int skillIdText;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> statistics = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
    public TypeSkill typeSkill;
    public virtual void UseSkill(Character character){ Debug.LogError("UseSkill non implemented"); }
    public virtual void LevelUpSkill(Character character) { Debug.LogError("LevelUpSkill non implemented"); }
    public void AddSkill(Character character)
    {
        if (character.characterData.skills.ContainsKey(typeSkill))
        {
            if (!character.characterData.skills[typeSkill].ContainsKey(skillId))
            {
                character.characterData.skills[typeSkill].Add(skillId, new CharacterData.SkillInfo{ skillsBaseSO = this, level = 0, statistics = CloneStatistics() });
            }
        }
        else
        {
            character.characterData.skills.Add(typeSkill, new SerializedDictionary<int, CharacterData.SkillInfo>
            {
                {skillId, new CharacterData.SkillInfo { skillsBaseSO = this, level = 0, statistics = CloneStatistics() }}
            });
        }
    }
    public bool ValidateCanUseSkill(Character character)
    {
        return character.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue - character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue > 0;
    }
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> CloneStatistics()
    {
        var clone = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();

        foreach (var kvp in statistics)
        {
            clone[kvp.Key] = new CharacterData.Statistic
            {
                baseValue = kvp.Value.baseValue,
                aptitudeValue = kvp.Value.aptitudeValue,
                itemValue = kvp.Value.itemValue,
                buffValue = kvp.Value.buffValue,
                maxValue = kvp.Value.maxValue,
                currentValue = kvp.Value.currentValue
            };
        }

        return clone;
    }
    public enum TypeSkill
    {
        None = 0,
        Fist = 1,
        Sword = 2,
        Spear = 3,
        Bow = 4,
        Gun = 5,
        Axe = 6,
        Staff = 7,
        Monster = 8,
        General = 10
    }
}