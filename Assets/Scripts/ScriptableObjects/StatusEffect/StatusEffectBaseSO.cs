using AYellowpaper.SerializedCollections;
using UnityEngine;

public class StatusEffectBaseSO : ScriptableObject
{
    public Sprite icon;
    public int maxStats;
    public bool isPermanent;
    public SerializedDictionary<CharacterData.TypeStatistic, CharacterData.Statistic> statusEffectStatistics;
    public virtual void ApplyEffect(CharacterBase character) { Debug.LogError("ApplyEffect no implemented"); }
    public virtual void ReApplyEffect(CharacterBase character) { Debug.LogError("ReApplyEffect no implemented"); }
    public virtual void DiscountEffect(CharacterBase character) { Debug.LogError("DiscountEffect no implemented"); }
    public virtual void ReloadEffect(CharacterBase character) { Debug.LogError("ReloadEffect no implemented"); }
    public virtual void RemoveEffect(CharacterBase character) { Debug.LogError("RemoveEffect no implemented"); }
}
