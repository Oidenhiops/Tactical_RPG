using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 21;
    public int height = 21;
    public GameObject blockPrefab;
    public GameObject spawnBlockPrefab;
    private int[,] maze;

    void Start()
    {
        GenerateMaze();
        StartCoroutine(DrawMaze());
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1)
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
        List<Block> blocks = new List<Block>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);

                if (maze[x, y] == 0)
                {
                    Block blockDown = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                    Block blockUp = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1) + Vector3.up, Quaternion.identity, transform).GetComponent<Block>();
                    blocks.Add(blockDown);
                    blocks.Add(blockUp);
                }
                else
                {
                    if (x == 1 && y == 1)
                    {
                        Block block = Instantiate(spawnBlockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                        blocks.Add(block);
                    }
                    else if (x == width - 2 && y == height - 2)
                    {
                        Block block = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                        block.GetComponent<Renderer>().material.color = Color.red;
                        blocks.Add(block);
                    }
                    else
                    {
                        Block block = Instantiate(blockPrefab, pos + new Vector3(-1, 0, -1), Quaternion.identity, transform).GetComponent<Block>();
                        blocks.Add(block);                        
                    }
                }
            }
        }
        GenerateMap.Instance.blocks = blocks.ToArray();
        yield return null;
        StartCoroutine(GenerateMap.Instance.GenerateGrid());
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