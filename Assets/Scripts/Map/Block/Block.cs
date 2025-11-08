using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public GenerateMap generateMap;
    public TypeBlock typeBlock;
    public SerializedDictionary<TypeNeighbors, MeshesInfo> meshes;
    public int bitMask;
    public SerializedDictionary<int, BlocksInfo> renderInfo;
    public SerializedDictionary<Sprite, List<Sprite>> variationsSprites;
    [SerializeField] List<TypeNeighbors> neighbors = new List<TypeNeighbors>();
    public SerializedDictionary<Vector3Int, TypeNeighbors> directions;
    public GameObject poolingGrid;
    public bool cantWalk;
    public bool containsStairAround;
    public enum TypeBlock
    {
        None = 0,
        Block = 1,
        Spawn = 2,
        Stair = 3
    }
    public void DrawBlock()
    {
        CheckDirection();
    }
    int GetBitmask()
    {
        int mask = 0;
        foreach (var dir in neighbors)
        {
            mask |= 1 << (int)dir;
        }
        return mask;
    }
    [NaughtyAttributes.Button]
    public void CheckDirection()
    {
        neighbors = new List<TypeNeighbors>();
        containsStairAround = false;
        Vector3Int direction;
        foreach (KeyValuePair<Vector3Int, TypeNeighbors> value in directions)
        {
            direction = Vector3Int.RoundToInt(transform.position + value.Key);
            if (generateMap.aStarPathFinding.grid.ContainsKey(direction))
            {
                neighbors.Add(directions[GetDirection(Vector3Int.RoundToInt(transform.position), Vector3Int.RoundToInt(transform.position + value.Key))]);
                if (typeBlock != TypeBlock.Stair &&  generateMap.aStarPathFinding.grid[direction].blockInfo.typeBlock == TypeBlock.Stair)
                {
                    containsStairAround = true;
                }
            }
        }
        bitMask = GetBitmask();

        if (containsStairAround && !neighbors.Contains(TypeNeighbors.Up))
        {
            bitMask += 10000000;
        }

        if (renderInfo.TryGetValue(bitMask, out BlocksInfo blockInfo))
        {
            
            List<TypeNeighbors> directions = new List<TypeNeighbors>()
            {
                TypeNeighbors.Forward,
                TypeNeighbors.Back,
                TypeNeighbors.Left,
                TypeNeighbors.Right,
                TypeNeighbors.Up,
                TypeNeighbors.Down
            };

            for (int i = 0; i < directions.Count; i++)
            {
                if (!neighbors.Contains(directions[i]))
                {
                    if (meshes.ContainsKey(directions[i]))
                    {
                        meshes[directions[i]].meshRenderer.gameObject.SetActive(true);
                    }
                }
                else if (typeBlock != TypeBlock.Stair && !containsStairAround)
                {
                    if (meshes.ContainsKey(directions[i]))
                    {
                        meshes[directions[i]].meshRenderer.gameObject.SetActive(false);
                    }
                }
            }

            foreach (var meshInfo in meshes)
            {
                if (meshInfo.Value.meshRenderer.gameObject.activeSelf)
                {
                    SetTextureFromAtlas(GetVariationSprite(blockInfo.targetSprite), meshInfo.Value);
                }
            }
        }
        else if (BlockAddRuleManager.Instance && !BlockAddRuleManager.Instance.blockToAddRule.renderInfo.ContainsKey(bitMask)) BlockAddRuleManager.Instance.blockToAddRule.renderInfo.Add(bitMask, new BlocksInfo { targetSprite = null, blockGeneratedRule = this });
    }
    Sprite GetVariationSprite(Sprite originalSprite)
    {
        if (variationsSprites.TryGetValue(originalSprite, out List<Sprite> variations) && variations.Count > 0)
        {
            int randomIndex = Random.Range(0, variations.Count);
            return variations[randomIndex];
        }
        return originalSprite;
    }
    Vector3Int GetDirection(Vector3Int from, Vector3Int to)
    {
        Vector3Int diff = to - from;
        return new Vector3Int(
            diff.x == 0 ? 0 : (diff.x > 0 ? 1 : -1),
            diff.y == 0 ? 0 : (diff.y > 0 ? 1 : -1),
            diff.z == 0 ? 0 : (diff.z > 0 ? 1 : -1)
        );
    }
    void SetTextureFromAtlas(Sprite spriteFromAtlas, MeshesInfo meshesInfo)
    {
        Vector2[] uvs = meshesInfo.originalMesh.uv;
        Texture2D texture = spriteFromAtlas.texture;
        meshesInfo.meshRenderer.material.mainTexture = typeBlock == TypeBlock.Stair ? generateMap.currentAtlasMap.stairsTexture : generateMap.currentAtlasMap.blocksTexture;
        Rect spriteRect = spriteFromAtlas.rect; 
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i].x = Mathf.Lerp(spriteRect.x / texture.width, (spriteRect.x + spriteRect.width) / texture.width, uvs[i].x);
            uvs[i].y = Mathf.Lerp(spriteRect.y / texture.height, (spriteRect.y + spriteRect.height) / texture.height, uvs[i].y);
        }
        meshesInfo.meshRenderer.GetComponent<MeshFilter>().mesh.uv = uvs;
    }
    [System.Serializable] public class BlocksInfo
    {
        public Block blockGeneratedRule;
        public Sprite targetSprite;
    }
    [System.Serializable] public class MeshesInfo
    {
        public MeshRenderer meshRenderer;
        public Mesh originalMesh;
    }
    public enum TypeNeighbors
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4,
        Forward = 5,
        Back = 6,
        UpLeft = 7,
        UpRight = 8,
        DownLeft = 9,
        DownRight = 10,
        UpForward = 11,
        UpBack = 12,
        DownForward = 13,
        DownBack = 14,
        ForwardLeft = 15,
        ForwardRight = 16,
        BackLeft = 17,
        BackRight = 18,
    }
}
