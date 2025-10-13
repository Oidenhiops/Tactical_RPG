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
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> initialStats = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
    public SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo> mastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
    {
        {CharacterData.TypeMastery.Fist, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}},
        {CharacterData.TypeMastery.Sword, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}},
        {CharacterData.TypeMastery.Spear, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}},
        {CharacterData.TypeMastery.Bow, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}},
        {CharacterData.TypeMastery.Gun, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}},
        {CharacterData.TypeMastery.Axe, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}},
        {CharacterData.TypeMastery.Staff, new CharacterData.CharacterMasteryInfo{masteryRange = CharacterData.MasteryRange.N, masteryLevel = 0}}
    };
    public SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<int, CharacterData.CharacterSkillInfo>> skills = new SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<int, CharacterData.CharacterSkillInfo>>();
    public SerializedDictionary<string, AnimationsInfo> animations = new SerializedDictionary<string, AnimationsInfo>();
    private string[] defaultNames = { "Idle", "Walk", "TakeDamage", "Defend", "Lifted", "Lift", "Throw", "FistAttack", "SwordAttack", "SpearAttack", "BowAttack", "GunAttack", "AxeAttack", "StaffAttack" };
    public GenerateAllAnimations generateAllAnimations;

#if UNITY_EDITOR
    [NaughtyAttributes.Button]
    public void GenerateAllCharacterAnimations()
    {
        if (generateAllAnimations.atlas == null || generateAllAnimations.baseSprite == null) return;

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
    }
#endif

    [NaughtyAttributes.Button]
    public void RefreshSkillsData()
    {
        SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<int, CharacterData.CharacterSkillInfo>> clonedSkills = new SerializedDictionary<ItemBaseSO.TypeWeapon, SerializedDictionary<int, CharacterData.CharacterSkillInfo>>();
        for (int i = 0; i < skills.Count; i++)
        {
            SerializedDictionary<int, CharacterData.CharacterSkillInfo> innerSkills = new SerializedDictionary<int, CharacterData.CharacterSkillInfo>();
            for (int x = 0; x < skills.ElementAt(i).Value.Count; x++)
            {
                innerSkills.Add(skills.ElementAt(i).Value.ElementAt(x).Value.skillsBaseSO.skillId, new CharacterData.CharacterSkillInfo
                {
                    skillId = skills.ElementAt(i).Value.ElementAt(x).Value.skillsBaseSO.skillId,
                    skillsBaseSO = skills.ElementAt(i).Value.ElementAt(x).Value.skillsBaseSO,
                    level = skills.ElementAt(i).Value.ElementAt(x).Value.level,
                    statistics = skills.ElementAt(i).Value.ElementAt(x).Value.skillsBaseSO.CloneStatistics()
                });
            }
            clonedSkills.Add(skills.ElementAt(i).Key, innerSkills);
        }
        skills = clonedSkills;
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

        foreach (var kvp in mastery)
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
    public UnityEngine.Rendering.SerializedDictionary<ItemBaseSO.TypeWeapon, UnityEngine.Rendering.SerializedDictionary<int, CharacterData.CharacterSkillInfo>> CloneSkills()
    {
        var clone = new UnityEngine.Rendering.SerializedDictionary<ItemBaseSO.TypeWeapon, UnityEngine.Rendering.SerializedDictionary<int, CharacterData.CharacterSkillInfo>>();

        foreach (var kvp in skills)
        {
            var innerClone = new UnityEngine.Rendering.SerializedDictionary<int, CharacterData.CharacterSkillInfo>();

            foreach (var innerKvp in kvp.Value)
            {
                var statisticsClone = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
                foreach (var statKvp in innerKvp.Value.statistics)
                {
                    var statClone = new CharacterData.Statistic
                    {
                        aptitudeValue = statKvp.Value.aptitudeValue,
                        baseValue = statKvp.Value.baseValue,
                        buffValue = statKvp.Value.buffValue,
                        currentValue = statKvp.Value.currentValue,
                        itemValue = statKvp.Value.itemValue,
                        maxValue = statKvp.Value.maxValue,
                    };
                    statisticsClone.Add(statKvp.Key, statClone);
                }
                var skillInfoClone = new CharacterData.CharacterSkillInfo
                {
                    skillId = innerKvp.Value.skillsBaseSO.skillId,
                    skillsBaseSO = innerKvp.Value.skillsBaseSO,
                    statistics = statisticsClone,
                    level = innerKvp.Value.level,
                };

                innerClone.Add(innerKvp.Key, skillInfoClone);
            }
            clone.Add(kvp.Key, innerClone);
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
    }
}