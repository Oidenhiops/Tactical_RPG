using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class BattleEnemyManager : MonoBehaviour
{
    public static BattleEnemyManager Instance;
    public ActionsManager actionsManager;
    public GenerateMap generateMap;
    public BattlePlayerManager battlePlayerManager;
    public AStarPathFinding aStarPathFinding;
    public List<CharacterBase> characters;
    public Dictionary<CharacterBase, AiAction> bestActions = new Dictionary<CharacterBase, AiAction>();
    public GameObject characterBattlePrefab;
    public Transform charactersContainer;
    public List<InitialDataSO> initialDataSelected;
    public CharacterBase principalCharacter;
    public Material materialCharacterEnemy;
    public CharacterBase characterForTest;
    public Vector2Int amountCharacters;
    public bool manualCreateCharacters;
    public int _charactersMoving;
    public int charactersMoving
    {
        get => _charactersMoving;
        set
        {
            if (_charactersMoving != value)
            {
                _charactersMoving = value;
                if (_charactersMoving == 0)
                {
                    MakeActionsActerCharactersMoveFinish();
                }
            }
        }
    }
    public Dictionary<CharacterBase, Vector3Int> occupiedPositions = new Dictionary<CharacterBase, Vector3Int>();
    public int targetLevel;
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Start()
    {
        if (ManagementBattleInfo.Instance) principalCharacter = ManagementBattleInfo.Instance.principalCharacterEnemy;
        generateMap.OnFinishGenerateMap += InitializeCharacters;
        actionsManager.OnEndTurn += CreateStrategy;
    }
    void OnDestroy()
    {
        generateMap.OnFinishGenerateMap -= InitializeCharacters;
        actionsManager.OnEndTurn -= CreateStrategy;
    }
    [NaughtyAttributes.Button]
    public void CreateStrategy()
    {
        if (!actionsManager.isPlayerTurn)
        {
            bestActions.Clear();
            occupiedPositions.Clear();
            foreach (var character in characters)
            {
                GetCharacterActions(character, ref bestActions);
            }
            MoveCharactersForMakeActions(bestActions);
        }
    }
    public void DiscountCharacterMoving(CharacterBase characterFinishMove)
    {
        charactersMoving--;
        characterFinishMove.OnCharacterFinishMovement -= DiscountCharacterMoving;
        characterFinishMove.LookAt(characterFinishMove.positionInGrid, bestActions[characterFinishMove].posibleTargets.First().positionInGrid);
    }
    public void GetCharacterActions(CharacterBase characterForValidate, ref Dictionary<CharacterBase, AiAction> actions)
    {
        List<CharacterBase> posibleTargets = new List<CharacterBase>();
        List<AiAction> posibleActions = new List<AiAction>();
        Dictionary<PosibleActions, AiAction> bestActions = new Dictionary<PosibleActions, AiAction>();
        GetPosibleCharactersForMakeAction(ref posibleTargets, characterForValidate);
        GetPosibleActionsForMakeAction(ref posibleActions, posibleTargets, characterForValidate);
        GetBestActions(ref bestActions, posibleActions, characterForValidate);
        actions.Add(characterForValidate, bestActions.Values.First());
    }
    public void GetPosibleCharactersForMakeAction(ref List<CharacterBase> posibleTargets, CharacterBase characterForValidate)
    {
        Vector2 posCharacter;
        Vector2 posTarget;
        foreach (var posibleTarget in characters)
        {
            posCharacter.x = characterForValidate.transform.position.x;
            posCharacter.y = characterForValidate.transform.position.z;
            posTarget.x = posibleTarget.transform.position.x;
            posTarget.y = posibleTarget.transform.position.z;

            if (posibleTarget.gameObject.activeSelf && Vector2.Distance(posCharacter, posTarget) <= characterForValidate.characterData.GetMovementRadius() * 2)
            {
                posibleTargets.Add(posibleTarget);
            }
        }

        foreach (var posibleTarget in battlePlayerManager.characters)
        {
            posCharacter.x = characterForValidate.transform.position.x;
            posCharacter.y = characterForValidate.transform.position.z;
            posTarget.x = posibleTarget.transform.position.x;
            posTarget.y = posibleTarget.transform.position.z;

            if (posibleTarget.gameObject.activeSelf && Vector2.Distance(posCharacter, posTarget) <= characterForValidate.characterData.GetMovementRadius() * 2)
            {
                posibleTargets.Add(posibleTarget);
            }
        }

        posibleTargets.Remove(characterForValidate);
        print("finish");
    }
    public void GetPosibleActionsForMakeAction(ref List<AiAction> posibleActions, List<CharacterBase> posibleTargets, CharacterBase characterForValidate)
    {
        foreach (var posibleTarget in posibleTargets)
        {
            if (posibleTarget.isCharacterPlayer)
            {
                //Enemy
                #region Basic Attack Action

                aStarPathFinding.GetTilesForMakeAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> attackPositions, characterForValidate, posibleTarget);
                if (attackPositions.Count > 0)
                {
                    AiAction aiAction = new AiAction
                    {
                        characterMakeAction = characterForValidate,
                        typeAction = PosibleActions.BasicAttack,
                        posiblePositions = attackPositions,
                        posibleTargets = new List<CharacterBase> { posibleTarget }
                    };
                    posibleActions.Add(aiAction);
                }

                #endregion

                #region Skill Attack Action

                if (characterForValidate.characterData.skills.Count > 0)
                {
                    if (characterForValidate.characterData.skills.ContainsKey(ItemBaseSO.TypeWeapon.None) &&
                        characterForValidate.characterData.skills[ItemBaseSO.TypeWeapon.None].ContainsKey(SkillsBaseSO.TypeSkill.Attack))
                    {
                        foreach (var skill in characterForValidate.characterData.skills[ItemBaseSO.TypeWeapon.None][SkillsBaseSO.TypeSkill.Attack])
                        {
                            if (skill.Value.statistics[CharacterData.TypeStatistic.Sp].baseValue <= characterForValidate.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue)
                            {
                                aStarPathFinding.GetTilesForMakeSkill(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> skillPositions,
                                    characterForValidate, posibleTarget, skill.Value);
                                if (skillPositions.Count > 0 && aStarPathFinding.PathExists(skillPositions.ElementAt(UnityEngine.Random.Range(0, skillPositions.Count)).Value.pos, characterForValidate.positionInGrid, aStarPathFinding.GetWalkableTiles(characterForValidate)))
                                {
                                    AiAction aiAction = new AiAction
                                    {
                                        characterMakeAction = characterForValidate,
                                        typeAction = PosibleActions.SkillAttack,
                                        skill = skill.Value,
                                        posiblePositions = skillPositions
                                    };
                                    posibleActions.Add(aiAction);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Skill Debuff Action

                if (characterForValidate.characterData.skills.Count > 0)
                {
                    if (characterForValidate.characterData.skills.ContainsKey(ItemBaseSO.TypeWeapon.None) &&
                        characterForValidate.characterData.skills[ItemBaseSO.TypeWeapon.None].ContainsKey(SkillsBaseSO.TypeSkill.Debuff))
                    {
                        foreach (var skill in characterForValidate.characterData.skills[ItemBaseSO.TypeWeapon.None][SkillsBaseSO.TypeSkill.Debuff])
                        {
                            if (skill.Value.statistics[CharacterData.TypeStatistic.Sp].baseValue <= characterForValidate.characterData.statistics[CharacterData.TypeStatistic.Sp].currentValue)
                            {
                                aStarPathFinding.GetTilesForMakeSkill(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> skillPositions,
                                    characterForValidate, posibleTarget, skill.Value);
                                if (skillPositions.Count > 0)
                                {
                                    AiAction aiAction = new AiAction
                                    {
                                        characterMakeAction = characterForValidate,
                                        typeAction = PosibleActions.SkillAttack,
                                        skill = skill.Value,
                                        posiblePositions = skillPositions
                                    };
                                    posibleActions.Add(aiAction);
                                }
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                //Ally
            }
        }
    }
    public void GetBestActions(ref Dictionary<PosibleActions, AiAction> bestActions, List<AiAction> posibleActions, CharacterBase characterForValidate)
    {
        Dictionary<PosibleActions, List<AiAction>> groupedActions = new Dictionary<PosibleActions, List<AiAction>>();
        foreach (var action in posibleActions)
        {
            if (!groupedActions.ContainsKey(action.typeAction))
            {
                groupedActions[action.typeAction] = new List<AiAction>();
            }
            groupedActions[action.typeAction].Add(action);
        }
        foreach (var group in groupedActions)
        {
            switch (group.Key)
            {
                case PosibleActions.BasicAttack:
                    foreach (var action in group.Value)
                    {
                        if (!bestActions.ContainsKey(PosibleActions.BasicAttack))
                        {
                            if (GetNonOccupiedPositionFromAction(action, out Vector3Int availablePos))
                            {
                                action.posiblePositions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>
                                {
                                    { availablePos, action.posiblePositions[availablePos] }
                                };
                                bestActions.Add(PosibleActions.BasicAttack, action);
                                occupiedPositions.Add(action.characterMakeAction, availablePos);
                            }
                        }
                        else
                        {
                            if (bestActions[PosibleActions.BasicAttack].posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue -
                                action.characterMakeAction.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
                            {
                                if (action.posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue -
                                action.characterMakeAction.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
                                {
                                    if (action.posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Atk].currentValue >
                                        bestActions[PosibleActions.BasicAttack].posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Atk].currentValue)
                                    {
                                        if (GetNonOccupiedPositionFromAction(action, out Vector3Int availablePos))
                                        {
                                            action.posiblePositions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>
                                            {
                                                { availablePos, action.posiblePositions[availablePos] }
                                            };
                                            bestActions.Add(PosibleActions.BasicAttack, action);
                                            occupiedPositions[action.characterMakeAction] = availablePos;
                                        }
                                    }
                                }
                                else
                                {
                                    if (GetNonOccupiedPositionFromAction(action, out Vector3Int availablePos))
                                    {
                                        action.posiblePositions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>
                                        {
                                            { availablePos, action.posiblePositions[availablePos] }
                                        };
                                        bestActions.Add(PosibleActions.BasicAttack, action);
                                        occupiedPositions[action.characterMakeAction] = availablePos;
                                    }
                                }
                            }
                            else if (action.posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue -
                                action.characterMakeAction.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue <= 0 &&
                                action.posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Atk].currentValue >
                                bestActions[PosibleActions.BasicAttack].posibleTargets[0].characterData.statistics[CharacterData.TypeStatistic.Atk].currentValue)
                            {
                                if (GetNonOccupiedPositionFromAction(action, out Vector3Int availablePos))
                                {
                                    action.posiblePositions = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>
                                    {
                                        { availablePos, action.posiblePositions[availablePos] }
                                    };
                                    bestActions.Add(PosibleActions.BasicAttack, action);
                                    occupiedPositions[action.characterMakeAction] = availablePos;
                                }
                            }
                        }
                    }
                    break;
                case PosibleActions.SkillAttack:
                    foreach (var action in group.Value)
                    {
                        if (!bestActions.ContainsKey(PosibleActions.SkillAttack))
                        {
                            bestActions.Add(PosibleActions.SkillAttack, action);
                        }
                        else
                        {

                        }
                    }
                    break;
            }
        }
    }
    void MoveCharactersForMakeActions(Dictionary<CharacterBase, AiAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.Key.positionInGrid != action.Value.posiblePositions.Keys.First())
            {
                charactersMoving++;
                action.Key.OnCharacterFinishMovement += DiscountCharacterMoving;
                action.Key.MoveCharacter(action.Value.posiblePositions.Keys.First());
            }
        }
    }
    void MakeActionsActerCharactersMoveFinish()
    {
        foreach (var action in bestActions)
        {
            switch (action.Value.typeAction)
            {
                case PosibleActions.BasicAttack:
                    actionsManager.characterActions.Add(
                        action.Key,
                        new List<ActionsManager.ActionInfo> 
                        {
                            new ActionsManager.ActionInfo
                            {
                                characterMakeAction = action.Key,
                                typeAction = ActionsManager.TypeAction.Attack,
                                characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                                {
                                    new ActionsManager.OtherCharacterInfo(action.Value.posibleTargets.First(), Vector3Int.RoundToInt(action.Value.posibleTargets.First().transform.position))
                                }
                            }
                        }
                    );
                    actionsManager.characterFinalActions.Add(action.Key, new ActionsManager.ActionInfo
                    {
                        characterMakeAction = action.Key,
                        typeAction = ActionsManager.TypeAction.Attack,
                        characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                        {
                            new ActionsManager.OtherCharacterInfo(action.Value.posibleTargets.First(), Vector3Int.RoundToInt(action.Value.posibleTargets.First().transform.position))
                        }
                    });
                    break;
                case PosibleActions.SkillAttack:
                    actionsManager.characterActions.Add(
                        action.Key,
                        new List<ActionsManager.ActionInfo>
                        {
                            new ActionsManager.ActionInfo
                            {
                                characterMakeAction = action.Key,
                                typeAction = ActionsManager.TypeAction.Skill,
                                skillInfo = action.Value.skill,
                                positionsToMakeSkill = action.Value.posiblePositions.Keys.ToList()
                            }
                        }
                    );
                    actionsManager.characterFinalActions.Add(action.Key, new ActionsManager.ActionInfo
                    {
                        characterMakeAction = action.Key,
                        typeAction = ActionsManager.TypeAction.Skill,
                        skillInfo = action.Value.skill,
                        positionsToMakeSkill = action.Value.posiblePositions.Keys.ToList()
                    });
                    break;
            }
        }
        _ = actionsManager.EndTurn();
    }
    public bool GetNonOccupiedPositionFromAction(AiAction action, out Vector3Int availablePos)
    {
        availablePos = occupiedPositions.Count > 0 ? action.posiblePositions.Keys.FirstOrDefault(pos => !occupiedPositions.ContainsValue(pos)) : action.posiblePositions.Keys.FirstOrDefault();
        bool positionAvailable = availablePos != default;

        return positionAvailable;
    }
    #region Character Initialization
    public void InitializeCharacters()
    {
        _ = GetCharactersInitialData();
    }
    public async Awaitable GetCharactersInitialData()
    {
        try
        {
            if (amountCharacters.y != 0)
            {
                if (!manualCreateCharacters)
                {
                    initialDataSelected.Add(principalCharacter.initialDataSO);
                    if (amountCharacters.y > initialDataSelected.Count)
                    {
                        int range = UnityEngine.Random.Range(amountCharacters.x, amountCharacters.y + 1);
                        for (int i = 0; i < range - 1; i++)
                        {
                            initialDataSelected.Add(GameData.Instance.charactersDataDBSO.GetRandomInitialDataSO());
                        }
                    }
                }
                await CreateCharacters();
                await LevelUpCharacters(principalCharacter ? principalCharacter.characterData.level : targetLevel);
                await SpawnCharactersInBattle();
            }
            else
            {
                Debug.LogError("El rango de cantidad de personajes debe ser mayor a 0 en el valor Y");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable CreateCharacters()
    {
        try
        {
            if (characters.Count == 0)
            {
                List<CharacterBase> charactersSpawned = new List<CharacterBase>();
                foreach (InitialDataSO initialData in initialDataSelected)
                {
                    CharacterData characterData = new CharacterData
                    {
                        id = initialData.id,
                        subId = initialData.subId,
                        level = 1,
                        mastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
                    };
                    characterData.items = new SerializedDictionary<CharacterData.CharacterItemInfo, CharacterData.CharacterItem>
                    {
                        {new CharacterData.CharacterItemInfo{index = 0, typeCharacterItem = CharacterData.TypeCharacterItem.Weapon}, new CharacterData.CharacterItem()},
                        {new CharacterData.CharacterItemInfo{index = 1, typeCharacterItem = CharacterData.TypeCharacterItem.Object1}, new CharacterData.CharacterItem()},
                        {new CharacterData.CharacterItemInfo{index = 2, typeCharacterItem = CharacterData.TypeCharacterItem.Object2}, new CharacterData.CharacterItem()},
                        {new CharacterData.CharacterItemInfo{index = 3, typeCharacterItem = CharacterData.TypeCharacterItem.Object3}, new CharacterData.CharacterItem()}
                    };
                    characterData.statistics = initialData.CloneStatistics();
                    characterData.mastery = initialData.CloneMastery();
                    characterData.skills = initialData.CloneSkills();

                    foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in characterData.statistics)
                    {
                        if (statistic.Key != CharacterData.TypeStatistic.Exp)
                        {
                            statistic.Value.RefreshValue();
                            statistic.Value.SetMaxValue();
                        }
                        else
                        {
                            statistic.Value.baseValue = 15;
                            statistic.Value.RefreshValue();
                        }
                    }
                    CharacterBase character = Instantiate(characterBattlePrefab, Vector3Int.down * 2, Quaternion.identity, charactersContainer).GetComponent<CharacterBase>();
                    character.initialDataSO = GameData.Instance.charactersDataDBSO.data[initialData.id][initialData.subId].initialDataSO;
                    character.characterData = characterData;
                    character.characterModel.characterMeshRenderer.material = materialCharacterEnemy;
                    character.characterModel.characterMeshRendererHand.material = materialCharacterEnemy;
                    charactersSpawned.Add(character);
                    await character.InitializeCharacter();
                }
                characters = charactersSpawned;
            }
            else
            {
                foreach (var character in characters)
                {
                    character.characterData.statistics = character.initialDataSO.CloneStatistics();
                    character.characterData.mastery = character.initialDataSO.CloneMastery();
                    character.characterData.skills = character.initialDataSO.CloneSkills();
                    character.characterData.id = character.initialDataSO.id;
                    character.characterData.subId = character.initialDataSO.subId;
                    character.characterModel.characterMeshRenderer.material = materialCharacterEnemy;
                    character.characterModel.characterMeshRendererHand.material = materialCharacterEnemy;
                    foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in character.characterData.statistics)
                    {
                        if (statistic.Key != CharacterData.TypeStatistic.Exp)
                        {
                            statistic.Value.RefreshValue();
                            statistic.Value.SetMaxValue();
                        }
                        else
                        {
                            statistic.Value.baseValue = 15;
                            statistic.Value.RefreshValue();
                        }
                    }
                    await character.InitializeCharacter();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable LevelUpCharacters(int baseLevel)
    {
        try
        {
            if (!manualCreateCharacters)
            {
                foreach (var character in characters)
                {
                    int targetLevel = UnityEngine.Random.Range(-5, 5);
                    if (character.characterData.level + targetLevel <= 0) targetLevel = 1;
                    while (character.characterData.level < targetLevel)
                    {
                        CharacterData.Statistic statistic = new CharacterData.Statistic
                        {
                            maxValue = character.characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue * 10
                        };
                        character.TakeExp(statistic);
                    }

                    foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in character.characterData.statistics)
                    {
                        if (statistic.Key != CharacterData.TypeStatistic.Exp)
                        {
                            statistic.Value.RefreshValue();
                            statistic.Value.SetMaxValue();
                        }
                        else
                        {
                            statistic.Value.baseValue = 15;
                            statistic.Value.RefreshValue();
                        }
                    }
                }
            }
            else
            {
                foreach (var character in characters)
                {
                    while (character.characterData.level < baseLevel)
                    {
                        CharacterData.Statistic statistic = new CharacterData.Statistic
                        {
                            maxValue = character.characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue * 10
                        };
                        character.TakeExp(statistic);
                    }

                    foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in character.characterData.statistics)
                    {
                        if (statistic.Key != CharacterData.TypeStatistic.Exp)
                        {
                            statistic.Value.RefreshValue();
                            statistic.Value.SetMaxValue();
                        }
                        else
                        {
                            statistic.Value.baseValue = 15;
                            statistic.Value.RefreshValue();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable SpawnCharactersInBattle()
    {
        try
        {
            foreach (var character in characters)
            {
                if (!manualCreateCharacters)
                {
                    aStarPathFinding.GetRandomAvailablePosition(out GenerateMap.WalkablePositionInfo block);
                    character.gameObject.transform.position = block.pos;
                    character.positionInGrid = block.pos;
                    character.startPositionInGrid = block.pos;
                    block.hasCharacter = character;
                }
                else
                {
                    character.gameObject.transform.position = Vector3Int.RoundToInt(character.gameObject.transform.position);
                    character.positionInGrid = Vector3Int.RoundToInt(character.gameObject.transform.position);
                    character.startPositionInGrid = Vector3Int.RoundToInt(character.gameObject.transform.position);
                    aStarPathFinding.grid[Vector3Int.RoundToInt(character.gameObject.transform.position)].hasCharacter = character;
                }
            }
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion
    [Serializable]
    public class AiAction
    {
        public CharacterBase characterMakeAction;
        public PosibleActions typeAction;
        public List<CharacterBase> posibleTargets;
        public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> posiblePositions;
        public CharacterData.CharacterSkillInfo skill;
    }
    public enum PosibleActions
    {
        Heal,
        Buff,
        Debuff,
        BasicAttack,
        SkillAttack,
        SkillDebuff,
        SkillBuff
    }
}