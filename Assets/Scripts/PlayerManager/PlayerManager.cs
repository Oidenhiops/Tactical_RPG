using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public CharacterActions characterActions;
    public ActionsManager actionsManager;
    public MenuCharacterSelector menuCharacterSelector;
    public MenuGeneralActions menuGeneralActions;
    public MenuCharacterActions menuCharacterActions;
    public MenuAttackCharacter menuAttackCharacter;
    public MenuAllCharacters menuAllCharacters;
    public MenuLiftCharacter menuLiftCharacter;
    public MenuCharacterInfo menuCharacterInfo;
    public MenuThrowCharacter menuThrowCharacter;
    public MenuItemsCharacter menuItemsCharacter;
    public MenuSkillsCharacter menuSkillsCharacter;
    public AStarPathFinding aStarPathFinding;
    public MouseDecalAnim mouseDecal;
    public Vector3Int currentMousePos;
    public Transform cameraRot;
    public bool canShowGridAndDecal;
    public CharacterBase[] characters;
    public Animator roundStateAnimator;
    public ManagementLanguage roundStateLanguage;
    public Transform charactersContainer;
    public bool _characterPlayerMakingActions;
    public Action<bool, bool> OnCharacterPlayerMakingActions;
    public bool characterPlayerMakingActions
    {
        get => _characterPlayerMakingActions;
        set
        {
            if (_characterPlayerMakingActions != value)
            {
                _characterPlayerMakingActions = value;
                OnCharacterPlayerMakingActions?.Invoke(_characterPlayerMakingActions, canShowGridAndDecal);
            }
        }
    }
    public GameObject characterBattlePrefab;
    public bool isDecalMovement;
    Vector3Int direction;
    Vector3 camDirection;
    bool cantRotateCamera;
    bool onRotateCamera;
    bool directionCamera;
    void Awake()
    {
        if (Instance == null) Instance = this;
        characterActions = new CharacterActions();
        characterActions.Enable();
        InitializeActions();
        _ = InitializeCharacterData();
    }
    void OnDestroy()
    {
        characterActions.CharacterInputs.Movement.performed -= HandleMovement;
        characterActions.CharacterInputs.Movement.canceled -= HandleMovement;
        characterActions.CharacterInputs.Interact.performed -= HandleAction;
        characterActions.CharacterInputs.Interact.performed -= menuThrowCharacter.OnHandleTrow;
        characterActions.CharacterInputs.RotateCamera.performed -= HandleRotateCamera;
        characterActions.CharacterInputs.ActiveGeneralActions.performed -= HandleMenuGeneralActions;
        OnCharacterPlayerMakingActions -= OnToggleCharacterPlayerMove;
        GameManager.Instance.openCloseScene.OnFinishOpenAnimation -= OnFinishOpenAnimation;
    }
    void InitializeActions()
    {
        characterActions.CharacterInputs.Movement.performed += HandleMovement;
        characterActions.CharacterInputs.Movement.canceled += HandleMovement;
        characterActions.CharacterInputs.Interact.performed += HandleAction;
        characterActions.CharacterInputs.Interact.performed += menuThrowCharacter.OnHandleTrow;
        characterActions.CharacterInputs.RotateCamera.started += HandleRotateCamera;
        characterActions.CharacterInputs.ActiveGeneralActions.performed += HandleMenuGeneralActions;
        OnCharacterPlayerMakingActions += OnToggleCharacterPlayerMove;
        GameManager.Instance.openCloseScene.OnFinishOpenAnimation += OnFinishOpenAnimation;
    }
    void OnFinishOpenAnimation()
    {
        _ = ChangeRoundState("game_scene_menu_round_state_start");
    }
    public async Task ChangeRoundState(string idText)
    {
        roundStateLanguage.ChangeTextById(idText);
        roundStateAnimator.Play("ShowStateOpen");
        while (true)
        {
            await Awaitable.NextFrameAsync();
            if (roundStateAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowStateIdle"))
            {
                actionsManager.isPlayerTurn = !actionsManager.isPlayerTurn;
                break;
            }
        }
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
    void OnToggleCharacterPlayerMove(bool state, bool canShowGridAndDecal)
    {
        if (state || !canShowGridAndDecal)
        {
            DisableVisuals();
        }
        else if (canShowGridAndDecal)
        {
            EnableVisuals();
        }
    }
    public void DisableVisuals()
    {
        mouseDecal.decal.gameObject.SetActive(false);
        if (aStarPathFinding.currentGrid.Count > 0) aStarPathFinding.DisableGrid();
    }
    public void EnableVisuals()
    {
        mouseDecal.decal.gameObject.SetActive(true);
        if (aStarPathFinding.characterSelected) aStarPathFinding.EnableGrid(aStarPathFinding.GetWalkableTiles(), Color.magenta);
    }
    void HandleMovement(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isPause) return;
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera &&
            !AnyMenuIsActive() && !menuThrowCharacter.isThrowingCharacter || menuThrowCharacter.menuThrowCharacter.activeSelf &&
            !menuThrowCharacter.isThrowingCharacter || menuSkillsCharacter.menuSkillsCharacter.activeSelf && !menuSkillsCharacter.menuSkillSelectSkill.activeSelf &&
            menuSkillsCharacter.canMovePointer)
        {
            if (context.performed)
            {
                Vector2 input = context.ReadValue<Vector2>();
                direction = new Vector3Int(Mathf.RoundToInt(input.x), 0, Mathf.RoundToInt(input.y));
            }
            else
            {
                direction = Vector3Int.zero;
            }
        }
    }
    void HandleAction(InputAction.CallbackContext context)
    {
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !AnyMenuIsActive() &&
            !menuThrowCharacter.menuThrowCharacter.activeSelf && !GameManager.Instance.isPause)
        {
            aStarPathFinding.ValidateAction(new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z)));
        }
    }
    void HandleRotateCamera(InputAction.CallbackContext context)
    {
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !onRotateCamera && !AnyMenuIsActive() &&
            !GameManager.Instance.isPause)
        {
            cantRotateCamera = true;
            onRotateCamera = true;
            directionCamera = context.ReadValue<float>() > 0 ? false : true;
            StartCoroutine(RotateCamera());
        }
    }
    void HandleMenuGeneralActions(InputAction.CallbackContext callbackContext)
    {
        if (actionsManager.isPlayerTurn && !AnyMenuIsActive() && !GameManager.Instance.isPause)
        {
            _= menuGeneralActions.EnableMenu();
        }
    }
    public bool AnyMenuIsActive()
    {
        return menuCharacterSelector.menuCharacterSelector.activeSelf ||
               menuCharacterActions.menuCharacterActions.activeSelf ||
               menuGeneralActions.menuGeneralActions.activeSelf ||
               menuAllCharacters.menuAllCharacters.activeSelf ||
               menuCharacterInfo.menuCharacterInfo.activeSelf ||
               menuLiftCharacter.menuLiftCharacter.activeSelf ||
               menuAttackCharacter.menuAttackCharacter.activeSelf ||
               menuItemsCharacter.menuItemCharacters.activeSelf ||
               menuSkillsCharacter.isMenuActive;
    }
    void Update()
    {
        if (direction != Vector3Int.zero && !isDecalMovement)
        {
            Vector3Int lastPos = currentMousePos;
            CameraInfo.Instance.CamDirection(direction, out Vector3 directionFromCamera);
            camDirection = directionFromCamera;
            Vector3Int currentPos = new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z));
            currentMousePos = currentPos + new Vector3Int(Mathf.RoundToInt(camDirection.x), Mathf.RoundToInt(camDirection.y), Mathf.RoundToInt(camDirection.z));
            FixHeight(currentMousePos, out GenerateMap.WalkablePositionInfo blockFinded);
            currentMousePos.y = blockFinded != null ? blockFinded.pos.y : 0;
            isDecalMovement = true;
            if (aStarPathFinding.currentGrid.Count == 0)
            {
                if (currentMousePos.x < aStarPathFinding.limitX.x || currentMousePos.x > aStarPathFinding.limitX.y)
                {
                    currentMousePos.x = Math.Clamp(currentMousePos.x, aStarPathFinding.limitX.x, aStarPathFinding.limitX.y);
                }
                if (currentMousePos.z < aStarPathFinding.limitZ.x || currentMousePos.z > aStarPathFinding.limitZ.y)
                {
                    currentMousePos.z = Math.Clamp(currentMousePos.z, aStarPathFinding.limitZ.x, aStarPathFinding.limitZ.y);
                }
                aStarPathFinding.GetHighestBlockAt(currentMousePos, out GenerateMap.WalkablePositionInfo block);
                currentMousePos.y = block != null ? block.pos.y : 0;
                StartCoroutine(MovePointerTo(currentMousePos));
            }
            else
            {
                if (!aStarPathFinding.currentGrid.ContainsKey(currentMousePos))
                {
                    if (aStarPathFinding.currentGrid.ContainsKey(GetNearestPositionAccordingDirection(lastPos, out Vector3Int newPos)) && newPos != lastPos)
                    {
                        currentMousePos = newPos;
                        StartCoroutine(MovePointerTo(currentMousePos));
                    }
                    else
                    {
                        currentMousePos = lastPos;
                        isDecalMovement = false;
                    }
                }
                else
                {
                    StartCoroutine(MovePointerTo(currentMousePos));
                }
            }
        }
    }
    Vector3Int GetNearestPositionAccordingDirection(Vector3Int lastPos, out Vector3Int newPos) {
        newPos = lastPos;
        for (int i = 0; i < 10; i++)
        {
            aStarPathFinding.GetHighestBlockAt(currentMousePos + Vector3Int.RoundToInt(camDirection * i), out GenerateMap.WalkablePositionInfo blockFinded);
            if (blockFinded != null && aStarPathFinding.currentGrid.ContainsKey(blockFinded.pos))
            {
                newPos = blockFinded.pos;
                return newPos;
            }
        }
        return lastPos;
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
    private void FixHeight(Vector3Int posToFix, out GenerateMap.WalkablePositionInfo blockFinded)
    {
        aStarPathFinding.GetHighestBlockAt(posToFix, out GenerateMap.WalkablePositionInfo block);
        blockFinded = block;
    }
    public IEnumerator MovePointerTo(Vector3Int posToGo)
    {
        isDecalMovement = true;
        Vector3 startPos = mouseDecal.transform.position;
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            mouseDecal.transform.position = Vector3.Lerp(startPos, posToGo, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mouseDecal.transform.position = posToGo;
        isDecalMovement = false;

        if (aStarPathFinding.characterSelected)
        {
            if (aStarPathFinding.characterSelected.positionInGrid != posToGo)
            {
                if (aStarPathFinding.characterSelected.positionInGrid.x == posToGo.x)
                {
                    aStarPathFinding.characterSelected.nextDirection.x = aStarPathFinding.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
                else
                {
                    aStarPathFinding.characterSelected.nextDirection.x = aStarPathFinding.characterSelected.positionInGrid.x < posToGo.x ? -1 : 1;
                }
                if (aStarPathFinding.characterSelected.positionInGrid.z == posToGo.z)
                {
                    aStarPathFinding.characterSelected.nextDirection.z = aStarPathFinding.characterSelected.positionInGrid.x < posToGo.x ? 1 : -1;
                }
                else
                {
                    aStarPathFinding.characterSelected.nextDirection.z = aStarPathFinding.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
            }
        }
    }
    public void MovePointerToInstant(Vector3Int posToGo)
    {
        mouseDecal.transform.position = posToGo;

        if (aStarPathFinding.characterSelected)
        {
            if (aStarPathFinding.characterSelected.positionInGrid != posToGo)
            {
                if (aStarPathFinding.characterSelected.positionInGrid.x == posToGo.x)
                {
                    aStarPathFinding.characterSelected.nextDirection.x = aStarPathFinding.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
                else
                {
                    aStarPathFinding.characterSelected.nextDirection.x = aStarPathFinding.characterSelected.positionInGrid.x < posToGo.x ? -1 : 1;
                }
                if (aStarPathFinding.characterSelected.positionInGrid.z == posToGo.z)
                {
                    aStarPathFinding.characterSelected.nextDirection.z = aStarPathFinding.characterSelected.positionInGrid.x < posToGo.x ? 1 : -1;
                }
                else
                {
                    aStarPathFinding.characterSelected.nextDirection.z = aStarPathFinding.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
            }
        }

    } 
}
