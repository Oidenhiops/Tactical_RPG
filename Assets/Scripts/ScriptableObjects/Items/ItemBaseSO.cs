using AYellowpaper.SerializedCollections;
using UnityEngine;

public class ItemBaseSO : ScriptableObject
{
    public int id;
    public Sprite icon;
    public TypeObject typeObject;
    public string animationName;
    public int amountToAddId;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> itemStatistics = new SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic>();
    public virtual void EquipItem(Character character, CharacterData.CharacterItem characterItem) { Debug.LogError("EquipItem not implemented"); }
    public virtual void DesEquipItem(Character character, CharacterData.CharacterItem characterItem) { Debug.LogError("DesEquipItem not implemented"); }
    [NaughtyAttributes.Button] public void AddToId()
    {
        id += amountToAddId;
    }
    public enum TypeObject
    {
        None = 0,
        Weapon = 1,
        Monster = 2,
        Item = 3,
        Consumable = 4
    }
}
