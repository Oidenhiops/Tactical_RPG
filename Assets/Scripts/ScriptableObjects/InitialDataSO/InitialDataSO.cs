using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEditor;
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
    public SerializedDictionary<string, AnimationsInfo> animations = new SerializedDictionary<string, AnimationsInfo>();
    public AnimationsInfo newOrEditAnimation;
    public GenerateAllAnimations generateAllAnimations;
    [NaughtyAttributes.Button]
    public void NewAnimation()
    {
        animations.Add(newOrEditAnimation.name, newOrEditAnimation);
        newOrEditAnimation = new AnimationsInfo();
    }
    [NaughtyAttributes.Button]
    public void EditAnimation()
    {
        if (animations.ContainsKey(newOrEditAnimation.name))
        {
            animations[newOrEditAnimation.name] = newOrEditAnimation;
            newOrEditAnimation = new AnimationsInfo();
        }
        else
        {
            Debug.Log("No se encontro la animación, comprueba el nombre");
        }
    }
    [NaughtyAttributes.Button]
    public void GenerateAllCharacterAnimations()
    {
        if (generateAllAnimations.atlas == null || generateAllAnimations.baseSprite == null) return;

        Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(atlas)).OfType<Sprite>().ToArray();
        int spriteW = Mathf.RoundToInt(generateAllAnimations.baseSprite.rect.width);
        int indexSpriteForEvaluate = 0;
        int nameIndex = 0;
        int middleIndex;
        animations.Clear();
        while (true)
        {
            List<Sprite> row = new List<Sprite>();
            for (int i = 0; i < atlas.width / spriteW; i++)
            {
                if (allSprites[i + indexSpriteForEvaluate].rect.y != allSprites[indexSpriteForEvaluate].rect.y)
                {
                    break;
                }
                row.Add(allSprites[i + indexSpriteForEvaluate]);
            }
            middleIndex = row.Count / 2;
            AnimationsInfo animationInfo = new AnimationsInfo
            {
                name = nameIndex.ToString(),
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
            animations.Add(nameIndex.ToString(), animationInfo);
            nameIndex++;
            indexSpriteForEvaluate += row.Count;
            if (nameIndex >= atlas.height / spriteW)
            {
                break;
            }
        }
        atlas = generateAllAnimations.atlas;
        atlasHands = generateAllAnimations.atlasHands;
        icon = generateAllAnimations.icon;
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