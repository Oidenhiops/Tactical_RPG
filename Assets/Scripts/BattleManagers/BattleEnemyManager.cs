using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
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
    public SerializedDictionary<CharacterBase, List<AiAction>> possibleActions = new SerializedDictionary<CharacterBase, List<AiAction>>();
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
    public CharacterBase GetLowestHealthAlly()
    {
        return characters.OrderBy(a => a.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue / a.characterData.statistics[CharacterData.TypeStatistic.Hp].maxValue).FirstOrDefault();
    }
    [NaughtyAttributes.Button]
    public void GetEnemiesForAttack()
    {
        List<CharacterBase> enemiesNear;
        Vector2 playerPos;
        Vector2 enemyPos;
        int weaponRange;
        int moveRadius;
        foreach (var characterEnemy in characters)
        {
            enemiesNear = new List<CharacterBase>();
            foreach (var characterPlayer in battlePlayerManager.characters)
            {
                moveRadius = characterEnemy.characterData.GetMovementRadius();
                weaponRange = 0;
                if (characterEnemy.characterData.GetCurrentWeapon(out CharacterData.CharacterItem weaponData))
                {
                    weaponRange = weaponData.itemBaseSO.gridSize;
                }

                playerPos = new Vector2(characterPlayer.transform.position.x, characterPlayer.transform.position.z);
                enemyPos = new Vector2(characterEnemy.transform.position.x, characterEnemy.transform.position.z);

                if (characterPlayer.gameObject.activeSelf && Vector2.Distance(enemyPos, playerPos) <= moveRadius + weaponRange)
                {
                    enemiesNear.Add(characterPlayer);
                }
            }
            if (enemiesNear.Count > 0)
            {
                if (possibleActions.ContainsKey(characterEnemy))
                {
                    possibleActions[characterEnemy].Add(new AiAction
                    {
                        characterMakeAction = characterEnemy,
                        typeAction = TypeAction.Attack,
                        posibleTargets = enemiesNear
                    });
                }
                else
                {
                    possibleActions.Add(characterEnemy, new List<AiAction>
                    {
                        new AiAction
                        {
                            characterMakeAction = characterEnemy,
                            typeAction = TypeAction.Attack,
                            posibleTargets = enemiesNear
                        }
                    });
                }
            }
        }
    }
    public void InitializeCharacters()
    {
        _ = GetCharactersInitialData();
    }
    public async Awaitable GetCharactersInitialData()
    {
        try
        {
            initialDataSelected.Add(principalCharacter.initialDataSO);
            for (int i = 0; i < UnityEngine.Random.Range(5, 20); i++)
            {
                initialDataSelected.Add(GameData.Instance.charactersDataDBSO.GetRandomInitialDataSO());
            }
            await CreateCharacters();
            await LevelUpCharacters();
            await SpawnCharactersInBattle();
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
            List<CharacterBase> charactersSpawned = new List<CharacterBase>();
            foreach (InitialDataSO initialData in initialDataSelected)
            {
                CharacterData characterData = new CharacterData
                {
                    id = initialData.id,
                    subId = initialData.subId,
                    name = GameData.Instance.charactersDataDBSO.GenerateFantasyName(),
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
                characterData.statistics = GameData.Instance.charactersDataDBSO.data[characterData.id][characterData.subId].initialDataSO.CloneStatistics();
                characterData.mastery = GameData.Instance.charactersDataDBSO.data[characterData.id][characterData.subId].initialDataSO.CloneMastery();
                characterData.skills = GameData.Instance.charactersDataDBSO.data[characterData.id][characterData.subId].initialDataSO.CloneSkills();

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
                character.name = character.characterData.name;
                charactersSpawned.Add(character);
                await character.InitializeCharacter();
            }
            characters = charactersSpawned.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable LevelUpCharacters()
    {
        try
        {
            foreach (var character in characters)
            {
                int targetLevel = UnityEngine.Random.Range(-5, 5);
                if (character.characterData.level + targetLevel <= 0) targetLevel = 1;
                while (character.characterData.level < targetLevel)
                {
                    CharacterData.Statistic statistic = new CharacterData.Statistic
                    {
                        maxValue = character.characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue
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
                aStarPathFinding.GetRandomAvailablePosition(out GenerateMap.WalkablePositionInfo block);
                character.gameObject.transform.position = block.pos;
                character.positionInGrid = block.pos;
                character.startPositionInGrid = block.pos;
                block.hasCharacter = character;
            }
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    [Serializable]
    public class AiAction
    {
        public CharacterBase characterMakeAction;
        public TypeAction typeAction;
        public List<CharacterBase> posibleTargets;
    }
    public enum TypeAction
    {
        Move,
        Attack,
        UseItem,
        Wait,
    }
}