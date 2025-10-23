using UnityEngine;

public class ManagementBattleInfo : MonoBehaviour
{
    public static ManagementBattleInfo Instance { get; private set; }
    public GenerateMap generateMap;
    public CharacterBase principalCharacterEnemy;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void GetInfo()
    {
        
    }
}
