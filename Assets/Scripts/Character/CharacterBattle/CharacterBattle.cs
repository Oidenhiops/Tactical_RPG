using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterBattle : CharacterBase
{
    public override void Awake()
    {
        if (autoInit) _ = InitializeCharacter();
        BattlePlayerManager.Instance.actionsManager.OnEndTurn += OnEndTurn;
    }
    void OnDestroy()
    {
        BattlePlayerManager.Instance.actionsManager.OnEndTurn -= OnEndTurn;
    }
    void OnEndTurn()
    {
        if (lastAction == ActionsManager.TypeAction.EndTurn)
        {
            lastAction = ActionsManager.TypeAction.None;
        }
        canMoveAfterFinishTurn = false;
        startPositionInGrid = positionInGrid;
    }
    public override IEnumerator Die(CharacterBase characterMakeDamage, string lastAnimation = "")
    {
        yield return new WaitForSeconds(0.3f);
        GameObject dieEffect = Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        characterModel.characterMeshRenderer.gameObject.SetActive(false);
        if (lastAnimation == "Lift")
        {
            CharacterBase characterLifted = GetComponentsInChildren<CharacterBase>().FirstOrDefault(c => c != this);
            if (characterLifted != null)
            {
                if (characterLifted.characterAnimations.currentAnimation.name != "Lift")
                {
                    characterLifted.characterAnimations.MakeAnimation("Idle");
                }
                BattlePlayerManager.Instance.aStarPathFinding.grid[Vector3Int.RoundToInt(gameObject.transform.position)].hasCharacter = characterLifted;
                characterLifted.transform.localPosition = Vector3.zero;
                characterLifted.positionInGrid = positionInGrid;
                characterLifted.startPositionInGrid = startPositionInGrid;
                characterLifted.hasLifted = false;
                characterLifted.gameObject.transform.SetParent(BattlePlayerManager.Instance.charactersContainer);
            }
        }
        else
        {
            BattlePlayerManager.Instance.aStarPathFinding.grid[Vector3Int.RoundToInt(gameObject.transform.position)].hasCharacter = null;
        }
        yield return new WaitForSeconds(1);
        Destroy(dieEffect);
        gameObject.transform.position = Vector3.zero + Vector3.down;
        if (characterMakeDamage)
        {
            characterMakeDamage.TakeExp(characterData.statistics[CharacterData.TypeStatistic.Exp]);
        }
        if (isCharacterPlayer) BattlePlayerManager.Instance.characters.Remove(this);
        else BattleEnemyManager.Instance.characters.Remove(this);
        characterStatusEffect.statusEffects = new AYellowpaper.SerializedCollections.SerializedDictionary<StatusEffectBaseSO, int>();
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].dieCharacters.Add(characterData.name, characterData);
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters.Remove(characterData.name);
        Destroy(gameObject);
    }
    public override void MoveCharacter(Vector3Int targetPosition)
    {
        List<Vector3Int> path = isCharacterPlayer ? BattlePlayerManager.Instance.aStarPathFinding.FindPath(positionInGrid, targetPosition) : BattlePlayerManager.Instance.aStarPathFinding.FindPath(positionInGrid, targetPosition, BattlePlayerManager.Instance.aStarPathFinding.GetWalkableTiles(this));

        if (path != null && path.Count > 0)
        {
            BattlePlayerManager.Instance.aStarPathFinding.grid[path[0]].hasCharacter = null;
            BattlePlayerManager.Instance.aStarPathFinding.grid[targetPosition].hasCharacter = this;
            if (isCharacterPlayer) BattlePlayerManager.Instance.characterPlayerMakingActions = true;
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
        if (isCharacterPlayer)
        {
            BattlePlayerManager.Instance.characterPlayerMakingActions = false;
            if (positionInGrid == Vector3Int.zero)
            {
                gameObject.SetActive(false);
                BattlePlayerManager.Instance.aStarPathFinding.characterSelected = null;
                BattlePlayerManager.Instance.aStarPathFinding.grid[Vector3Int.zero].hasCharacter = null;
                if (BattlePlayerManager.Instance.actionsManager.characterActions.ContainsKey(this))
                {
                    BattlePlayerManager.Instance.actionsManager.characterActions.Remove(this);
                }
                BattlePlayerManager.Instance.menuCharacterSelector.amountCharacters++;
                startPositionInGrid = Vector3Int.zero;
                BattlePlayerManager.Instance.aStarPathFinding.DisableGrid();
            }
            else
            {
                if (BattlePlayerManager.Instance.actionsManager.characterActions.TryGetValue(this, out List<ActionsManager.ActionInfo> actions))
                {
                    actions.Add(new ActionsManager.ActionInfo
                    {
                        characterMakeAction = this,
                        typeAction = ActionsManager.TypeAction.Move,
                        positionInGrid = path[0]
                    });
                }
                else
                {
                    BattlePlayerManager.Instance.actionsManager.characterActions.Add(this, new List<ActionsManager.ActionInfo> { new ActionsManager.ActionInfo{
                        characterMakeAction = this,
                        typeAction = ActionsManager.TypeAction.Move,
                        positionInGrid = path[0]
                    } });
                }
                characterAnimations.MakeAnimation("Idle");
            }
        }
        else characterAnimations.MakeAnimation("Idle");
        OnCharacterFinishMovement?.Invoke(this);
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
    private IEnumerator WalkInStairs(GenerateMap.WalkablePositionInfo from, GenerateMap.WalkablePositionInfo to)
    {
        Vector3 startPos = new Vector3(from.pos.x, from.pos.y, from.pos.z);
        Vector3 endPos = new Vector3(to.pos.x, to.pos.y, to.pos.z);
        float duration = 0.2f;
        float elapsed = 0f;
        if (from.blockInfo.typeBlock == Block.TypeBlock.Stair && to.blockInfo.typeBlock == Block.TypeBlock.Stair)
        {
            startPos.y -= 0.3f;
            endPos.y -= 0.3f;
        }
        else if (to.blockInfo.typeBlock == Block.TypeBlock.Stair)
        {
            endPos.y -= 0.3f;
        }
        else
        {
            startPos.y -= 0.3f;
        }

        if (from.blockInfo.typeBlock != to.blockInfo.typeBlock)
        {
            Vector3 midPoint = (startPos + endPos) / 2f;
            if (from.pos.y == to.pos.y)
            {
                midPoint.y = Mathf.RoundToInt(midPoint.y);
            }
            else
            {
                if (from.blockInfo.transform.rotation.y == 0 && to.blockInfo.transform.rotation.y == 0)
                {
                    Vector3Int moveDir = Vector3Int.RoundToInt(to.blockInfo.transform.position - from.blockInfo.transform.position);
                    float dot = Vector3.Dot(moveDir, transform.forward);
                    if (dot > 0.5f)
                    {
                        midPoint.y = 0;
                    }
                    else
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = endPos.y;
                        }
                        else
                        {
                            midPoint.y = startPos.y;
                        }
                    }
                }
                else if (from.blockInfo.transform.rotation.y != 0 && to.blockInfo.transform.rotation.y == 0)
                {
                    Vector3 localToPos = from.blockInfo.transform.InverseTransformPoint(to.blockInfo.transform.position);
                    if (Mathf.Abs(localToPos.z) > Mathf.Abs(localToPos.x))
                    {
                        midPoint.y = 0;
                    }
                    else
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = endPos.y;
                        }
                        else
                        {
                            midPoint.y = startPos.y;
                        }
                    }
                }
                else if (from.blockInfo.transform.rotation.y == 0 && to.blockInfo.transform.rotation.y != 0)
                {
                    Vector3 moveDir = (to.blockInfo.transform.position - from.blockInfo.transform.position).normalized;
                    Vector3 forward = to.blockInfo.transform.forward; Vector3 right = to.blockInfo.transform.right;
                    float forwardDot = Vector3.Dot(moveDir, forward);
                    float rightDot = Vector3.Dot(moveDir, right);
                    if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
                    {
                        midPoint.y = 0;
                    }
                    else
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = endPos.y;
                        }
                        else
                        {
                            midPoint.y = startPos.y;
                        }
                    }
                }
            }

            float halfDuration = duration / 2f;
            while (elapsed < halfDuration)
            {
                float t = elapsed / halfDuration;
                transform.position = Vector3.Lerp(startPos, midPoint, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = midPoint;
            float elapsed2 = 0f;
            while (elapsed2 < halfDuration)
            {
                float t = elapsed2 / halfDuration;
                transform.position = Vector3.Lerp(midPoint, endPos, t);
                elapsed2 += Time.deltaTime;
                yield return null;
            }
            transform.position = endPos;
        }
        else
        {
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 pos = Vector3.Lerp(startPos, endPos, t);
                transform.position = pos;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
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
}
