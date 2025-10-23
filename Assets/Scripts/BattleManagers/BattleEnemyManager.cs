using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class BattleEnemyManager : MonoBehaviour
{
    public BattlePlayerManager battlePlayerManager;
    public AStarPathFinding aStarPathFinding;
    public CharacterBase[] characters;
    public GameObject characterBattlePrefab;
    public Transform charactersContainer;
    public List<InitialDataSO> charactersList;
    public CharacterBase principalCharacter;
    void Start()
    {
        if (ManagementBattleInfo.Instance) principalCharacter = ManagementBattleInfo.Instance.principalCharacterEnemy;

    }
    public void GetCharactersInitialData()
    {
        charactersList.Add(principalCharacter.initialDataSO);
    }
    public async Task InitializeCharacterData()
    {
        List<CharacterBase> charactersSpawned = new List<CharacterBase>();
        foreach (InitialDataSO characterInfo in charactersList)
        {
            CharacterData character = new CharacterData
            {
                id = characterInfo.id,
                subId = characterInfo.subId,
                name = GameData.Instance.charactersDataDBSO.GenerateFantasyName(),
                level = 1,
                mastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
            };
            character.statistics = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneStatistics();
            character.mastery = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneMastery();
            character.skills = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneSkills();
            foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in character.statistics)
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
        characters = charactersSpawned.ToArray();
    }
}
