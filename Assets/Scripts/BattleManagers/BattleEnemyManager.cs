using System.Collections.Generic;
using System.Threading.Tasks;
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
    void Start()
    {
        if (ManagementBattleInfo.Instance) principalCharacter = ManagementBattleInfo.Instance.principalCharacterEnemy;
        generateMap.OnFinishGenerateMap += InitializeCharacters;
    }
    void OnDestroy()
    {
        generateMap.OnFinishGenerateMap -= InitializeCharacters;
    }
    public void InitializeCharacters()
    {
        _ = GetCharactersInitialData();
    }
    public async Task GetCharactersInitialData()
    {
        initialDataSelected.Add(principalCharacter.initialDataSO);
        for (int i = 0; i < Random.Range(5, 20); i++)
        {
            initialDataSelected.Add(GameData.Instance.charactersDataDBSO.GetRandomInitialDataSO());
        }
        await CreateCharacters();
        await LevelUpCharacters();
        await SpawnCharactersInBattle();
    }
    public async Task CreateCharacters()
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
            character.name = character.characterData.name;
            charactersSpawned.Add(character);
            await character.InitializeCharacter();
        }
        characters = charactersSpawned.ToArray();
    }
    public async Task LevelUpCharacters()
    {
        foreach (var character in characters)
        {
            int targetLevel = Random.Range(-5, 5);
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
    public async Task SpawnCharactersInBattle()
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
}