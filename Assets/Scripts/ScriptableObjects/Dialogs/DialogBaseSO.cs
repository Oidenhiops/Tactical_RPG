using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "DialogBase", menuName = "ScriptableObjects/Dialog/DialogBase", order = 1)]
public class DialogBaseSO : ScriptableObject
{
    public List<DialogText> dialogLines = new List<DialogText>();
    [Serializable] public class DialogText
    {
        public List<TypeAnimate> typeAnimates;
        public string textId;
        public float timeBetweenChars = 0.05f;
        public List<BannerData> banners;
    }
    [Serializable]
    public class BannerData
    {
        public string bannerTextId;
        public string bannerFunction;
    }
    public void MakeBannerFunction(string functionName)
    {
        
    }
    public enum TypeAnimate
    {
        None,
        Bounce,
        Dangle,
        Rainb,
        Rot,
        Shake,
        Incr,
        Slide,
        Swing,
        Wave,
        Wiggle
    }
}
