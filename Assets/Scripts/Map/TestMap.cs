using System.Collections;
using UnityEngine;

public class TestMap : MonoBehaviour
{
    public bool drawMap;
    public void Start()
    {
        if (drawMap) DrawMap();
    }
    [NaughtyAttributes.Button]
    public void DrawMap()
    {
        GenerateMap.Instance.GenerateGrid();
    }
}
