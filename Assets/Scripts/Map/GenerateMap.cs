using System;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    public static GenerateMap Instance { get; private set; }
    public Transform[] blocks;
    public WalkablePositionInfo[,] grid;
    public bool showGizmos;
    public bool showHandles;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void GenerateGrid()
    {
        if (blocks == null || blocks.Length == 0)
        {
            return;
        }

        var xs = blocks.Select(b => Mathf.RoundToInt(b.position.x)).Distinct().OrderBy(v => v).ToList();
        var zs = blocks.Select(b => Mathf.RoundToInt(b.position.z)).Distinct().OrderBy(v => v).ToList();

        int width = xs.Count;
        int height = zs.Count;

        grid = new WalkablePositionInfo[width, height];

        foreach (var block in blocks)
        {
            int xIndex = xs.IndexOf(Mathf.RoundToInt(block.position.x));
            int zIndex = zs.IndexOf(Mathf.RoundToInt(block.position.z));

            grid[xIndex, zIndex] = new WalkablePositionInfo
            {
                pos = Vector3Int.RoundToInt(block.position),
                isWalkable = true,
                hasCharacter = null,
                blockInfo = block.GetComponent<Block>()
            };
        }
        AStarPathFinding.Instance.grid = ConvertToDictionary(grid);
    }

    SerializedDictionary<Vector3Int, WalkablePositionInfo> ConvertToDictionary(WalkablePositionInfo[,] arrayGrid)
    {
        var dict = new SerializedDictionary<Vector3Int, WalkablePositionInfo>();

        int width = arrayGrid.GetLength(0);
        int height = arrayGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var tile = arrayGrid[x, z];
                if (tile != null)
                {
                    dict.Add(tile.pos, tile);
                }
            }
        }

        return dict;
    }

    [Serializable] public class WalkablePositionInfo
    {
        public Vector3Int pos;
        public bool isWalkable;
        public Character hasCharacter;
        public Block blockInfo;
    }
    void OnDrawGizmos()
    {
        if (grid != null && showGizmos)
        {
            foreach (var block in grid)
            {
                if (block != null)
                {
                    Gizmos.color = block.isWalkable ? block.hasCharacter ? Color.cyan : Color.green : Color.red;
                    Gizmos.DrawSphere(block.pos + Vector3.up * .25f, 0.1f);

                    if (showHandles)
                    {
                        GUIStyle labelStyle = new GUIStyle();
                        labelStyle.normal.textColor = Color.black;
                        labelStyle.fontStyle = FontStyle.Bold;
                        string state = block.isWalkable ? "Walkable" : "Blocked";
                        Handles.Label(
                            block.pos,
                            $"   {state}\n   {block.pos}",
                            labelStyle
                        );
                    }
                }
            }
        }
    }
}