using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GenerateMap generateMap;
    public int mazeSize = 21;
    public GameObject blockPrefab;
    public GameObject spawnBlockPrefab;
    private int[,] maze;
    public int sizeCell;
    public bool testGeneration = true;
    void Start()
    {
        GenerateMaze();
        StartCoroutine(DrawMaze());
    }

    void GenerateMaze()
    {
        maze = new int[mazeSize, mazeSize];
        for (int x = 0; x < mazeSize; x++)
        {
            for (int y = 0; y < mazeSize; y++)
            {
                maze[x, y] = 0;
            }
        }
        CarvePassage(1, 1);
    }

    void CarvePassage(int x, int y)
    {
        Vector2Int[] directions = {
            new Vector2Int(0, 2),
            new Vector2Int(0, -2),
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0)
        };

        Shuffle(directions);

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (nx > 0 && ny > 0 && nx < mazeSize - 1 && ny < mazeSize - 1)
            {
                if (maze[nx, ny] == 0)
                {
                    maze[x + dir.x / 2, y + dir.y / 2] = 1;
                    maze[nx, ny] = 1;
                    CarvePassage(nx, ny);
                }
            }
        }
    }

    IEnumerator DrawMaze()
    {
        Vector3Int endPos = new Vector3Int();
        List<Block> blocks = new List<Block>();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int y = 0; y < mazeSize; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);

                if (maze[x, y] == 0)
                {
                    // Block blockDown = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                    // blocks.Add(blockDown);
                    // Block blockUp = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1) + Vector3.up, Quaternion.identity, transform).GetComponent<Block>();
                    // blocks.Add(blockUp);
                }
                else
                {
                    if (x == 1 && y == 1)
                    {
                        if (sizeCell <= 1)
                        {
                            Block block = Instantiate(spawnBlockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                            blocks.Add(block);
                        }
                        else
                        {
                            GetAroundPos(out List<Vector3Int> positions);
                            if (positions.Count > 0)
                            {
                                for (int i = 0; i < positions.Count; i++)
                                {
                                    if (positions[i] == Vector3Int.zero)
                                    {
                                        Block block = Instantiate(spawnBlockPrefab, (pos + new Vector3(-1, 0, -1)) * sizeCell + positions[i], Quaternion.identity, transform).GetComponent<Block>();
                                        blocks.Add(block);
                                    }
                                    else
                                    {
                                        Block block = Instantiate(blockPrefab, (pos + new Vector3(-1, 0, -1)) * sizeCell + positions[i], Quaternion.identity, transform).GetComponent<Block>();
                                        blocks.Add(block);
                                    }
                                }
                            }
                        }
                    }
                    else if (x == mazeSize - 2 && y == mazeSize - 2)
                    {
                        if (sizeCell <= 1)
                        {
                            Block block = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                            foreach (KeyValuePair<Block.TypeNeighbors, Block.MeshesInfo> mesh in block.meshes)
                            {
                                mesh.Value.meshRenderer.material.color = Color.red;
                            }
                            blocks.Add(block);
                        }
                        else
                        {
                            GetAroundPos(out List<Vector3Int> positions);
                            if (positions.Count > 0)
                            {
                                for (int i = 0; i < positions.Count; i++)
                                {
                                    Block block = Instantiate(blockPrefab, (pos + new Vector3(-1, 0, -1)) * sizeCell + positions[i], Quaternion.identity, transform).GetComponent<Block>();
                                    foreach (KeyValuePair<Block.TypeNeighbors, Block.MeshesInfo> mesh in block.meshes)
                                    {
                                        mesh.Value.meshRenderer.material.color = Color.red;
                                    }
                                    blocks.Add(block);
                                    if (i == positions.Count - 1)
                                    {
                                        Vector3Int a = Vector3Int.RoundToInt(pos) + new Vector3Int(-1, 0, -1);
                                        Vector3Int b = a * sizeCell;
                                        Vector3Int c = b + positions[i];
                                        endPos = c;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (sizeCell <= 1)
                        {
                            Block block = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                            blocks.Add(block);
                        }
                        else
                        {
                            GetAroundPos(out List<Vector3Int> positions);
                            if (positions.Count > 0)
                            {
                                for (int i = 0; i < positions.Count; i++)
                                {
                                    Block block = Instantiate(blockPrefab, (pos + new Vector3(-1, 0, -1)) * sizeCell + positions[i], Quaternion.identity, transform).GetComponent<Block>();
                                    blocks.Add(block);
                                }
                            }
                        }
                    }
                }
            }
        }
        generateMap.blocks = blocks.ToArray();
        yield return null;
        if (sizeCell <= 1)
        {
            generateMap.aStarPathFinding.limitX.x = -1;
            generateMap.aStarPathFinding.limitX.y = -mazeSize;
            generateMap.aStarPathFinding.limitZ.x = -1;
            generateMap.aStarPathFinding.limitZ.y = -mazeSize;
        }
        else
        {
            generateMap.aStarPathFinding.limitX.x = -(sizeCell - 1) / 2;
            generateMap.aStarPathFinding.limitX.y = endPos.x;
            generateMap.aStarPathFinding.limitZ.x = -(sizeCell - 1) / 2;
            generateMap.aStarPathFinding.limitZ.y = endPos.z;
        }
        if (testGeneration || generateMap.findMapInfo) _ = generateMap.GenerateGrid();
    }
    public List<Vector3Int> GetAroundPos(out List<Vector3Int> positions)
    {
        positions = new List<Vector3Int>();
        int middleValue = (sizeCell - 1) / 2;
        for (int x = -middleValue; x <= middleValue; x++)
        {
            for (int y = -middleValue; y <= middleValue; y++)
            {
                positions.Add(new Vector3Int(x, 0, y));
            }
        }
        return positions;
    }
    void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Vector2Int temp = array[i];
            int rand = Random.Range(i, array.Length);
            array[i] = array[rand];
            array[rand] = temp;
        }
    }
}