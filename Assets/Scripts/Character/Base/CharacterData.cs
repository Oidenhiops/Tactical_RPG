using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public int id;
    public int level;
    public string name;
    public SerializedDictionary<TypeStatistic, Statistic> statistics = new SerializedDictionary<TypeStatistic, Statistic>();
    public List<CharacterItems> items = new List<CharacterItems>();
    [Serializable]
    public class Statistic
    {
        public int baseValue = 0;
        public int itemValue = 0;
        public int buffValue = 0;
        public int currentValue = 0;
        public int maxValue = 0;
        public void RefreshValue()
        {
            int baseWhitItem = baseValue + itemValue;
            int baseWhitBuff = baseValue * buffValue / 100;
            int finalValue = Mathf.RoundToInt(baseWhitItem + baseWhitBuff);
            maxValue = Mathf.Clamp(finalValue, 1, 99999);
            if (currentValue > maxValue) currentValue = maxValue;
        }
        public void SetMaxValue()
        {
            currentValue = maxValue;
        }
    }
    public int GetMovementMaxHeight()
    {
        //return 2 + statistics[TypeStatistic.Spd].currentValue > 6 ? 6 : statistics[TypeStatistic.Spd].currentValue;
        return 0;
    }
    public int GetMovementRadius()
    {
        return statistics[TypeStatistic.Spd].currentValue > 10 ? 10 : statistics[TypeStatistic.Spd].currentValue;
    }
    public int GetThrowRadius()
    {
        return 4 + statistics[TypeStatistic.Spd].currentValue > 6 ? 6 : statistics[TypeStatistic.Spd].currentValue;
    }
    [Serializable]
    public class CharacterItems
    {
        public int id;
        public SerializedDictionary<TypeStatistic, Statistic> statistics;
    }
    public enum TypeStatistic
    {
        None = 0,
        Hp = 1,
        Sp = 2,
        Atk = 3,
        Hit = 4,
        Int = 5,
        Def = 6,
        Res = 7,
        Spd = 8,
        Exp = 9,
    }
}
