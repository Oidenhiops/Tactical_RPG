using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersSkinDataDB", menuName = "ScriptableObjects/DB/CharactersSkinDataDB", order = 1)]
public class CharactersSkinDBSO : ScriptableObject
{
    public SerializedDictionary<int, SerializedDictionary<int, CharacterData.CharacterSkinData>> data = new SerializedDictionary<int, SerializedDictionary<int, CharacterData.CharacterSkinData>>();
}
