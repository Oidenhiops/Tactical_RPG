using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuHelper : MonoBehaviour
{
    public Button lastButtonSelected;
    public InputAction handleBack;
    public Character characterView;
    void Awake()
    {
        handleBack.Enable();
        if (GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].principalCharacterName != "")
        {
            var characterInfo = GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].
                characters[GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].principalCharacterName];
            characterView.initialDataSO = GameData.Instance.charactersDataDBSO.data[characterInfo.id][characterInfo.subId].initialDataSO;
        }
        _ = characterView.InitializeCharacter();
    }
    void OnDestroy()
    {
        handleBack.Disable();
    }
}
