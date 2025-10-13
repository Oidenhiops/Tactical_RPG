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
    [NaughtyAttributes.Button]
    public void SortItems()
    {
        data = new SerializedDictionary<int, ItemBaseSO>(
            data.OrderBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
    [NaughtyAttributes.Button]
    public void SetItemIdText()
    {
        for (int i = 0; i < data.Count; i++)
        {
            var item = data.ElementAt(i).Value;
            string idText = System.Text.RegularExpressions.Regex.Replace(item.name, @"([a-z])([A-Z])", "$1_$2");
            idText = idText.Replace(" ", "_").ToLower();
            data.ElementAt(i).Value.idText = $"item_{idText}";
        }
    }
    [NaughtyAttributes.Button]
    public void SetNewId()
    {
        for (int i = 0; i < data.Count; i++)
        {
            data.ElementAt(i).Value.id += -1;
        }
    }
}
