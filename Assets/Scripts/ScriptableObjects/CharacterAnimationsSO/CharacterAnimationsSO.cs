using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAnimations", menuName = "ScriptableObjects/Character/CharacterAnimationsSO", order = 1)]
public class CharacterAnimationsSO : ScriptableObject
{
    public Texture2D atlas;
    public Texture2D atlasHands;
    public SerializedDictionary<string, AnimationsInfo> animations = new SerializedDictionary<string, AnimationsInfo>();

    [Serializable] public class AnimationsInfo
    {
        public string linkAnimation;
        public SpritesInfo[] spritesInfoDown;
        public SpritesInfo[] spritesInfoUp;
        public bool loop = false;
        public bool needInstance = false;
        public int frameToInstance = 0;
        public GameObject instanceObj;
        public GameObject instance;
    }
    [Serializable] public class SpritesInfo
    {
        public Sprite characterSprite;
        public Sprite handSprite;
        public Vector3 leftHandPosDL;
        public Vector3 leftHandPosDR;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosDL;
        public Vector3 rightHandPosDR;
        public Quaternion rightHandRotation;
    }
}