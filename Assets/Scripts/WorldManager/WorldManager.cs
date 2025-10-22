using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }
    public CharacterActions characterActions;
    public GameObject worldContainer;
    public CharacterBase characterWorld;
    public GenerateMap currentWorldMap;
    public AStarPathFinding aStarPathFinding;
    bool cantRotateCamera = false;
    bool onRotateCamera = false;
    bool directionCamera = false;
    public Transform cameraRot;
    public bool enemyHitted;
    void Awake()
    {
        if (Instance == null) Instance = this;
        characterActions = new CharacterActions();
        characterActions.Enable();
        _ = InitializeData();
    }
    void OnDestroy()
    {
        characterActions.CharacterInputs.RotateCamera.performed -= HandleRotateCamera;
    }
    async Task InitializeData()
    {
        await InitializeActions();
        await InitializeCharacterData();
    }
    async Task InitializeActions()
    {
        characterActions.CharacterInputs.RotateCamera.started += HandleRotateCamera;
        await Awaitable.NextFrameAsync();
        GameManager.Instance.openCloseScene.AdjustLoading(50);
        await Awaitable.NextFrameAsync();
    }
    async Task InitializeCharacterData()
    {
        var characterInfo = GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].
            characters[GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].principalCharacterName];
        characterWorld.initialDataSO = GameData.Instance.charactersDataDBSO.data[characterInfo.id][characterInfo.subId].initialDataSO;
        characterWorld.isCharacterPlayer = true;
        characterWorld.name = characterInfo.name;
        characterWorld.characterData = characterInfo;
        characterWorld.transform.position = GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].positionSave;
        await characterWorld.InitializeCharacter();
        await Awaitable.NextFrameAsync();
        GameManager.Instance.openCloseScene.AdjustLoading(100);
        await Awaitable.NextFrameAsync();
    }
    void HandleRotateCamera(InputAction.CallbackContext context)
    {
        if (!cantRotateCamera && !onRotateCamera && !GameManager.Instance.isPause)
        {
            cantRotateCamera = true;
            onRotateCamera = true;
            directionCamera = context.ReadValue<float>() > 0 ? false : true;
            StartCoroutine(RotateCamera());
        }
    }
    public async Task OnEnemyHit()
    {
        enemyHitted = true;
        ManagementBattleInfo.Instance.generateMap = currentWorldMap;
        _ = GameManager.Instance.ChangeScene(GameManager.TypeScene.BattleScene, LoadSceneMode.Additive);
        worldContainer.gameObject.SetActive(false);
    }
    IEnumerator RotateCamera()
    {
        while (characterActions.CharacterInputs.RotateCamera.IsPressed())
        {
            Quaternion startRot = cameraRot.transform.localRotation;
            Quaternion endRot = startRot * Quaternion.Euler(0, directionCamera ? 90 : -90, 0);
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                cameraRot.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cameraRot.transform.localRotation = endRot;
            cantRotateCamera = false;
        }
        onRotateCamera = false;
    }
}
