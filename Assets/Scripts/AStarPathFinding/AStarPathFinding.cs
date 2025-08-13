using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    public static AStarPathFinding Instance { get; private set; }
    public SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo> _currentGrid = new SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo>();
    public Action<SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo>> OnCurrentGridChange;
    public SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo> currentGrid
    {
        get => _currentGrid;
        set
        {
            if (!_currentGrid.SequenceEqual(value))
            {
                _currentGrid = value;
                if (_currentGrid.Count > 0) _ToggleGrid = StartCoroutine(ToggleGrid(_currentGrid));
                else StopCoroutine(_ToggleGrid);
                OnCurrentGridChange?.Invoke(_currentGrid);
            }
        }
    }
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> grid = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
    public Character characterSelected;
    public int limitX = 10, limitZ = 10;
    public Character characterTest;
    Coroutine _ToggleGrid;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void EnableGrid(Character character)
    {
        currentGrid = GetWalkableTiles(character.lastPositionInGrid, character.statistics[Character.TypeStatistic.Spd].currentValue);
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> cell in currentGrid)
        {
            cell.Value.blockInfo.blockGrid.SetActive(true);
        }
    }
    public void DisableGrid()
    {
        characterSelected = null;
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> cell in currentGrid)
        {
            cell.Value.blockInfo.blockGrid.SetActive(false);
        }
        currentGrid = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
    }
    IEnumerator ToggleGrid(SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo> grid)
    {
        bool isShow = true;
        bool update = true;
        float elapsedTime = 0;
        float totalTime = 1;
        while (true)
        {
            if (isShow && update)
            {
                totalTime = 1f;
                update = false;
                foreach (KeyValuePair<Vector3Int ,GenerateMap.WalkablePositionInfo> blockGrid in grid)
                {
                    blockGrid.Value.blockInfo.blockGrid.SetActive(true);
                }
            }
            else if (!isShow && update)
            {
                totalTime = 0.25f;
                update = false;
                foreach (var blockGrid in grid)
                {
                    blockGrid.Value.blockInfo.blockGrid.SetActive(false);
                }
            }
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= totalTime)
            {
                elapsedTime = 0;
                update = true;
                isShow = !isShow;
            }
            yield return null;
        }
    }
    public void ValidateAction(Vector3Int pointerPos)
    {
        if (grid.TryGetValue(pointerPos, out GenerateMap.WalkablePositionInfo block))
        {
            if (block.blockInfo.typeBlock == Block.TypeBlock.Normal)
            {
                if (block.hasCharacter)
                {
                    characterSelected = block.hasCharacter;
                    EnableGrid(block.hasCharacter);
                    print("Personaje encontrado");
                }
                else if (characterSelected && characterSelected.isCharacterPlayer)
                {
                    PlayerManager.Instance.actionsManager.AddAction(new ActionsManager.ActionInfo(characterSelected, ActionsManager.TypeAction.Move, pointerPos));
                    characterSelected.MoveCharacter(pointerPos);
                }
                else
                {
                    print("No hay nada en el bloque wey");
                }
            }
            else if (block.blockInfo.typeBlock == Block.TypeBlock.Spawn)
            {
                if (block.hasCharacter)
                {
                    characterSelected = block.hasCharacter;
                    EnableGrid(block.hasCharacter);
                    print("Personaje encontrado");
                }
                else if (characterSelected && characterSelected.isCharacterPlayer)
                {
                    characterSelected.MoveCharacter(pointerPos);
                }
                else
                {
                    characterTest.gameObject.SetActive(true);
                    characterSelected = characterTest;
                    EnableGrid(characterSelected);
                    grid[Vector3Int.zero].hasCharacter = characterTest;
                    PlayerManager.Instance.actionsManager.AddAction(new ActionsManager.ActionInfo(characterTest, ActionsManager.TypeAction.Spawn, Vector3Int.zero));
                    print("Spawn encontrado yei");
                }
            }
        }
        else
        {
            print("Estás en el vacío wey");
        }
    }
    public SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo> GetWalkableTiles(Vector3Int startPos, int radius)
    {
        SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo> availablePositions = new SerializedDictionary<Vector3Int ,GenerateMap.WalkablePositionInfo>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                Vector3Int checkPos = new Vector3Int(startPos.x + x, startPos.y, startPos.z + z);

                if (Vector2Int.Distance(
                        new Vector2Int(startPos.x, startPos.z),
                        new Vector2Int(checkPos.x, checkPos.z)) > radius)
                    continue;
                
                if (GetHighestBlockAt(checkPos.x, checkPos.z, out GenerateMap.WalkablePositionInfo block) && block.pos.y <= characterSelected.GetMaxHeightToUp())
                {
                    if (block.isWalkable && !block.hasCharacter || block.isWalkable && block.hasCharacter && block.hasCharacter.isCharacterPlayer)
                    {
                        availablePositions.Add(block.pos, block);
                    }
                }
                else
                {
                    Debug.Log($"No existe en el grid: {checkPos}");
                }
            }
        }
        return availablePositions;
    }
    public bool GetHighestBlockAt(int x, int z, out GenerateMap.WalkablePositionInfo block)
    {
        block = grid
            .Where(kv => kv.Key.x == x && kv.Key.z == z)
            .OrderByDescending(kv => kv.Key.y)
            .FirstOrDefault().Value;

        return block != null;
    }
    public List<Vector3Int> FindPath(Vector3 startPos, Vector3 endPos)
    {
        Vector3Int start = Vector3Int.FloorToInt(startPos);
        Vector3Int end = Vector3Int.FloorToInt(endPos);

        if (!currentGrid.TryGetValue(start, out var startTile) || !startTile.isWalkable ||
            !currentGrid.TryGetValue(end, out var endTile) || !endTile.isWalkable)
        {
            return null;
        }
        var openList = new List<Node>();
        var closedList = new HashSet<Node>();
        Node startNode = new Node(start.x, start.y, start.z);
        Node endNode = new Node(end.x, end.y, end.z);
        openList.Add(startNode);
        while (openList.Count > 0)
        {
            openList.Sort((a, b) => a.F.CompareTo(b.F));
            Node currentNode = openList[0];

            if (currentNode.Equals(endNode))
                return ReconstructPath(currentNode);
            openList.RemoveAt(0);
            closedList.Add(currentNode);

            foreach (var neighbor in GetNeighbors(currentNode))
            {
                Vector3Int neighborPos = new Vector3Int(neighbor.X, neighbor.Y, neighbor.Z);
                if (!currentGrid.TryGetValue(neighborPos, out var neighborTile) || !neighborTile.isWalkable)
                    continue;
                if (closedList.Contains(neighbor))
                    continue;
                int tentativeG = currentNode.G + 1;
                if (!openList.Contains(neighbor) || tentativeG < neighbor.G)
                {
                    neighbor.G = tentativeG;
                    neighbor.H = Mathf.Abs(neighbor.X - endNode.X)
                               + Mathf.Abs(neighbor.Y - endNode.Y)
                               + Mathf.Abs(neighbor.Z - endNode.Z);
                    neighbor.F = neighbor.G + neighbor.H;
                    neighbor.Parent = currentNode;
                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        return null;
    }
    List<Node> GetNeighbors(Node node)
    {
        var neighbors = new List<Node>();
        var directions = new (int x, int z)[]
        {
        ( 1, 0), (-1, 0),
        ( 0, 1), ( 0, -1)
        };
        foreach (var dir in directions)
        {
            int targetX = node.X + dir.x;
            int targetZ = node.Z + dir.z;
            if (GetHighestBlockAt(targetX, targetZ, out var highestBlock))
            {
                if (!highestBlock.isWalkable)
                    continue;

                neighbors.Add(new Node(highestBlock.pos.x, highestBlock.pos.y, highestBlock.pos.z));
            }
        }
        return neighbors;
    }
    List<Vector3Int> ReconstructPath(Node node)
    {
        var path = new List<Vector3Int>();
        while (node != null)
        {
            Vector3Int pos = new Vector3Int(node.X, node.Y, node.Z);
            if (currentGrid.TryGetValue(pos, out var tile))
                path.Add(tile.pos);
            else
                path.Add(pos);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }
    private class Node
    {
        public int X, Y, Z;
        public int G, H, F;
        public Node Parent;
        public Node(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public override bool Equals(object obj)
        {
            return obj is Node other && other.X == X && other.Y == Y && other.Z == Z;
        }
        public override int GetHashCode()
        {
            return (X, Y, Z).GetHashCode();
        }
    }
}