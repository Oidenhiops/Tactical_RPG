using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public CharacterActions characterActions;
    public Transform decal;
    public bool onMovement;
    public Vector3Int direction;
    public Vector3 movementDirection;
    public Vector3Int nextPos;
    public bool cantRotateCamera;
    public bool onRotateCamera;
    public Transform cameraRot;
    public bool directionCamera;
    void Awake()
    {
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
    }

    void HandleMovement(InputAction.CallbackContext context)
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
    void HandleAction(InputAction.CallbackContext context)
    {
        if (!onMovement)
        {
            AStarPathFinding.Instance.ValidateAction(new Vector3Int(Mathf.RoundToInt(decal.transform.position.x), Mathf.RoundToInt(decal.transform.position.y), Mathf.RoundToInt(decal.transform.position.z)));
        }
    }
    void HandleRotateCamera(InputAction.CallbackContext context)
    {
        if (!cantRotateCamera)
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
            Vector3Int currentPos = new Vector3Int(Mathf.RoundToInt(decal.transform.position.x), Mathf.RoundToInt(decal.transform.position.y), Mathf.RoundToInt(decal.transform.position.z));
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
        Vector3 startPos = decal.transform.position;
        Vector3 endPos = nextPos;
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            decal.transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        decal.transform.position = endPos;
        onMovement = false;
    }
}
