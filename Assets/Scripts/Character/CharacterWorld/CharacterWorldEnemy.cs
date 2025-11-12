using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWorldEnemy : CharacterBase
{
    public Vector3 detectorOffset;
    public Vector3 detectorSize = Vector3.one;
    Collider[] colliders;
    List<Vector3Int> path = new List<Vector3Int>();
    public CharacterBase target;
    public bool isOnMovement;
    bool remakePath;
    Vector3Int _targetPos;
    Vector3Int targetPos
    {
        get => _targetPos;
        set
        {
            if (_targetPos != value)
            {
                _targetPos = value;
                remakePath = true;
            }
        }
    }
    void Update()
    {
        if (isInitialize && characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
        {
            if (!target) DetectTarget();
            else if (target)
            {
                targetPos = Vector3Int.RoundToInt(target.transform.position);
                if (!isOnMovement)
                {
                    if (targetPos != Vector3Int.RoundToInt(transform.position))
                    {
                        MoveCharacter(targetPos);
                    }
                }
            }
        }
    }
    bool DetectTarget()
    {
        colliders = Physics.OverlapBox(transform.position + detectorOffset, detectorSize / 2, Quaternion.identity, LayerMask.GetMask("CharacterPlayer"));

        foreach (var col in colliders)
        {
            if (col.transform != transform)
            {
                target = col.GetComponent<CharacterBase>();
                return true;
            }
        }
        target = null;
        return false;
    }
    public override async Awaitable Die(CharacterBase characterMakeDamage, string lastAnimation = "")
    {
        await Awaitable.WaitForSecondsAsync(0.3f);
        GameObject dieEffect = Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        characterModel.characterMeshRenderer.gameObject.SetActive(false);
        await Awaitable.WaitForSecondsAsync(1);
        Destroy(dieEffect);
        WorldManager.Instance.characterWorld.characterHitted = null;
        Destroy(gameObject);
        await Awaitable.NextFrameAsync();
    }
    public override void MoveCharacter(Vector3Int targetPosition)
    {
        path = WorldManager.Instance.aStarPathFinding.FindPath(Vector3Int.RoundToInt(transform.position), targetPosition);

        if (path != null && path.Count > 0)
        {
            isOnMovement = true;
            StartCoroutine(FollowPath(path));
        }
    }
    private IEnumerator FollowPath(List<Vector3Int> path)
    {
        positionInGrid = path[path.Count - 1];
        nextDirection = Vector3Int.zero;
        characterAnimations.MakeAnimation("Walk");
        for (int i = 1; i < path.Count; i++)
        {
            if (!remakePath)
            {
                LookAt(path[i - 1], path[i]);
                if (WorldManager.Instance.aStarPathFinding.grid[path[i - 1]].blockInfo.typeBlock == Block.TypeBlock.Stair || WorldManager.Instance.aStarPathFinding.grid[path[i]].blockInfo.typeBlock == Block.TypeBlock.Stair)
                {
                    yield return StartCoroutine(WalkInStairs(WorldManager.Instance.aStarPathFinding.grid[path[i - 1]], WorldManager.Instance.aStarPathFinding.grid[path[i]]));
                }
                else
                {
                    if (path[i - 1].y != path[i].y)
                    {
                        yield return StartCoroutine(JumpToPosition(path[i - 1], path[i], 0.5f));
                    }
                    else
                    {
                        yield return StartCoroutine(MoveToPosition(path[i]));
                    }
                }
            }
            else
            {                
                break;
            }
        }
        if (!remakePath)
        {
            characterAnimations.MakeAnimation("Idle");
        }
        isOnMovement = false;
        remakePath = false;
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
        if (startPos.y > endPos.y)
        {
            Vector3 midPos = new Vector3(endPos.x, startPos.y, endPos.z);
            yield return MakeParabola(startPos, midPos, duration / 2);
            yield return GoToHightPoint(midPos, endPos, duration / 4);
        }
        else
        {
            Vector3 midPos = new Vector3(startPos.x, endPos.y, startPos.z);
            yield return GoToHightPoint(startPos, endPos, duration / 4);
            yield return MakeParabola(midPos, endPos, duration / 2);
        }
        transform.position = endPos;
    }
    public IEnumerator GoToHightPoint(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            Vector3 pos = Vector3.Lerp(startPos,
                new Vector3(startPos.x, endPos.y, startPos.z), t);
            transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator MakeParabola(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
            float parabola = 4 * 1f * t * (1 - t);
            horizontal.y += parabola;
            transform.position = horizontal;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.rebeccaPurple;
        Gizmos.DrawWireCube(transform.position + detectorOffset, detectorSize);
        if (path != null && path.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 from = new Vector3(path[i].x, path[i].y, path[i].z);
                Vector3 to = new Vector3(path[i + 1].x, path[i + 1].y, path[i + 1].z);
                Gizmos.DrawLine(from, to);
                Gizmos.DrawSphere(from, 0.1f);
            }
            Gizmos.DrawSphere(new Vector3(path[path.Count - 1].x, path[path.Count - 1].y, path[path.Count - 1].z), 0.1f);
        }
    }
}