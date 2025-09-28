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
    public SerializedDictionary<CharacterItemInfo, CharacterItem> items = new SerializedDictionary<CharacterItemInfo, CharacterItem>
    {
        {new CharacterItemInfo{index = 0, typeCharacterItem = TypeCharacterItem.Weapon}, null},
        {new CharacterItemInfo{index = 1, typeCharacterItem = TypeCharacterItem.Object1}, null},
        {new CharacterItemInfo{index = 2, typeCharacterItem = TypeCharacterItem.Object2}, null},
        {new CharacterItemInfo{index = 3, typeCharacterItem = TypeCharacterItem.Object3}, null}
    };
    [Serializable] public class Statistic
    {
        public int baseValue = 0;
        public int itemValue = 0;
        public SerializedDictionary<StatusEffectBaseSO, int> buffValue = new SerializedDictionary<StatusEffectBaseSO, int>();
        public int currentValue = 0;
        public int maxValue = 0;
        public void RefreshValue()
        {
            int baseWhitItem = baseValue + itemValue;
            int totalBuffValue = 0;
            foreach (KeyValuePair<StatusEffectBaseSO, int> buff in buffValue)
            {
                totalBuffValue += buff.Value;
            }
            int baseWhitBuff = baseValue * totalBuffValue / 100;
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
    public void GetCurrentWeapon(out CharacterItem weapon)
    {
        foreach (KeyValuePair<CharacterItemInfo, CharacterItem> item in items)
        {
            if (item.Value.itemBaseSO && item.Value.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Weapon)
            {
                weapon = item.Value;
                return;
            }
        }
        weapon = null;
    }
    [Serializable]public class CharacterItem
    {
        public int itemId;
        public ItemBaseSO itemBaseSO;
        public SerializedDictionary<TypeStatistic, Statistic> itemStatistics = new SerializedDictionary<TypeStatistic, Statistic>();
    }
    [Serializable] public class CharacterItemInfo
    {
        public int index;
        public TypeCharacterItem typeCharacterItem;
    }
    public enum TypeCharacterItem
    {
        None = 0,
        Weapon = 1,
        Object1 = 2,
        Object2 = 3,
        Object3 = 4,    
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
