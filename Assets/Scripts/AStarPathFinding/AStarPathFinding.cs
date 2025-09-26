using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    public static AStarPathFinding Instance { get; private set; }
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> currentGrid;
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> grid = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
    [SerializeField] GameObject poolinGrid;
    [SerializeField] Transform gridContainer;
    [SerializeField] private int poolSize = 50;
    private Queue<GameObject> pool = new Queue<GameObject>();
    public Character characterSelected;
    public Vector2Int limitX = new Vector2Int(-10, 10), limitZ = new Vector2Int(-10, 10);
    public Material gridMaterial;
    Coroutine _ToggleGrid;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(poolinGrid, gridContainer);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }
    }
    public GameObject GetPoolingGrid()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(poolinGrid, gridContainer);
            return obj;
        }
    }
    public void ReturnPoolingGridToQueue(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
    public void EnableGrid(SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> gridToEnable, Color gridColor)
    {
        if (_ToggleGrid != null)
        {
            StopCoroutine(_ToggleGrid);
        }
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> cell in currentGrid)
        {
            ReturnPoolingGridToQueue(cell.Value.blockInfo.poolingGrid);
            cell.Value.blockInfo.poolingGrid = null;
        }
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> cell in gridToEnable)
        {
            cell.Value.blockInfo.poolingGrid = GetPoolingGrid();
            cell.Value.blockInfo.poolingGrid.transform.position = cell.Value.blockInfo.transform.position + Vector3.up * 0.1f;
        }
        gridMaterial.color = gridColor;
        currentGrid = gridToEnable;
        _ToggleGrid = StartCoroutine(ToggleGrid());
    }
    public void DisableGrid()
    {
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> cell in currentGrid)
        {
            ReturnPoolingGridToQueue(cell.Value.blockInfo.poolingGrid);
            cell.Value.blockInfo.poolingGrid = null;
        }
        currentGrid = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
    }
    IEnumerator ToggleGrid()
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
                foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> blockGrid in currentGrid)
                {
                    blockGrid.Value.blockInfo.poolingGrid.SetActive(true);
                }
            }
            else if (!isShow && update)
            {
                totalTime = 0.25f;
                update = false;
                foreach (var blockGrid in currentGrid)
                {
                    blockGrid.Value.blockInfo.poolingGrid.SetActive(false);
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
                    if (currentGrid.Count == 0 || grid[pointerPos].hasCharacter != characterSelected)
                    {
                        characterSelected = block.hasCharacter;
                        if (characterSelected.isCharacterPlayer)
                        {
                            if (LastCharacterActionPermitActions())
                            {
                                EnableGrid(GetWalkableTiles(), Color.magenta);
                            }
                            else StartCoroutine(PlayerManager.Instance.menuCharacterActions.EnableMenu());
                        }
                        else
                        {
                            PlayerManager.Instance.menuCharacterInfo.ReloadInfo(characterSelected);
                        }
                    }
                    else
                    {
                        StartCoroutine(PlayerManager.Instance.menuCharacterActions.EnableMenu());
                    }
                }
                else if (characterSelected && characterSelected.isCharacterPlayer)
                {
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
                    if (currentGrid.Count == 0 || grid[pointerPos].hasCharacter != characterSelected)
                    {
                        characterSelected = block.hasCharacter;
                        if (LastCharacterActionPermitActions())
                        {
                            EnableGrid(GetWalkableTiles(), Color.magenta);
                        }
                        else
                        {
                            StartCoroutine(PlayerManager.Instance.menuCharacterActions.EnableMenu());
                        }
                    }
                    else
                    {
                        StartCoroutine(PlayerManager.Instance.menuCharacterActions.EnableMenu());
                    }
                }
                else if (characterSelected && characterSelected.isCharacterPlayer)
                {
                    characterSelected.MoveCharacter(pointerPos);
                }
                else
                {
                    StartCoroutine(PlayerManager.Instance.menuCharacterSelector.EnableMenu());
                }
            }
        }
        else
        {
            print("Estás en el vacío wey");
        }
    }
    public bool LastCharacterActionPermitActions()
    {
        return characterSelected.lastAction != ActionsManager.TypeAction.Lift &&
                characterSelected.lastAction != ActionsManager.TypeAction.Attack &&
                characterSelected.lastAction != ActionsManager.TypeAction.Defend &&
                characterSelected.lastAction != ActionsManager.TypeAction.Item &&
                characterSelected.lastAction != ActionsManager.TypeAction.EndTurn;
    }
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> GetWalkableTiles()
    {
        SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> availablePositions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
        Vector2Int startPos = new Vector2Int(characterSelected.startPositionInGrid.x, characterSelected.startPositionInGrid.z);
        Vector2Int checkPos = new Vector2Int();
        int radius = characterSelected.characterData.GetMovementRadius();
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                checkPos.x = startPos.x + x;
                checkPos.y = startPos.y + z;

                if (Vector2Int.Distance(startPos, checkPos) <= radius &&
                    GetHighestBlockAt(checkPos.x, checkPos.y, out GenerateMap.WalkablePositionInfo block) &&
                    block.pos.y <= characterSelected.positionInGrid.y + characterSelected.characterData.GetMovementMaxHeight())
                {
                    if (block.isWalkable && !block.hasCharacter || block.isWalkable && block.hasCharacter && block.hasCharacter.isCharacterPlayer)
                    {
                        availablePositions.Add(block.pos, block);
                    }
                }
            }
        }
        return availablePositions;
    }
    public bool GetHighestBlockAt(Vector3Int pos, out GenerateMap.WalkablePositionInfo block)
    {
        Vector3Int posToValidate = new Vector3Int();
        for (int i = 10; i >= 0; i--)
        {
            posToValidate.x = pos.x;
            posToValidate.y = i;
            posToValidate.z = pos.z;
            if (grid.ContainsKey(posToValidate))
            {
                block = grid[posToValidate];
                return true;
            }
        }
        block = null;
        return false;
    }
    public bool GetHighestBlockAt(int x, int z, out GenerateMap.WalkablePositionInfo block)
    {
        Vector3Int posToValidate = new Vector3Int();
        for (int i = 10; i >= 0; i--)
        {
            posToValidate.x = x;
            posToValidate.y = i;
            posToValidate.z = z;
            if (grid.ContainsKey(posToValidate))
            {
                block = grid[posToValidate];
                return true;
            }
        }
        block = null;
        return false;
    }
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> GetTilesToThrow()
    {
        SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> availablePositions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
        Vector3Int startPos = characterSelected.positionInGrid;
        int radius = characterSelected.characterData.GetThrowRadius();

        Vector3Int[] directions = new Vector3Int[]{
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.right,
        };
        for (int i = 0; i < directions.Length; i++)
        {
            for (int x = 1; x < radius; x++)
            {
                Vector3Int checkPos = startPos + directions[i] * x;
                if (GetHighestBlockAt(checkPos, out GenerateMap.WalkablePositionInfo block) && block.pos.y <= characterSelected.positionInGrid.y + characterSelected.characterData.GetMovementMaxHeight())
                {
                    if (block.isWalkable && !block.hasCharacter || block.isWalkable && block.hasCharacter && block.hasCharacter.isCharacterPlayer)
                    {
                        availablePositions.Add(block.pos, block);
                    }
                }
            }
        }
        if (GetHighestBlockAt(startPos, out GenerateMap.WalkablePositionInfo startBlock))
        {
            availablePositions.Add(startBlock.pos, startBlock);
        }
        return availablePositions;
    }
    public bool GetPositionsToLift(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions)
    {
        positions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.right
        };
        foreach (var directionFounded in directions)
        {
            Vector3Int direction = directionFounded + characterSelected.positionInGrid;
            if (GetHighestBlockAt(direction, out GenerateMap.WalkablePositionInfo block) && MathF.Abs(block.pos.y - characterSelected.positionInGrid.y) <= 2 && block.hasCharacter)
            {
                positions.Add(direction, block);
            }
        }
        return positions.Count > 0;
    }
    public bool GetPositionsToAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions)
    {
        positions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
        Vector3Int[] directions = new Vector3Int[0];
        characterSelected.characterData.GetCurrentWeapon(out CharacterData.CharacterItem weapon);
        // if (!weapon || weapon.typeObject == ItemBaseSO.TypeObject.Fist || weapon.typeObject == ItemBaseSO.TypeObject.Staff)
        // {
        //     directions = new Vector3Int[]{
        //         Vector3Int.forward,
        //         Vector3Int.back,
        //         Vector3Int.left,
        //         Vector3Int.right,
        //     };
        // }
        if (directions.Length > 0)
        {
            foreach (var directionFounded in directions)
            {
                Vector3Int direction = directionFounded + characterSelected.positionInGrid;
                if (GetHighestBlockAt(direction, out GenerateMap.WalkablePositionInfo block) && MathF.Abs(block.pos.y - characterSelected.positionInGrid.y) <= 2 && block.hasCharacter)
                {
                    positions.Add(direction, block);
                }
            }
        }
        return positions.Count > 0;
    }
    public Vector3Int GetCercanousPosition(Vector3Int lastPost)
    {
        return currentGrid.Keys.OrderBy(pos => Vector3Int.Distance(pos, lastPost)).First();
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

    public bool PathExists(Vector3 startPos, Vector3 endPos)
    {
        Vector3Int start = Vector3Int.FloorToInt(startPos);
        Vector3Int end = Vector3Int.FloorToInt(endPos);

        if (!grid.TryGetValue(start, out var startTile) || !startTile.isWalkable ||
            !grid.TryGetValue(end, out var endTile) || !endTile.isWalkable)
        {
            return false;
        }

        Node startNode = new Node(start.x, start.y, start.z);
        Node endNode = new Node(end.x, end.y, end.z);

        var queue = new Queue<Node>();
        var visited = new HashSet<Vector3Int>();

        queue.Enqueue(startNode);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Equals(endNode))
                return true;

            foreach (var neighbor in GetNeighbors(current))
            {
                Vector3Int neighborPos = new Vector3Int(neighbor.X, neighbor.Y, neighbor.Z);

                if (!grid.TryGetValue(neighborPos, out var neighborTile) || !neighborTile.isWalkable)
                    continue;

                if (visited.Contains(neighborPos))
                    continue;

                visited.Add(neighborPos);
                queue.Enqueue(neighbor);
            }
        }

        return false;
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
            if (GetHighestBlockAt(new Vector3Int(targetX, 0, targetZ), out var highestBlock))
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
    [Serializable]
    public class PoolingGridInfo
    {
        public Vector2Int pos;
        public GameObject grid;
    }
}