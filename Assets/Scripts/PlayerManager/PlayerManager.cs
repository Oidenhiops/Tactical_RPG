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
    public MouseDecalAnim mouseDecal;
    public Vector3Int currentMousePos;
    public Transform cameraRot;
    public bool canShowGridAndDecal;
    public Character[] characters;
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
    public GameObject generalCharacterPrefab;
    bool isDecalMovement;
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
        characterActions.CharacterInputs.ChangeSubMenu.performed -= HandleChangeSubMenu;
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
        characterActions.CharacterInputs.ChangeSubMenu.performed += HandleChangeSubMenu;
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
        List<Character> charactersSpawned = new List<Character>();
        foreach (KeyValuePair<string, CharacterData> characterInfo in GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters)
        {
            if (characterInfo.Value.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
            {
                Character character = Instantiate(generalCharacterPrefab, Vector3Int.down * 2, Quaternion.identity, charactersContainer).GetComponent<Character>();
                character.isCharacterPlayer = true;
                character.characterData = characterInfo.Value;
                character.name = character.characterData.name;
                charactersSpawned.Add(character);
                await character.InitializeCharacter();
                character.gameObject.SetActive(false);
            }
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
        if (AStarPathFinding.Instance.currentGrid.Count > 0) AStarPathFinding.Instance.DisableGrid();
    }
    public void EnableVisuals()
    {
        mouseDecal.decal.gameObject.SetActive(true);
        if (AStarPathFinding.Instance.characterSelected) AStarPathFinding.Instance.EnableGrid(AStarPathFinding.Instance.GetWalkableTiles(), Color.magenta);
    }
    void HandleMovement(InputAction.CallbackContext context)
    {
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !AnyMenuIsActive() && !menuThrowCharacter.isThrowingCharacter || menuThrowCharacter.menuThrowCharacter.activeSelf && !menuThrowCharacter.isThrowingCharacter)
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
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !AnyMenuIsActive() && !menuThrowCharacter.menuThrowCharacter.activeSelf && !GameManager.Instance.isPause)
        {
            AStarPathFinding.Instance.ValidateAction(new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z)));
        }
    }
    void HandleRotateCamera(InputAction.CallbackContext context)
    {
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !onRotateCamera && !AnyMenuIsActive() && !GameManager.Instance.isPause)
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
            menuGeneralActions.EnableMenu();
        }
    }
    void HandleChangeSubMenu(InputAction.CallbackContext context)
    {
        if (menuCharacterInfo.isMenuActive && !GameManager.Instance.isPause)
        {
            menuCharacterInfo.ChangeSubMenu();
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
               menuItemsCharacter.menuItemCharacters.activeSelf;
    }
    void Update()
    {
        if (direction != Vector3Int.zero && !isDecalMovement)
        {
            Vector3Int lastPos = currentMousePos;
            CameraInfo.Instance.CamDirection(out Vector3 camForward, out Vector3 camRight);
            camDirection = (direction.x * camRight + direction.z * camForward).normalized;
            Vector3Int currentPos = new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z));
            currentMousePos = currentPos + new Vector3Int(Mathf.RoundToInt(camDirection.x), Mathf.RoundToInt(camDirection.y), Mathf.RoundToInt(camDirection.z));
            FixHeight(currentMousePos, out GenerateMap.WalkablePositionInfo blockFinded);
            currentMousePos.y = blockFinded != null ? blockFinded.pos.y : 0;
            isDecalMovement = true;
            if (AStarPathFinding.Instance.currentGrid.Count == 0)
            {
                if (currentMousePos.x < AStarPathFinding.Instance.limitX.x || currentMousePos.x > AStarPathFinding.Instance.limitX.y)
                {
                    currentMousePos.x = Math.Clamp(currentMousePos.x, AStarPathFinding.Instance.limitX.x, AStarPathFinding.Instance.limitX.y);
                }
                if (currentMousePos.z < AStarPathFinding.Instance.limitZ.x || currentMousePos.z > AStarPathFinding.Instance.limitZ.y)
                {
                    currentMousePos.z = Math.Clamp(currentMousePos.z, AStarPathFinding.Instance.limitZ.x, AStarPathFinding.Instance.limitZ.y);
                }
                AStarPathFinding.Instance.GetHighestBlockAt(currentMousePos, out GenerateMap.WalkablePositionInfo block);
                currentMousePos.y = block != null ? block.pos.y : 0;
                StartCoroutine(MovePointerTo(currentMousePos));
            }
            else
            {
                if (!AStarPathFinding.Instance.currentGrid.ContainsKey(currentMousePos))
                {
                    if (AStarPathFinding.Instance.currentGrid.ContainsKey(GetNearestPositionAccordingDirection(lastPos, out Vector3Int newPos)) && newPos != lastPos)
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
            AStarPathFinding.Instance.GetHighestBlockAt(currentMousePos + Vector3Int.RoundToInt(camDirection * i), out GenerateMap.WalkablePositionInfo blockFinded);
            if (blockFinded != null && AStarPathFinding.Instance.currentGrid.ContainsKey(blockFinded.pos))
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
        AStarPathFinding.Instance.GetHighestBlockAt(posToFix, out GenerateMap.WalkablePositionInfo block);
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

        if (AStarPathFinding.Instance.characterSelected)
        {
            if (AStarPathFinding.Instance.characterSelected.positionInGrid != posToGo)
            {
                if (AStarPathFinding.Instance.characterSelected.positionInGrid.x == posToGo.x)
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.x = AStarPathFinding.Instance.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
                else
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.x = AStarPathFinding.Instance.characterSelected.positionInGrid.x < posToGo.x ? -1 : 1;
                }
                if (AStarPathFinding.Instance.characterSelected.positionInGrid.z == posToGo.z)
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.z = AStarPathFinding.Instance.characterSelected.positionInGrid.x < posToGo.x ? 1 : -1;
                }
                else
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.z = AStarPathFinding.Instance.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
            }
        }
    }
    public void MovePointerToInstant(Vector3Int posToGo)
    {
        mouseDecal.transform.position = posToGo;

        if (AStarPathFinding.Instance.characterSelected)
        {
            if (AStarPathFinding.Instance.characterSelected.positionInGrid != posToGo)
            {
                if (AStarPathFinding.Instance.characterSelected.positionInGrid.x == posToGo.x)
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.x = AStarPathFinding.Instance.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
                else
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.x = AStarPathFinding.Instance.characterSelected.positionInGrid.x < posToGo.x ? -1 : 1;
                }
                if (AStarPathFinding.Instance.characterSelected.positionInGrid.z == posToGo.z)
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.z = AStarPathFinding.Instance.characterSelected.positionInGrid.x < posToGo.x ? 1 : -1;
                }
                else
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.z = AStarPathFinding.Instance.characterSelected.positionInGrid.z < posToGo.z ? 1 : -1;
                }
            }
        }

    } 
}
