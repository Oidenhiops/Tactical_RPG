#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
[CreateAssetMenu(fileName = "InitialData", menuName = "ScriptableObjects/Character/InitialDataSO", order = 1)]
public class InitialDataSO : ScriptableObject
{
    public int id;
    public int subId;
    public bool isHumanoid;
    public Texture2D atlas;
    public Texture2D atlasHands;
    public Sprite icon;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> initialStats = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>
    {
        {CharacterData.TypeStatistic.Hp, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Sp, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Atk, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Hit, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Int, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Def, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Res, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Spd, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Mvtr, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Mvth, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Thwr, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Crtv, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Crtd, new CharacterData.Statistic{ aptitudeValue = 100 }},
        {CharacterData.TypeStatistic.Exp, new CharacterData.Statistic{ aptitudeValue = 100 }},
    };
    public SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo> initialMastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
    {
        {CharacterData.TypeMastery.Fist, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}},
        {CharacterData.TypeMastery.Sword, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}},
        {CharacterData.TypeMastery.Spear, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}},
        {CharacterData.TypeMastery.Bow, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}},
        {CharacterData.TypeMastery.Gun, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}},
        {CharacterData.TypeMastery.Axe, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}},
        {CharacterData.TypeMastery.Staff, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0, maxExp = 15}}
    };
    public SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<SkillsBaseSO.TypeSkill, SerializedDictionary<string, CharacterData.CharacterSkillInfo>>> initialSkills = new SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<SkillsBaseSO.TypeSkill, SerializedDictionary<string, CharacterData.CharacterSkillInfo>>>();
    public SerializedDictionary<string, AnimationsInfo> animations = new SerializedDictionary<string, AnimationsInfo>();
    private string[] defaultNames = { "Idle", "Walk", "TakeDamage", "Defend", "Lifted", "Lift", "Throw", "FistAttack", "SwordAttack", "SpearAttack", "BowAttack", "GunAttack", "AxeAttack", "StaffAttack" };
    public GenerateAllAnimations generateAllAnimations;

#if UNITY_EDITOR
    [NaughtyAttributes.Button]
    public void GenerateAllCharacterAnimations()
    {
        if (generateAllAnimations.atlas == null || generateAllAnimations.baseSprite == null) return;

        isHumanoid = generateAllAnimations.isHumanoid;
        Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(generateAllAnimations.atlas)).OfType<Sprite>().ToArray();
        int spriteW = Mathf.RoundToInt(generateAllAnimations.baseSprite.rect.width);
        int indexSpriteForEvaluate = 0;
        int nameIndex = 0;
        string animationName;
        int middleIndex;
        animations.Clear();
        while (true)
        {
            animationName = isHumanoid ? defaultNames.Length > nameIndex ? defaultNames[nameIndex] : nameIndex.ToString() : 5 > nameIndex ? defaultNames[nameIndex] : nameIndex == 5 ? "FistAttack" : nameIndex.ToString();
            List<Sprite> row = new List<Sprite>();
            for (int i = 0; i < generateAllAnimations.atlas.width / spriteW; i++)
            {
                if (i + indexSpriteForEvaluate > allSprites.Length - 1 || allSprites[i + indexSpriteForEvaluate].rect.y != allSprites[indexSpriteForEvaluate].rect.y)
                {
                    break;
                }
                row.Add(allSprites[i + indexSpriteForEvaluate]);
            }
            middleIndex = row.Count / 2;
            AnimationsInfo animationInfo = new AnimationsInfo
            {
                name = animationName,
                spritesInfoDown = new SpritesInfo[middleIndex],
                spritesInfoUp = new SpritesInfo[middleIndex],
            };
            for (int i = 0; i < row.Count; i++)
            {
                if (i < middleIndex)
                {
                    animationInfo.spritesInfoDown[i] = new SpritesInfo();
                    animationInfo.spritesInfoDown[i].characterSprite = row[i];
                }
                else
                {
                    animationInfo.spritesInfoUp[i - middleIndex] = new SpritesInfo();
                    animationInfo.spritesInfoUp[i - middleIndex].characterSprite = row[i];
                }
            }
            animations.Add(animationName, animationInfo);

            switch (animationName)
            {
                case "Defend":
                case "TakeDamage":
                    int amountSprites = 0;
                    List<SpritesInfo> spritesUp = new List<SpritesInfo>();
                    List<SpritesInfo> spritesDown = new List<SpritesInfo>();
                    for (int i = 0; i < 6; i++)
                    {
                        foreach (var spriteUp in animations[animationName].spritesInfoDown)
                        {
                            spritesDown.Add(new SpritesInfo
                            {
                                characterSprite = spriteUp.characterSprite
                            });
                            amountSprites++;
                        }
                        foreach (var spriteUp in animations[animationName].spritesInfoUp)
                        {
                            spritesUp.Add(new SpritesInfo
                            {
                                characterSprite = spriteUp.characterSprite
                            });
                        }
                        if (i == 0 && amountSprites == 4 || amountSprites == 6)
                        {
                            break;
                        }
                    }
                    animations[animationName].spritesInfoDown = spritesDown.ToArray();
                    animations[animationName].spritesInfoUp = spritesUp.ToArray();
                    break;
                case "Idle":
                case "Walk":
                case "Lifted":
                case "Lift":
                    animations[animationName].loop = true;
                    break;
            }
            if (animationName == "Defend")
            {
                animations.Add("GeneralSkillEffect", animations["Defend"]);
                animations["GeneralSkillEffect"].name = "GeneralSkillEffect";
            }
            nameIndex++;
            indexSpriteForEvaluate += row.Count;
            if (nameIndex >= generateAllAnimations.atlas.height / spriteW)
            {
                break;
            }
        }
        animations["TakeDamage"].animationsEffects = new SerializedDictionary<CharacterAnimation.TypeAnimationsEffects, CharacterAnimation.AnimationEffectInfo>
        {
            {
                CharacterAnimation.TypeAnimationsEffects.Shake,
                new CharacterAnimation.AnimationEffectInfo
                {
                    amplitude = 0.1f,
                    frequency = 100
                }
            },
            {
                CharacterAnimation.TypeAnimationsEffects.Blink,
                new CharacterAnimation.AnimationEffectInfo
                {
                    colorBlink = Color.HSVToRGB(0, 100, 58)
                }
            }
        };
        atlas = generateAllAnimations.atlas;
        atlasHands = generateAllAnimations.atlasHands;
        icon = generateAllAnimations.icon;

        if (isHumanoid)
        {
            animations["FistAttack"].frameToInstance = 2;
            animations["SwordAttack"].frameToInstance = 3;
        }
    }
#endif
    [NaughtyAttributes.Button]
    public void RefreshSkillsData()
    {
        SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<SkillsBaseSO.TypeSkill, SerializedDictionary<string, CharacterData.CharacterSkillInfo>>> clonedSkills = new SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<SkillsBaseSO.TypeSkill, SerializedDictionary<string, CharacterData.CharacterSkillInfo>>>();
        for (int i = 0; i < initialSkills.Count; i++)
        {
            for (int y = 0; y < initialSkills.ElementAt(i).Value.Count; y++)
            {
                SerializedDictionary<string, CharacterData.CharacterSkillInfo> innerSkills = new SerializedDictionary<string, CharacterData.CharacterSkillInfo>();
                for (int x = 0; x < initialSkills.ElementAt(i).Value.ElementAt(y).Value.Count; x++)
                {
                    innerSkills.Add(initialSkills.ElementAt(i).Value.ElementAt(y).Value.ElementAt(x).Value.skillsBaseSO.skillId, new CharacterData.CharacterSkillInfo
                    {
                        skillId = initialSkills.ElementAt(i).Value.ElementAt(y).Value.ElementAt(x).Value.skillsBaseSO.skillId,
                        skillsBaseSO = initialSkills.ElementAt(i).Value.ElementAt(y).Value.ElementAt(x).Value.skillsBaseSO,
                        level = initialSkills.ElementAt(i).Value.ElementAt(y).Value.ElementAt(x).Value.level,
                        statistics = initialSkills.ElementAt(i).Value.ElementAt(y).Value.ElementAt(x).Value.skillsBaseSO.CloneStatistics()
                    });
                }
                clonedSkills.Add(initialSkills.ElementAt(i).Key, new SerializedDictionary<SkillsBaseSO.TypeSkill, SerializedDictionary<string, CharacterData.CharacterSkillInfo>>()
                {
                    { initialSkills.ElementAt(i).Value.ElementAt(y).Key, innerSkills }
                });
            }
        }
        initialSkills = clonedSkills;
    }
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> CloneStatistics()
    {
        var clone = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();

        foreach (var kvp in initialStats)
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
    public SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo> CloneMastery()
    {
        var clone = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>();

        foreach (var kvp in initialMastery)
        {
            clone[kvp.Key] = new CharacterData.CharacterMasteryInfo
            {
                currentExp = 0,
                masteryLevel = 0,
                masteryRange = kvp.Value.masteryRange,
                maxExp = kvp.Value.maxExp
            };
        }

        return clone;
    }
    public UnityEngine.Rendering.SerializedDictionary<ItemBaseSO.TypeWeapon, UnityEngine.Rendering.SerializedDictionary<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>>> CloneSkills()
    {
        var clone = new UnityEngine.Rendering.SerializedDictionary<ItemBaseSO.TypeWeapon, UnityEngine.Rendering.SerializedDictionary<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>>>();

        foreach (var weaponKvp in initialSkills)
        {
            var weaponClone = new UnityEngine.Rendering.SerializedDictionary<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>>();
            foreach (var typeSkillKvp in weaponKvp.Value)
            {
                var typeSkillClone = new UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>();
                foreach (var skillKvp in typeSkillKvp.Value)
                {
                    typeSkillClone.Add(skillKvp.Key, new CharacterData.CharacterSkillInfo
                    {
                        skillId = skillKvp.Value.skillsBaseSO.skillId,
                        skillsBaseSO = skillKvp.Value.skillsBaseSO,
                        level = skillKvp.Value.level,
                        statistics = skillKvp.Value.skillsBaseSO.CloneStatistics()
                    });
                }
                weaponClone.Add(typeSkillKvp.Key, typeSkillClone);
            }
            clone.Add(weaponKvp.Key, weaponClone);
        }
        return clone;
    }
    [Serializable]
    public class AnimationsInfo
    {
        public string name;
        public string linkAnimation;
        public SpritesInfo[] spritesInfoDown;
        public SpritesInfo[] spritesInfoUp;
        public SerializedDictionary<CharacterAnimation.TypeAnimationsEffects, CharacterAnimation.AnimationEffectInfo> animationsEffects;
        public bool loop = false;
        public bool needInstance = false;
        public int frameToInstance = 0;
        public GameObject instanceObj;
        public GameObject instance;
    }
    [Serializable]
    public class SpritesInfo
    {
        public Sprite characterSprite;
        public Vector3 leftHandPosDL;
        public Vector3 leftHandPosDR;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosDL;
        public Vector3 rightHandPosDR;
        public Quaternion rightHandRotation;
    }
    [Serializable]
    public class GenerateAllAnimations
    {
        public Sprite baseSprite;
        public Texture2D atlas;
        public Texture2D atlasHands;
        public Sprite icon;
        public bool isHumanoid;
    }
}