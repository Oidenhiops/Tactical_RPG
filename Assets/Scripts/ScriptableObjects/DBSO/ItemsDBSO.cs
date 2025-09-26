using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemsDataDB", menuName = "ScriptableObjects/DB/ItemsDataDB", order = 1)]
public class ItemsDBSO : ScriptableObject
{
    public SerializedDictionary<int, ItemBaseSO> data = new SerializedDictionary<int, ItemBaseSO>();
    public ItemBaseSO[] itemsToAdd;
    [NaughtyAttributes.Button]
    public void AddNewItems()
    {
        for (int i = 0; i < itemsToAdd.Length; i++)
        {
            data.Add(itemsToAdd[i].id, itemsToAdd[i]);
        }
    }
    [NaughtyAttributes.Button] public void SortItems()
    {
        data = new SerializedDictionary<int, ItemBaseSO>(
            data.OrderBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
}
