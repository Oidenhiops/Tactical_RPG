using System;
using System.Collections;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    public static GenerateMap Instance { get; private set; }
    public Sprite currentAtlasMap;
    public Block[] blocks;
    public bool showGizmos;
    public bool showHandles;
    public bool autoInit;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        if (autoInit) StartCoroutine(GenerateGrid());
    }
    public IEnumerator GenerateGrid()
    {
        if (blocks != null && blocks.Length > 0)
        {
            foreach (Block block in blocks)
            {
                if (block != null)
                {
                    AStarPathFinding.Instance.grid.Add(Vector3Int.RoundToInt(block.transform.position), new WalkablePositionInfo
                    {
                        pos = Vector3Int.RoundToInt(block.transform.position),
                        isWalkable = false,
                        hasCharacter = null,
                        blockInfo = block
                    });
                }
            }
            foreach (Block blockEvaluate in blocks)
            {
                AStarPathFinding.Instance.GetHighestBlockAt(Vector3Int.RoundToInt(blockEvaluate.transform.position), out WalkablePositionInfo block);
                if (block != null) block.isWalkable = true;
            }
            yield return null;
            DrawBlocks();
        }
    }
    public void DrawBlocks()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null)
            {
                blocks[i].DrawBlock();
            }
        }
    }
    // void OnDrawGizmos()
    // {
    //     if (AStarPathFinding.Instance && AStarPathFinding.Instance.grid != null && AStarPathFinding.Instance.grid.Count > 0 && showGizmos)
    //     {
    //         foreach (Block blockEvaluate in blocks)
    //         {
    //             if (AStarPathFinding.Instance.grid.TryGetValue(Vector3Int.RoundToInt(blockEvaluate.transform.position), out WalkablePositionInfo block))
    //             {
    //                 Gizmos.color = block.isWalkable ? block.hasCharacter ? Color.cyan : Color.green : Color.red;
    //                 Gizmos.DrawSphere(block.pos + Vector3.up * .25f, 0.1f);

    //                 if (showHandles)
    //                 {
    //                     GUIStyle labelStyle = new GUIStyle();
    //                     labelStyle.normal.textColor = Color.black;
    //                     labelStyle.fontStyle = FontStyle.Bold;
    //                     string state = block.isWalkable ? "Walkable" : "Blocked";
    //                     Handles.Label(
    //                         block.pos,
    //                         $"   {state}\n   {block.pos}",
    //                         labelStyle
    //                     );
    //                 }
    //             }
    //         }
    //     }
    // }
    [Serializable]
    public class WalkablePositionInfo
    {
        public Vector3Int pos;
        public bool isWalkable;
        public Character hasCharacter;
        public Block blockInfo;
    }
}