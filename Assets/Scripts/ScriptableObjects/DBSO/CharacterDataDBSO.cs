using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataDB", menuName = "ScriptableObjects/DB/CharacterDataDB", order = 1)]
public class CharacterDataDBSO : ScriptableObject
{
    public SerializedDictionary<int, InitialDataSO> data = new SerializedDictionary<int, InitialDataSO>();
}
