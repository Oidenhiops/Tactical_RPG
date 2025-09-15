using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "InitialData", menuName = "ScriptableObjects/Character/InitialDataSO", order = 1)]
public class InitialDataSO : ScriptableObject
{
    public bool isHumanoid;
    public Texture2D atlas;
    public Texture2D atlasHands;
    public Sprite icon;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> initialStats = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
    public SerializedDictionary<string, AnimationsInfo> animations = new SerializedDictionary<string, AnimationsInfo>();
    [Serializable]
    public class AnimationsInfo
    {
        public string name;
        public string linkAnimation;
        public SpritesInfo[] spritesInfoDown;
        public SpritesInfo[] spritesInfoUp;
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
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> CloneStatistics()
    {
        var clone = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();

        foreach (var kvp in initialStats)
        {
            clone[kvp.Key] = new CharacterData.Statistic
            {
                baseValue = kvp.Value.baseValue,
                itemValue = kvp.Value.itemValue,
                buffValue = kvp.Value.buffValue,
                maxValue = kvp.Value.maxValue,
                currentValue = kvp.Value.currentValue
            };
        }

        return clone;
    }
    public enum TypeAnimation
    {
        None = 0,
        General = 1,
        Attack = 2,
        Special = 3
    }
}