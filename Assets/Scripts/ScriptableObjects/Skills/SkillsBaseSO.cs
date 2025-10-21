using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SkillsBaseSO : ScriptableObject
{
    public string skillId;
    public string skillIdText;
    public string animationSkillName;
    public string generalAnimationSkillName;
    public TypeSkill typeSkill;
    public bool needSceneAnimation;
    public GameObject skillVFXPrefab;
    public float skillVFXDuration = 1f;
    public GameObject floatingTextPrefab;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> statistics = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
    public ItemBaseSO.TypeWeapon weaponForUseSkill;
    public Vector3Int[] positionsToMakeSkill;
    public int positionsToMakeSkillGridSize = 5;
    public bool usePositionsToMakeSkill;
    public Vector3Int[] positionsSkillForm;
    public int positionsSkillFormGridSize = 5;
    public bool isFreeMovementSkill;
    public bool needCharacterToMakeSkill;
    public virtual void UseSkill(CharacterBase characterMakeSkill, CharacterBase characterToMakeSkill) { Debug.LogError("UseSkill non implemented"); }
    public virtual void DiscountMpAfterUseSkill(CharacterBase characterMakeSkill) { Debug.LogError("DiscountMpAfterUseSkill non implemented"); }
    public virtual void LevelUpSkill(CharacterBase character) { Debug.LogError("LevelUpSkill non implemented"); }
    public void AddSkill(CharacterBase character)
    {
        if (character.characterData.skills.ContainsKey(weaponForUseSkill))
        {
            if (!character.characterData.skills[weaponForUseSkill][typeSkill].ContainsKey(skillId))
            {
                character.characterData.skills[weaponForUseSkill][typeSkill].Add(skillId, new CharacterData.CharacterSkillInfo{ skillsBaseSO = this, level = 0, statistics = CloneStatistics() });
            }
        }
        else
        {
            character.characterData.skills.Add(weaponForUseSkill, new UnityEngine.Rendering.SerializedDictionary<TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>>()
            {
                {typeSkill, new UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>
                    {
                        {skillId, new CharacterData.CharacterSkillInfo { skillsBaseSO = this, level = 0, statistics = CloneStatistics() }}
                    }
                }
            });
        }
    }
    public bool ValidateCanUseSkill(CharacterBase character)
    {
        return character.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue - character.characterData.skills[weaponForUseSkill][typeSkill][skillId].statistics[CharacterData.TypeStatistic.Sp].baseValue > 0;
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
    public enum TypeSkill
    {
        Attack,
        Heal,
        Buff,
        Debuff
    }
}