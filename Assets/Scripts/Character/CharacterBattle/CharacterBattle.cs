using System.Collections;
using System.Collections.Generic;
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
        startPositionInGrid = positionInGrid;
    }
    public override IEnumerator Die(CharacterBase characterMakeDamage)
    {
        yield return new WaitForSeconds(0.3f);
        GameObject dieEffect = Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        characterModel.characterMeshRenderer.gameObject.SetActive(false);
        if (characterAnimations.currentAnimation.name == "Lift" || characterAnimations.animationAfterEnd == "Lift")
        {
            if (transform.GetChild(1).gameObject.TryGetComponent(out CharacterAnimation component))
            {
                if (component.currentAnimation.name != "Lift")
                {
                    component.MakeAnimation("Idle");
                }
                BattlePlayerManager.Instance.aStarPathFinding.grid[Vector3Int.RoundToInt(gameObject.transform.position)].hasCharacter = component.character;
                component.transform.position = transform.position;
                component.character.positionInGrid = positionInGrid;
                component.character.startPositionInGrid = startPositionInGrid;
            }
            transform.GetChild(1).gameObject.transform.SetParent(transform.parent);
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
        characterStatusEffect.statusEffects = new AYellowpaper.SerializedCollections.SerializedDictionary<StatusEffectBaseSO, int>();
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].dieCharacters.Add(characterData.name, characterData);
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters.Remove(characterData.name);
    }
    public override void MoveCharacter(Vector3Int targetPosition)
    {
        List<Vector3Int> path = BattlePlayerManager.Instance.aStarPathFinding.FindPath(positionInGrid, targetPosition);

        if (path != null && path.Count > 0)
        {
            BattlePlayerManager.Instance.aStarPathFinding.grid[path[0]].hasCharacter = null;
            if (isCharacterPlayer) BattlePlayerManager.Instance.characterPlayerMakingActions = true;
            BattlePlayerManager.Instance.aStarPathFinding.grid[targetPosition].hasCharacter = this;
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
            if (path[i - 1].x == path[i].x)
            {
                nextDirection.x = path[i - 1].z < path[i].z ? 1 : -1;
            }
            else
            {
                nextDirection.x = path[i - 1].x < path[i].x ? -1 : 1;
            }
            if (path[i - 1].z == path[i].z)
            {
                nextDirection.z = path[i - 1].x < path[i].x ? 1 : -1;
            }
            else
            {
                nextDirection.z = path[i - 1].z < path[i].z ? 1 : -1;
            }
            if (path[i - 1].y != path[i].y)
            {
                yield return StartCoroutine(JumpToPosition(path[i - 1], path[i], 0.5f));
            }
            else
            {
                yield return StartCoroutine(MoveToPosition(path[i]));
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
