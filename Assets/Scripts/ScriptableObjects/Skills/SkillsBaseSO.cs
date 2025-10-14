using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SkillsBaseSO : ScriptableObject
{
    public int skillId;
    public string skillIdText;
    public string animationSkillName;
    public string generalAnimationSkillName;
    public bool needSceneAnimation;
    public GameObject skillVFXPrefab;
    public float skillVFXDuration = 1f;
    public GameObject floatingTextPrefab;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> statistics = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
    public ItemBaseSO.TypeWeapon typeSkill;
    public Vector3Int[] positionsToMakeSkill;
    public int positionsToMakeSkillGridSize = 5;
    public bool usePositionsToMakeSkill;
    public Vector3Int[] positionsSkillForm;
    public int positionsSkillFormGridSize = 5;
    public bool isFreeMovementSkill;
    public bool needCharacterToMakeSkill;
    public virtual void UseSkill(Character characterMakeSkill, Character characterToMakeSkill){ Debug.LogError("UseSkill non implemented"); }
    public virtual void LevelUpSkill(Character character) { Debug.LogError("LevelUpSkill non implemented"); }
    public void AddSkill(Character character)
    {
        if (character.characterData.skills.ContainsKey(typeSkill))
        {
            if (!character.characterData.skills[typeSkill].ContainsKey(skillId))
            {
                character.characterData.skills[typeSkill].Add(skillId, new CharacterData.CharacterSkillInfo{ skillsBaseSO = this, level = 0, statistics = CloneStatistics() });
            }
        }
        else
        {
            character.characterData.skills.Add(typeSkill, new UnityEngine.Rendering.SerializedDictionary<int, CharacterData.CharacterSkillInfo>
            {
                {skillId, new CharacterData.CharacterSkillInfo { skillsBaseSO = this, level = 0, statistics = CloneStatistics() }}
            });
        }
    }
    public bool ValidateCanUseSkill(Character character)
    {
        return character.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue - character.characterData.skills[typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue > 0;
    }
    public string[] GetSkillDescription (SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> statistics)
    {
        List<string> info = new List<string>();

        foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in statistics)
        {
            if (statistic.Key != CharacterData.TypeStatistic.Exp)
            {
                info.Add($"{statistic.Value.baseValue}%");
            }
        }

        return info.ToArray();
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
}