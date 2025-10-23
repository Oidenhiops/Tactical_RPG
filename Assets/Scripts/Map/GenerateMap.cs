using System;
using System.Threading.Tasks;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    public AStarPathFinding aStarPathFinding;
    public Sprite currentAtlasMap;
    public Block[] blocks;
    public bool isWorldMap;
    public bool findMapInfo;
    public Action OnFinishGenerateMap;
    void Start()
    {
        if (isWorldMap) _ = GenerateGrid();
        if (findMapInfo && ManagementBattleInfo.Instance) currentAtlasMap = ManagementBattleInfo.Instance.generateMap.currentAtlasMap;
    }
    public async Task GenerateGrid()
    {
        try
        {
            if (blocks != null && blocks.Length > 0)
            {
                foreach (Block block in blocks)
                {
                    if (block != null)
                    {
                        block.generateMap = this;
                        aStarPathFinding.grid.Add(Vector3Int.RoundToInt(block.transform.position), new WalkablePositionInfo
                        {
                            pos = Vector3Int.RoundToInt(block.transform.position),
                            isWalkable = true,
                            hasCharacter = null,
                            blockInfo = block
                        });
                    }
                }
                foreach (Block blockEvaluate in blocks)
                {
                    aStarPathFinding.GetHighestBlockAt(Vector3Int.RoundToInt(blockEvaluate.transform.position), out WalkablePositionInfo block);
                    if (block != null) block.isWalkable = true;
                }
                await Awaitable.NextFrameAsync();
                DrawBlocks();
                OnFinishGenerateMap?.Invoke();
                if (isWorldMap) aStarPathFinding.currentGrid = aStarPathFinding.grid;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating grid: {e.Message}");
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
    [Serializable]
    public class WalkablePositionInfo
    {
        public Vector3Int pos;
        public bool isWalkable;
        public CharacterBase hasCharacter;
        public Block blockInfo;
    }
}