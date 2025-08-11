using System.Collections;
using UnityEngine;

public class TestMap : MonoBehaviour
{
    public bool drawMap;
    public Character character;
    public void Start()
    {
        if (drawMap) DrawMap();
    }
    [NaughtyAttributes.Button]
    public void DrawMap()
    {
        GenerateMap.Instance.GenerateGrid();
        StartCoroutine(SetCharacterToSpawn());
    }
    [NaughtyAttributes.Button]
    public void EnableGrid()
    {
        AStarPathFinding.Instance.EnableGrid(character);
    }
    [NaughtyAttributes.Button]
    public void DisableGrid()
    {
        AStarPathFinding.Instance.DisableGrid();
    }
    IEnumerator SetCharacterToSpawn()
    {
        while (true)
        {
            if (AStarPathFinding.Instance.grid.TryGetValue(new Vector3Int(0, 0, 0), out GenerateMap.WalkablePositionInfo block))
            {
                block.hasCharacter = character;
                break;
            }
            yield return null;
        }
    }
}
