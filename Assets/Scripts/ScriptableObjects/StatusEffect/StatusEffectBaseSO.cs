using AYellowpaper.SerializedCollections;
using UnityEngine;

public class StatusEffectBaseSO : ScriptableObject
{
    public Sprite icon;
    public int maxStats;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> statusEffectStatistics;
    public virtual void ApplyEffect(Character character) { Debug.LogError("ApplyEffect no implemented"); }
    public virtual void ReApplyEffect(Character character) { Debug.LogError("ReApplyEffect no implemented"); }
    public virtual void DiscountEffect(Character character) { Debug.LogError("DiscountEffect no implemented"); }
    public virtual void ReloadEffect(Character character) { Debug.LogError("ReloadEffect no implemented"); }
    public virtual void RemoveEffect(Character character) { Debug.LogError("RemoveEffect no implemented"); }
}
