using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataDB", menuName = "ScriptableObjects/DB/CharacterDataDB", order = 1)]
public class CharacterDataDBSO : ScriptableObject
{
    public SerializedDictionary<int, List<CharacterInfo>> data = new SerializedDictionary<int, List<CharacterInfo>>();
    [System.Serializable] public class CharacterInfo
    {
        public bool isUnlocked;
        public InitialDataSO initialDataSO;
    }
}
