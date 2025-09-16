using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public TypeBlock typeBlock;
    public SerializedDictionary<TypeNeighbors, MeshesInfo> meshes;
    public int bitMask;
    public SerializedDictionary<int, BlocksInfo> renderInfo;
    [SerializeField] List<TypeNeighbors> neighbors = new List<TypeNeighbors>();
    public SerializedDictionary<Vector3Int, TypeNeighbors> directions;
    public GameObject poolingGrid;
    public enum TypeBlock
    {
        None = 0,
        Normal = 1,
        Spawn = 2
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
        Vector3Int direction;
        foreach (KeyValuePair<Vector3Int, TypeNeighbors> value in directions)
        {
            direction = Vector3Int.RoundToInt(transform.position + value.Key);
            if (AStarPathFinding.Instance.grid.ContainsKey(direction))
            {
                neighbors.Add(directions[GetDirection(Vector3Int.RoundToInt(transform.position), Vector3Int.RoundToInt(transform.position + value.Key))]);
            }
        }
        if (renderInfo.TryGetValue(GetBitmask(), out BlocksInfo blockInfo))
        {
            bitMask = GetBitmask();
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
                    meshes[directions[i]].meshRenderer.gameObject.SetActive(true);
                    SetTextureFromAtlas(blockInfo.targetSprite, meshes[directions[i]]);
                }
            }
        }
        else if (BlockAddRuleManager.Instance && !BlockAddRuleManager.Instance.blockToAddRule.renderInfo.ContainsKey(GetBitmask())) BlockAddRuleManager.Instance.blockToAddRule.renderInfo.Add(GetBitmask(), new BlocksInfo { targetSprite = null, blockGeneratedRule = this });
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
        meshesInfo.meshRenderer.material.mainTexture = GenerateMap.Instance.currentAtlasMap.texture;
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
