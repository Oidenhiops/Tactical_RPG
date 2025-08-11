using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool isCharacterPlayer;
    public bool isInitialize;
    public CharacterModel characterModel;
    public SerializedDictionary<TypeStatistic, Statistic> statistics = new SerializedDictionary<TypeStatistic, Statistic>
    {
        {TypeStatistic.Hp, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Sp, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Atk, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Int, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Hit, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Def, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Res, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Spd, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
        {TypeStatistic.Exp, new Statistic{baseValue = 0, buffValue = 0, itemValue = 0, currentValue = 0, maxValue = 0}},
    };
    public SerializedDictionary<string, CharacterItems> items = new SerializedDictionary<string, CharacterItems>();
    public Vector3Int _direction;
    public Action<Vector3Int> OnDirectionChange;
    public Vector3Int direction
    {
        get => _direction;
        set
        {
            if (_direction != value)
            {
                _direction = value;
                OnDirectionChange?.Invoke(_direction);
            }
        }
    }
    public CharacterAnimation characterAnimations;
    public Vector3Int currentPositionInGrid;
    public Vector3Int initialPositionInGrid;
    public void Start()
    {
        _ = InitializeCharacter();
    }
    public async Awaitable InitializeCharacter()
    {
        try
        {
            await InitializeAnimations();
            isInitialize = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    async Awaitable InitializeAnimations()
    {
        try
        {
            characterAnimations.SetInitialData(ref characterModel.characterAnimations);
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    public int GetMaxHeightToUp()
    {
        return Mathf.RoundToInt(transform.position.y) + 5;
    }
    public void MoveCharacter(Vector3Int targetPosition)
    {
        List<Vector3Int> path = AStarPathFinding.Instance.FindPath(currentPositionInGrid, targetPosition);

        if (path != null && path.Count > 0)
        {
            AStarPathFinding.Instance.grid[currentPositionInGrid].hasCharacter = null;
            AStarPathFinding.Instance.grid[targetPosition].hasCharacter = this;
            StartCoroutine(FollowPath(path));
        }
    }
    private IEnumerator FollowPath(List<Vector3Int> path)
    {
        Vector3Int newDirection = Vector3Int.zero;
        characterAnimations.MakeAnimation("Walk");
        for (int i = 1; i < path.Count; i++)
        {
            if (path[i - 1].x == path[i].x)
            {
                newDirection.x = path[i-1].z < path[i].z ? 1 : -1;
            }
            else
            {
                newDirection.x = path[i-1].x < path[i].x ? -1 : 1;
            }
            if (path[i - 1].z == path[i].z)
            {
                newDirection.z = path[i-1].x < path[i].x ? 1 : -1;
            }
            else
            {
                newDirection.z = path[i-1].z < path[i].z ? 1 : -1;
            }

            CameraInfo.Instance.CamDirection(out Vector3 camForward, out Vector3 camRight);
            Vector3 camRelativeDir = (newDirection.x * camRight + newDirection.z * camForward).normalized;
            Vector3 movementDirection = new Vector3(camRelativeDir.x, 0, camRelativeDir.z).normalized;

            direction = new Vector3Int(Mathf.RoundToInt(movementDirection.x), Mathf.RoundToInt(movementDirection.y), Mathf.RoundToInt(movementDirection.z));
            ChangeDirectionModel();
            if (path[i - 1].y != path[i].y)
            {
                yield return StartCoroutine(JumpToPosition(path[i - 1], path[i], 1f));
            }
            else
            {
                yield return StartCoroutine(MoveToPosition(path[i]));
            }
            currentPositionInGrid = path[i];
        }
        characterAnimations.MakeAnimation("Idle");
    }
    void ChangeDirectionModel()
    {
        characterModel.characterMeshRenderer.transform.localRotation = direction.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }
    private IEnumerator MoveToPosition(Vector3Int targetPos)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, targetPos.y, targetPos.z);
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
private IEnumerator JumpToPosition(Vector3Int from, Vector3Int to, float duration)
{
    Vector3 startPos = new Vector3(from.x, from.y, from.z);
    Vector3 endPos = new Vector3(to.x, to.y, to.z);
    float elapsed = 0f;
    float height = Mathf.Abs(to.y - from.y) * 0.5f + 0.5f;
    while (elapsed < duration)
    {
        float t = elapsed / duration;
        Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
        float baseY = Mathf.Lerp(startPos.y, endPos.y, t);
        float parabola = 4 * height * t * (1 - t);
        horizontal.y = baseY + parabola;
        transform.position = horizontal;
        elapsed += Time.deltaTime;
        yield return null;
    }
    transform.position = endPos;
}

    [Serializable] public class Statistic
    {
        public int baseValue = 0;
        public int itemValue = 0;
        public int buffValue = 0;
        public int maxValue = 0;
        public int currentValue = 0;
    }
    [Serializable] public class CharacterModel
    {
        public string characterAnimationsId;
        public CharacterAnimationsSO characterAnimations;
        public MeshRenderer characterMeshRenderer;
        public MeshRenderer characterMeshRendererHand;
        public Transform leftHand;
        public Transform rightHand;
        public Mesh originalMesh;
    }
    [Serializable] public class CharacterItems
    {
        public string item;
    }
    public enum TypeStatistic
    {
        None = 0,
        Hp = 1,
        Sp = 2,
        Atk = 3,
        Hit = 4,
        Int = 5,
        Def = 6,
        Res = 7,
        Spd = 8,
        Exp = 9,
    }
}