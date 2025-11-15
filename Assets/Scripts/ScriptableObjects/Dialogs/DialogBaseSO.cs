using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "DialogText", menuName = "ScriptableObjects/Dialog/DialogText", order = 1)]
public class DialogBaseSO : ScriptableObject
{
    public List<DialogText> dialogLines = new List<DialogText>();
    [Serializable] public class DialogText
    {
        public List<TypeAnimate> typeAnimates;
        public string textId;
        public float timeBetweenChars = 0.05f;
        public List<BannerData> banners;
        public bool finishDialog;
        public DialogBaseSO otherDialog;
        public DialogFunctionBaseSO dialogFunction;
    }
    [Serializable]
    public class BannerData
    {
        public string bannerTextId;
        public DialogBaseSO newDialog;
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
