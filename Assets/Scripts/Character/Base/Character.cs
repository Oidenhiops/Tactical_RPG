using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool isInitialize;
    public bool isCharacterPlayer;
    public TypeCharacter typeCharacter;
    public InitialDataSO initialDataSO;
    public CharacterModel characterModel;
    public CharacterData characterData;
    public Vector3Int direction = new Vector3Int();
    public Vector3Int nextDirection;
    public CharacterAnimation characterAnimations;
    public CharacterStatusEffect characterStatusEffect;
    public Vector3Int positionInGrid;
    public Vector3Int startPositionInGrid;
    public ActionsManager.TypeAction lastAction;
    public GameObject floatingTextPrefab;
    public GameObject dieEffectPrefab;
    public bool setDataWhitInitialValues;
    public bool autoInit;
    public void OnEnable()
    {
        if (isInitialize) characterAnimations.MakeAnimation("Idle");
    }
    public void Awake()
    {
        if (autoInit) _ = InitializeCharacter();
        if (PlayerManager.Instance) PlayerManager.Instance.actionsManager.OnEndTurn += OnEndTurn;
    }
    void OnDestroy()
    {
        if (PlayerManager.Instance) PlayerManager.Instance.actionsManager.OnEndTurn -= OnEndTurn;
    }
    void OnEndTurn()
    {
        if (lastAction == ActionsManager.TypeAction.EndTurn)
        {
            lastAction = ActionsManager.TypeAction.None;
        }
        startPositionInGrid = positionInGrid;
    }
    void LateUpdate()
    {
        if (CameraInfo.Instance)
        {
            CameraInfo.Instance.CamDirection(out Vector3 camForward, out Vector3 camRight);
            Vector3 camRelativeDir = (nextDirection.x * camRight + nextDirection.z * camForward).normalized;
            Vector3 movementDirection = new Vector3(camRelativeDir.x, 0, camRelativeDir.z).normalized;
            direction = new Vector3Int(Mathf.RoundToInt(movementDirection.x), Mathf.RoundToInt(movementDirection.y), Mathf.RoundToInt(movementDirection.z));
            ChangeDirectionModel();
        }
    }
    public async Awaitable InitializeCharacter()
    {
        try
        {
            if (setDataWhitInitialValues) await InitializeDataWhitInitialValues();
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
            characterAnimations.SetInitialData(ref initialDataSO);
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    async Awaitable InitializeDataWhitInitialValues()
    {
        characterData.statistics = initialDataSO.CloneStatistics();
        foreach (var statistic in characterData.statistics)
        {
            statistic.Value.RefreshValue();
            statistic.Value.SetMaxValue();
        }
        await Awaitable.NextFrameAsync();
    }
    public void MoveCharacter(Vector3Int targetPosition)
    {
        List<Vector3Int> path = AStarPathFinding.Instance.FindPath(positionInGrid, targetPosition);

        if (path != null && path.Count > 0)
        {
            AStarPathFinding.Instance.grid[path[0]].hasCharacter = null;
            if (isCharacterPlayer) PlayerManager.Instance.characterPlayerMakingActions = true;
            AStarPathFinding.Instance.grid[targetPosition].hasCharacter = this;
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
            PlayerManager.Instance.characterPlayerMakingActions = false;
            if (positionInGrid == Vector3Int.zero)
            {
                gameObject.SetActive(false);
                AStarPathFinding.Instance.characterSelected = null;
                AStarPathFinding.Instance.grid[Vector3Int.zero].hasCharacter = null;
                if (PlayerManager.Instance.actionsManager.characterActions.ContainsKey(this))
                {
                    PlayerManager.Instance.actionsManager.characterActions.Remove(this);
                }
                PlayerManager.Instance.menuCharacterSelector.amountCharacters++;
                startPositionInGrid = Vector3Int.zero;
                AStarPathFinding.Instance.DisableGrid();
            }
            else
            {
                if (PlayerManager.Instance.actionsManager.characterActions.TryGetValue(this, out List<ActionsManager.ActionInfo> actions))
                {
                    actions.Add(new ActionsManager.ActionInfo
                    {
                        character = this,
                        typeAction = ActionsManager.TypeAction.Move,
                        positionInGrid = path[0]
                    });
                }
                else
                {
                    PlayerManager.Instance.actionsManager.characterActions.Add(this, new List<ActionsManager.ActionInfo> { new ActionsManager.ActionInfo{
                        character = this,
                        typeAction = ActionsManager.TypeAction.Move,
                        positionInGrid = path[0]
                    } });
                }
                characterAnimations.MakeAnimation("Idle");
            }
        }
        else characterAnimations.MakeAnimation("Idle");
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
    public void TakeDamage(Character characterMakeDamage, int damage, string otherAnimation = "")
    {
        characterAnimations.MakeAnimation("TakeDamage");
        characterAnimations.animationAfterEnd = otherAnimation;
        FloatingText floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity).GetComponent<FloatingText>();
        _ = floatingText.SendText(damage.ToString(), Color.red, false);
        if (characterData.statistics.TryGetValue(CharacterData.TypeStatistic.Hp, out CharacterData.Statistic characterTakedDamageStatistic))
        {
            characterTakedDamageStatistic.currentValue -= damage;
        }
        characterAnimations.MakeEffect(CharacterAnimation.TypeAnimationsEffects.Shake);
        characterAnimations.MakeEffect(CharacterAnimation.TypeAnimationsEffects.Blink);
        if (characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue <= 0) StartCoroutine(Die(characterMakeDamage));
    }
    [NaughtyAttributes.Button]
    public void RefreshStatistics()
    {
        foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in characterData.statistics)
        {
            statistic.Value.RefreshValue();
        }
    }
    public IEnumerator Die(Character characterMakeDamage)
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
                AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(gameObject.transform.position)].hasCharacter = component.character;
                component.transform.position = transform.position;
                component.character.positionInGrid = positionInGrid;
                component.character.startPositionInGrid = startPositionInGrid;
            }
            transform.GetChild(1).gameObject.transform.SetParent(transform.parent);
        }
        else
        {
            AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(gameObject.transform.position)].hasCharacter = null;
        }
        yield return new WaitForSeconds(1);
        Destroy(dieEffect);
        gameObject.transform.position = Vector3.zero + Vector3.down;
        if (characterMakeDamage)
        {
            characterMakeDamage.TakeExp(this);
        }
        characterStatusEffect.statusEffects = new AYellowpaper.SerializedCollections.SerializedDictionary<StatusEffectBaseSO, int>();
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].dieCharacters.Add(characterData.name, characterData);
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters.Remove(characterData.name);
    }
    public void TakeExp(Character characterDie)
    {
        int amount = Mathf.CeilToInt(characterDie.characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue * 0.1f);
        characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue += amount;
        while (characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue > characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue)
        {
            int spare = Mathf.CeilToInt(characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue - characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue);
            characterData.statistics[CharacterData.TypeStatistic.Exp].baseValue = Mathf.CeilToInt(characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue * 2.2f);
            characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue = characterData.statistics[CharacterData.TypeStatistic.Exp].baseValue;
            characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue = spare;
        }
    }
    public async Task MakeSpecial()
    {
        await Awaitable.NextFrameAsync();
    }
    [Serializable]
    public class CharacterModel
    {
        public MeshRenderer characterMeshRenderer;
        public MeshRenderer characterMeshRendererHand;
        public Transform leftHand;
        public Transform rightHand;
        public Mesh originalMesh;
    }
    public enum TypeCharacter
    {
        None = 0,
        Character = 1,
        GeoSymbol = 2,
        Chest = 3
    }
}