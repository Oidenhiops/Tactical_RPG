using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEditor.XR;
using UnityEngine;

public class BattleEnemyManager : MonoBehaviour
{
    public GenerateMap generateMap;
    public BattlePlayerManager battlePlayerManager;
    public AStarPathFinding aStarPathFinding;
    public CharacterBase[] characters;
    public GameObject characterBattlePrefab;
    public Transform charactersContainer;
    public List<InitialDataSO> initialDataSelected;
    public CharacterBase principalCharacter;
    public Material materialCharacterEnemy;
    public CharacterBase characterForTest;
    public Vector2Int amountCharacters;
    public bool manualCreateCharacters;
    public int targetLevel;
    void Start()
    {
        if (ManagementBattleInfo.Instance) principalCharacter = ManagementBattleInfo.Instance.principalCharacterEnemy;
        generateMap.OnFinishGenerateMap += InitializeCharacters;
        battlePlayerManager.actionsManager.OnEndTurn += CreateStrategy;
    }
    void OnDestroy()
    {
        generateMap.OnFinishGenerateMap -= InitializeCharacters;
        battlePlayerManager.actionsManager.OnEndTurn -= CreateStrategy;
    }
    public void CreateStrategy()
    {
        if (!battlePlayerManager.actionsManager.isPlayerTurn)
        {
            
        }
    }
    [NaughtyAttributes.Button]
    public void GetCharacterActions()
    {
        List<CharacterBase> posibleTargets = new List<CharacterBase>();
        List<AiAction> posibleActions = new List<AiAction>();
        GetPosibleCharactersForMakeAction(ref posibleTargets, characterForTest);
        GetPosibleActionsForMakeAction(ref posibleActions, ref posibleTargets, characterForTest);
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
    public void GetPosibleActionsForMakeAction(ref List<AiAction> posibleActions, ref List<CharacterBase> posibleTargets, CharacterBase characterForValidate)
    {
        foreach (var posibleTarget in posibleTargets)
        {
            if (posibleTarget.isCharacterPlayer)
            {
                //Enemy
                #region Basic Attack Action

                aStarPathFinding.GetTilesForMakeAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> attackPositions, characterForValidate, posibleTarget);
                if (attackPositions.Count > 0 && aStarPathFinding.PathExists(attackPositions.ElementAt(UnityEngine.Random.Range(0, attackPositions.Count)).Value.pos, characterForValidate.positionInGrid, aStarPathFinding.GetWalkableTiles(characterForValidate)))
                {
                    AiAction aiAction = new AiAction
                    {
                        characterMakeAction = characterForValidate,
                        typeAction = PosibleActions.BasicAttack,
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
                                if (skillPositions.Count > 0 && aStarPathFinding.PathExists(skillPositions.ElementAt(UnityEngine.Random.Range(0, skillPositions.Count)).Value.pos, characterForValidate.positionInGrid, aStarPathFinding.GetWalkableTiles(characterForValidate)))
                                {
                                    AiAction aiAction = new AiAction
                                    {
                                        characterMakeAction = characterForValidate,
                                        typeAction = PosibleActions.SkillAttack,
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
            if (characters.Length == 0)
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
                characters = charactersSpawned.ToArray();
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
                //Temporal, need change
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