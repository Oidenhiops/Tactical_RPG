using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BattleEnemyManager : MonoBehaviour
{
    public BattlePlayerManager battlePlayerManager;
    public AStarPathFinding aStarPathFinding;
    public CharacterBase[] characters;
    public GameObject characterBattlePrefab;
    public Transform charactersContainer;
    public List<CharacterBase> charactersList;
    public CharacterBase principalCharacter;
    void Start()
    {
        
    }
    void Update()
    {

    }
    public async Task InitializeCharacterData()
    {
        List<CharacterBase> charactersSpawned = new List<CharacterBase>();
        foreach (KeyValuePair<string, CharacterData> characterInfo in GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters)
        {
            CharacterBase character = Instantiate(characterBattlePrefab, Vector3Int.down * 2, Quaternion.identity, charactersContainer).GetComponent<CharacterBase>();
            character.initialDataSO = GameData.Instance.charactersDataDBSO.data[characterInfo.Value.id][characterInfo.Value.subId].initialDataSO;
            character.isCharacterPlayer = true;
            character.characterData = characterInfo.Value;
            character.name = character.characterData.name;
            charactersSpawned.Add(character);
            await character.InitializeCharacter();
            character.gameObject.SetActive(false);
        }
        characters = charactersSpawned.ToArray();
    }
}
