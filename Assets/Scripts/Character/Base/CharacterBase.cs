using System;
using System.Collections;
using System.Linq.Expressions;
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
    public GameObject floatingTextPrefab;
    public GameObject dieEffectPrefab;
    public bool canMoveAfterFinishTurn;
    public bool autoInit;
    public void OnEnable()
    {
        if (isInitialize) characterAnimations.MakeAnimation("Idle");
    }
    public virtual void Awake()
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
            await Awaitable.NextFrameAsync();
        }
    }
    [NaughtyAttributes.Button]
    public void UpdateLevelCharacter()
    {
        CharacterData.Statistic statistic = new CharacterData.Statistic
        {
            maxValue = characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue * 10
        };
        TakeExp(statistic);
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
        characterData.name = GameData.Instance.charactersDataDBSO.GenerateFantasyName();
        characterData.level = 1;
        characterData.id = initialDataSO.id;
        characterData.subId = initialDataSO.subId;
        gameObject.name = characterData.name;
        characterData.statistics = initialDataSO.CloneStatistics();
        characterData.skills = initialDataSO.CloneSkills();
        characterData.mastery = initialDataSO.CloneMastery();
        foreach (var statistic in characterData.statistics)
        {
            statistic.Value.RefreshValue();
            statistic.Value.SetMaxValue();
        }
        characterData.mastery = initialDataSO.CloneMastery();
        await Awaitable.NextFrameAsync();
    }
    public virtual void MoveCharacter(Vector3Int targetPosition) { }
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
    void ChangeDirectionModel()
    {
        characterModel.characterMeshRenderer.transform.localRotation = direction.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }
    public void TakeDamage(CharacterBase characterMakeDamage, int damage, string otherAnimation = "")
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
    public virtual IEnumerator Die(CharacterBase characterMakeDamage)
    {
        yield return new WaitForSeconds(0.3f);
        GameObject dieEffect = Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);
        characterModel.characterMeshRenderer.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        Destroy(dieEffect);
        GameManager.Instance.ChangeSceneSelector(GameManager.TypeScene.HomeScene);
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