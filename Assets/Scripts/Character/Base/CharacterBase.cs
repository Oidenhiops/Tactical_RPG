using System;
using System.Collections;
using UnityEngine;

public class CharacterBase : MonoBehaviour
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
    public bool hasLifted;
    public GameObject floatingTextPrefab;
    public GameObject dieEffectPrefab;
    public Action<CharacterBase> OnCharacterFinishMovement;
    public bool canMoveAfterFinishTurn;
    public bool autoInit;
    public void OnEnable()
    {
        if (isInitialize) characterAnimations.MakeAnimation("Idle");
        OnEnableHandle();
    }
    public void Awake()
    {
        if (autoInit) _ = InitializeCharacter();
    }
    void LateUpdate()
    {
        if (CameraInfo.Instance)
        {
            CameraInfo.Instance.CamDirection(nextDirection, out Vector3 directionFromCamera);
            direction = Vector3Int.RoundToInt(directionFromCamera);
            ChangeDirectionModel();
        }
    }
    [NaughtyAttributes.Button]
    public void ChangeSkin()
    {
        characterData.characterSkinId++;
        if (characterData.characterSkinId > GameData.Instance.charactersSkinDBSO.data[initialDataSO.id].Count - 1)
        {
            characterData.characterSkinId = 0;
        }
        characterData.characterSkinData = new CharacterData.CharacterSkinData
        {
            atlas = GameData.Instance.charactersSkinDBSO.data[initialDataSO.id][characterData.characterSkinId].atlas,
            atlasHands = GameData.Instance.charactersSkinDBSO.data[initialDataSO.id][characterData.characterSkinId].atlasHands
        };
        _ = InitializeAnimations();
    }
    public virtual void OnEnableHandle() { }
    public async Awaitable InitializeCharacter()
    {
        try
        {
            if (!isCharacterPlayer) await InitializeDataWhitInitialValues();
            await InitializeAnimations();
            isInitialize = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    async Awaitable InitializeAnimations()
    {
        try
        {
            characterAnimations.SetInitialData();
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    async Awaitable InitializeDataWhitInitialValues()
    {
        characterData.name = GameData.Instance.charactersDataDBSO.GenerateFantasyName();
        characterData.level = 1;
        characterData.characterId = initialDataSO.id;
        characterData.characterRangeId = initialDataSO.subId;
        gameObject.name = characterData.name;
        characterData.statistics = initialDataSO.CloneStatistics();
        characterData.skills = initialDataSO.CloneSkills();
        characterData.mastery = initialDataSO.CloneMastery();
        foreach (var statistic in characterData.statistics)
        {
            statistic.Value.RefreshValue();
            statistic.Value.SetMaxValue();
        }
        characterData.characterSkinData = new CharacterData.CharacterSkinData
        {
            atlas = GameData.Instance.charactersSkinDBSO.data[initialDataSO.id][characterData.characterSkinId].atlas,
            atlasHands = GameData.Instance.charactersSkinDBSO.data[initialDataSO.id][characterData.characterSkinId].atlasHands
        };
        characterData.mastery = initialDataSO.CloneMastery();
        await Awaitable.NextFrameAsync();
    }
    public virtual void MoveCharacter(Vector3Int targetPosition) { }
    protected IEnumerator WalkInStairs(GenerateMap.WalkablePositionInfo from, GenerateMap.WalkablePositionInfo to)
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
                    Vector3 moveDir = (to.blockInfo.transform.position - from.blockInfo.transform.position).normalized;
                    float dot = Vector3.Dot(moveDir, to.blockInfo.transform.forward);
                    if (Mathf.Abs(dot) > 0)
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = startPos.y;
                        }
                        else
                        {
                            midPoint.y = endPos.y;
                        }
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
                        midPoint.y = endPos.y;
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
                        midPoint.y = startPos.y;
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
    public virtual void TakeExp(CharacterData.Statistic statistic)
    {
        int amount = Mathf.CeilToInt(statistic.maxValue * 0.1f);
        characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue += amount;
        int level = 0;
        while (characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue >= characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue)
        {
            int spare = characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue > characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue ?
                Mathf.CeilToInt(characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue - characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue) : 0;
            characterData.statistics[CharacterData.TypeStatistic.Exp].baseValue = Mathf.CeilToInt(characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue * 2.2f);
            characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue = characterData.statistics[CharacterData.TypeStatistic.Exp].baseValue;
            characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue = spare;
            characterData.LevelUp();
            level++;
        }
    }
    public bool CanMakeActions()
    {
        return !hasLifted;
    }
    public void LookAt(Vector3Int startPos, Vector3Int finalPos)
    {
        if (startPos.x == finalPos.x)
        {
            nextDirection.x = startPos.z < finalPos.z ? 1 : -1;
        }
        else
        {
            nextDirection.x = startPos.x < finalPos.x ? -1 : 1;
        }
        if (startPos.z == finalPos.z)
        {
            nextDirection.z = startPos.x < finalPos.x ? 1 : -1;
        }
        else
        {
            nextDirection.z = startPos.z < finalPos.z ? 1 : -1;
        }
    }
    void ChangeDirectionModel()
    {
        characterModel.characterMeshRenderer.transform.localRotation = direction.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }
    public async Awaitable TakeDamage(CharacterBase characterMakeDamage, int damage, string otherAnimation = "")
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
        if (characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue <= 0) await Die(characterMakeDamage, otherAnimation);
        await Awaitable.NextFrameAsync();
    }
    public virtual async Awaitable Die(CharacterBase characterMakeDamage, string lastAnimation = "")
    {
        await Awaitable.WaitForSecondsAsync(0.3f);
        GameObject dieEffect = Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        characterModel.characterMeshRenderer.gameObject.SetActive(false);
        await Awaitable.WaitForSecondsAsync(1);
        Destroy(dieEffect);
        _ = GameManager.Instance.LoadScene(GameManager.TypeScene.HomeScene);
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