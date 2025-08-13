using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public CharacterActions characterActions;
    public ActionsManager actionsManager;
    public MouseDecalAnim mouseDecal;
    public bool onMovement;
    public Vector3Int direction;
    public Vector3 movementDirection;
    public Vector3Int nextPos;
    public bool cantRotateCamera;
    public bool onRotateCamera;
    public Transform cameraRot;
    public bool directionCamera;
    public bool _characterPlayerOnMove;
    public Action<bool> OnCharacterPlayerMove;
    public bool characterPlayerOnMove
    {
        get => _characterPlayerOnMove;
        set
        {
            if (_characterPlayerOnMove != value)
            {
                _characterPlayerOnMove = value;
                OnCharacterPlayerMove?.Invoke(_characterPlayerOnMove);
            }
        }
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        characterActions = new CharacterActions();
        characterActions.Enable();

        InitializeActions();
    }
    void InitializeActions()
    {
        characterActions.CharacterInputs.Movement.performed += HandleMovement;
        characterActions.CharacterInputs.Movement.canceled += HandleMovement;
        characterActions.CharacterInputs.Interact.performed += HandleAction;
        characterActions.CharacterInputs.RotateCamera.started += HandleRotateCamera;
        OnCharacterPlayerMove += OnToggleCharacterPlayerMove;
    }
    void OnToggleCharacterPlayerMove(bool state)
    {
        mouseDecal.decal.gameObject.SetActive(!state);
    }
    void HandleMovement(InputAction.CallbackContext context)
    {
        if (!characterPlayerOnMove)
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
        if (!onMovement)
        {
            AStarPathFinding.Instance.ValidateAction(new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z)));
        }
    }
    void HandleRotateCamera(InputAction.CallbackContext context)
    {
        if (!characterPlayerOnMove && !cantRotateCamera)
        {
            cantRotateCamera = true;
            onRotateCamera = true;
            float direction = context.ReadValue<float>();
            directionCamera = direction > 0 ? false : true;
        }
    }
    void Update()
    {
        if (direction != Vector3Int.zero && !onMovement)
        {
            onMovement = true;

            CameraInfo.Instance.CamDirection(out Vector3 camForward, out Vector3 camRight);
            Vector3 camDirection = (direction.x * camRight + direction.z * camForward).normalized;
            movementDirection = new Vector3
            (
                camDirection.x,
                0,
                camDirection.z
            ).normalized;
            Vector3Int currentPos = new Vector3Int(Mathf.RoundToInt(mouseDecal.transform.position.x), Mathf.RoundToInt(mouseDecal.transform.position.y), Mathf.RoundToInt(mouseDecal.transform.position.z));
            nextPos = currentPos + new Vector3Int(Mathf.RoundToInt(camDirection.x), Mathf.RoundToInt(camDirection.y), Mathf.RoundToInt(camDirection.z));
            FixHeight(nextPos, out GenerateMap.WalkablePositionInfo blockFinded);
            nextPos.y = blockFinded != null ? blockFinded.pos.y : 0;
            onMovement = true;
            if (Mathf.Abs(nextPos.x) > AStarPathFinding.Instance.limitX || Mathf.Abs(nextPos.z) > AStarPathFinding.Instance.limitZ)
            {
                if (nextPos.x > AStarPathFinding.Instance.limitX) nextPos.x = AStarPathFinding.Instance.limitX;
                if (nextPos.z > AStarPathFinding.Instance.limitZ) nextPos.z = AStarPathFinding.Instance.limitZ;
                onMovement = false;
            }
            else
            {
                StartCoroutine(MovePointer());
            }
        }
        if (onRotateCamera)
        {
            onRotateCamera = false;
            StartCoroutine(RotateCamera());
        }
    }

    IEnumerator RotateCamera()
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
        onRotateCamera = false;
        cantRotateCamera = false; 
    }

    private void FixHeight(Vector3Int posToFix, out GenerateMap.WalkablePositionInfo blockFinded)
    {
        AStarPathFinding.Instance.GetHighestBlockAt(posToFix.x, posToFix.z, out GenerateMap.WalkablePositionInfo block);
        blockFinded = block;
    }

    IEnumerator MovePointer()
    {
        Vector3 startPos = mouseDecal.transform.position;
        Vector3 endPos = nextPos;
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            mouseDecal.transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mouseDecal.transform.position = endPos;
        onMovement = false;
    }
}
