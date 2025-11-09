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
    void OnDisable()
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
    public override async Awaitable Die(CharacterBase characterMakeDamage, string lastAnimation = "")
    {
        await Awaitable.WaitForSecondsAsync(0.3f);
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
        await Awaitable.WaitForSecondsAsync(1);
        Destroy(dieEffect);
        gameObject.transform.position = Vector3.zero + Vector3.down;
        if (characterMakeDamage)
        {
            characterMakeDamage.TakeExp(characterData.statistics[CharacterData.TypeStatistic.Exp]);
        }
        if (isCharacterPlayer) await BattlePlayerManager.Instance.OnCharacterDie(this);
        else await BattleEnemyManager.Instance.OnCharacterDie(this);
        characterStatusEffect.statusEffects = new AYellowpaper.SerializedCollections.SerializedDictionary<StatusEffectBaseSO, int>();
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].dieCharacters.Add(characterData.name, characterData);
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters.Remove(characterData.name);
        Destroy(gameObject);
        await Awaitable.NextFrameAsync();
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
                if (BattlePlayerManager.Instance.aStarPathFinding.grid[path[i - 1]].blockInfo.typeBlock == Block.TypeBlock.Stair || BattlePlayerManager.Instance.aStarPathFinding.grid[path[i]].blockInfo.typeBlock == Block.TypeBlock.Stair)
                {
                    yield return StartCoroutine(WalkInStairs(BattlePlayerManager.Instance.aStarPathFinding.grid[path[i - 1]], BattlePlayerManager.Instance.aStarPathFinding.grid[path[i]]));
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
            if (BattlePlayerManager.Instance.aStarPathFinding.grid[positionInGrid].blockInfo.typeBlock == Block.TypeBlock.Spawn)
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
            else if (BattlePlayerManager.Instance.aStarPathFinding.grid[positionInGrid].blockInfo.typeBlock == Block.TypeBlock.End)
            {
                _ = BattlePlayerManager.Instance.PlayersWin();
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
