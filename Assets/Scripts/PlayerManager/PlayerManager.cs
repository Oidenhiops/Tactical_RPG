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
    public MenuAllCharacters menuAllCharacters;
    public MenuLiftCharacter menuLiftCharacter;
    public MenuCharacterInfo menuCharacterInfo;
    public MenuThrowCharacter menuThrowCharacter;
    public MouseDecalAnim mouseDecal;
    public bool isDecalMovement;
    public Vector3Int direction;
    public Vector3 movementDirection;
    public Vector3Int currentMousePos;
    public bool cantRotateCamera;
    public bool onRotateCamera;
    public Transform cameraRot;
    public bool directionCamera;
    public Character[] characters;
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
    public bool canShowGridAndDecal;
    public Animator roundStateAnimator;
    public ManagementLanguage roundStateLanguage;
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
        OnCharacterPlayerMakingActions += OnToggleCharacterPlayerMove;
        GameManager.Instance.openCloseScene.OnFinishOpenAnimation += OnFinishOpenAnimation;
    }
    void OnFinishOpenAnimation()
    {
        _ = ChangeRoundState(20);
    }
    public async Task ChangeRoundState(int idText)
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
        foreach (CharacterData characterInfo in GameData.Instance.saveData.characters)
        {
            if (characterInfo.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
            {
                Character character = Instantiate(Resources.Load<GameObject>("Prefabs/Character/Character"), Vector3Int.down * 2, Quaternion.identity).GetComponent<Character>();
                character.characterData = characterInfo;
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
        if (AStarPathFinding.Instance.characterSelected) AStarPathFinding.Instance.EnableGrid(AStarPathFinding.Instance.GetWalkableTiles());
    }
    void HandleMovement(InputAction.CallbackContext context)
    {
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !AnyMenuIsActive() && !menuThrowCharacter.isThrowingCharacter || menuThrowCharacter.menuThrowCharacter.activeSelf)
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
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !AnyMenuIsActive() && !menuThrowCharacter.menuThrowCharacter.activeSelf)
        {
            AStarPathFinding.Instance.ValidateAction(new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z)));
        }
    }
    void HandleRotateCamera(InputAction.CallbackContext context)
    {
        if (actionsManager.isPlayerTurn && !characterPlayerMakingActions && !actionsManager.isChangingTurn && !cantRotateCamera && !onRotateCamera)
        {
            cantRotateCamera = true;
            onRotateCamera = true;
            directionCamera = context.ReadValue<float>() > 0 ? false : true;
            StartCoroutine(RotateCamera());
        }
    }
    public bool AnyMenuIsActive()
    {
        return menuCharacterSelector.menuCharacterSelector.activeSelf ||
               menuCharacterActions.menuCharacterActions.activeSelf ||
               menuGeneralActions.menuGeneralActions.activeSelf ||
               menuAllCharacters.menuAllCharacters.activeSelf ||
               menuCharacterInfo.menuCharacterInfo.activeSelf ||
               menuLiftCharacter.menuLiftCharacter.activeSelf;
    }
    void Update()
    {
        if (direction != Vector3Int.zero && !isDecalMovement)
        {
            Vector3Int lastPos = currentMousePos;
            CameraInfo.Instance.CamDirection(out Vector3 camForward, out Vector3 camRight);
            Vector3 camDirection = (direction.x * camRight + direction.z * camForward).normalized;
            movementDirection = new Vector3
            (
                camDirection.x,
                0,
                camDirection.z
            ).normalized;
            Vector3Int currentPos = new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z));
            currentMousePos = currentPos + new Vector3Int(Mathf.RoundToInt(camDirection.x), Mathf.RoundToInt(camDirection.y), Mathf.RoundToInt(camDirection.z));
            FixHeight(currentMousePos, out GenerateMap.WalkablePositionInfo blockFinded);
            currentMousePos.y = blockFinded != null ? blockFinded.pos.y : 0;
            isDecalMovement = true;
            if (AStarPathFinding.Instance.currentGrid.Count == 0)
            {
                if (Mathf.Abs(currentMousePos.x) > AStarPathFinding.Instance.limitX || Mathf.Abs(currentMousePos.z) > AStarPathFinding.Instance.limitZ)
                {
                    currentMousePos.x = Math.Clamp(currentMousePos.x, -AStarPathFinding.Instance.limitX, AStarPathFinding.Instance.limitX);
                    currentMousePos.z = Math.Clamp(currentMousePos.z, -AStarPathFinding.Instance.limitZ, AStarPathFinding.Instance.limitZ);
                    AStarPathFinding.Instance.GetHighestBlockAt(currentMousePos.x, currentMousePos.z, out GenerateMap.WalkablePositionInfo block);
                    currentMousePos.y = block.pos.y;
                    isDecalMovement = false;
                }
                else
                {
                    StartCoroutine(MovePointerTo(currentMousePos));
                }
            }
            else if (AStarPathFinding.Instance.currentGrid.Count > 0)
            {
                if (!AStarPathFinding.Instance.currentGrid.ContainsKey(currentMousePos))
                {
                    currentMousePos = lastPos;
                    isDecalMovement = false;
                }
                else
                {
                    StartCoroutine(MovePointerTo(currentMousePos));
                }
            }
        }
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
        AStarPathFinding.Instance.GetHighestBlockAt(posToFix.x, posToFix.z, out GenerateMap.WalkablePositionInfo block);
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
    }
    public void MovePointerToInstant(Vector3Int posToGo)
    {
        mouseDecal.transform.position = posToGo;
    } 
}
