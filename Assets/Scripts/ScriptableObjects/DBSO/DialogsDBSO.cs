using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogsDB", menuName = "ScriptableObjects/DB/DialogsDB", order = 1)]
public class DialogsDBSO : ScriptableObject
{
    public SerializedDictionary<int, SerializedDictionary<int, DialogBaseSO>> data = new SerializedDictionary<int, SerializedDictionary<int, DialogBaseSO>>();
}
