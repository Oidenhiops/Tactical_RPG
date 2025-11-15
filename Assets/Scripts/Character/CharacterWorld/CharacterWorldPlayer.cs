using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterWorldPlayer : CharacterBase
{
    public Vector3Int movementDirection;
    public bool isOnMovement;
    public CharacterBase characterHitted;
    public IInteractable interactableObject;
    public override void OnEnableHandle()
    {
        WorldManager.Instance.characterActions.CharacterInputs.Movement.started += HandleMovement;
        WorldManager.Instance.characterActions.CharacterInputs.Movement.canceled += HandleMovement;
        WorldManager.Instance.characterActions.CharacterInputs.Interact.performed += HandleAction;
    }
    void OnDisable()
    {
        WorldManager.Instance.characterActions.CharacterInputs.Movement.started -= HandleMovement;
        WorldManager.Instance.characterActions.CharacterInputs.Movement.canceled -= HandleMovement;
        WorldManager.Instance.characterActions.CharacterInputs.Interact.performed -= HandleAction;
    }
    void Update()
    {
        if (isInitialize && characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0 && !WorldManager.Instance.cantMakeActions)
        {
            if (!GameManager.Instance.isPause && GameManager.Instance.startGame)
            {
                if (WorldManager.Instance.characterActions.CharacterInputs.Movement.IsPressed())
                {
                    Vector2 input = WorldManager.Instance.characterActions.CharacterInputs.Movement.ReadValue<Vector2>();
                    CameraInfo.Instance.CamDirection(new Vector3(input.x, 0, input.y), out Vector3 directionFromCamera);
                    movementDirection = Vector3Int.RoundToInt(directionFromCamera);
                    MoveCharacter(movementDirection);
                }
                else if (characterAnimations.currentAnimation.name == "Walk" && !isOnMovement)
                {
                    characterAnimations.MakeAnimation("Idle");
                }
            }
        }
    }
    void HandleMovement(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.isPause && GameManager.Instance.startGame && !isOnMovement && !WorldManager.Instance.cantMakeActions)
        {
            if (!context.performed)
            {
                movementDirection = Vector3Int.zero;
            }
        }
    }
    void HandleAction(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.isPause && GameManager.Instance.startGame && interactableObject != null && !WorldManager.Instance.cantMakeActions)
        {
            LookAt(positionInGrid, interactableObject.GetObjectInteract().positionInGrid);
            interactableObject.Interact(this);
        }
    }
    public override void MoveCharacter(Vector3Int targetPosition)
    {
        if (isOnMovement) return;

        Vector3Int finalPosition = targetPosition + positionInGrid;
        WorldManager.Instance.aStarPathFinding.GetHighestBlockAt(finalPosition, out GenerateMap.WalkablePositionInfo block);
        if (block != null && block.isWalkable)
        {
            if (block.pos.y < transform.position.y + characterData.GetMovementMaxHeight())
            {
                if (characterAnimations.currentAnimation.name != "Walk")
                {
                    characterAnimations.MakeAnimation("Walk");
                }
                isOnMovement = true;
                StartCoroutine(MoveTo(WorldManager.Instance.aStarPathFinding.grid[positionInGrid], block));
                positionInGrid = block.pos;
            }
            else if (characterAnimations.currentAnimation.name == "Walk")
            {
                characterAnimations.MakeAnimation("Idle");
            }
        }
        else if (characterAnimations.currentAnimation.name == "Walk")
        {
            characterAnimations.MakeAnimation("Idle");
        }
    }
    public IEnumerator MoveTo(GenerateMap.WalkablePositionInfo initialBlock, GenerateMap.WalkablePositionInfo finalBlock)
    {
        LookAt(initialBlock.pos, finalBlock.pos);
        if (initialBlock.blockInfo.typeBlock == Block.TypeBlock.Stair || finalBlock.blockInfo.typeBlock == Block.TypeBlock.Stair)
        {
            yield return StartCoroutine(WalkInStairs(initialBlock, finalBlock));
        }
        else
        {
            if (initialBlock.pos.y != finalBlock.pos.y)
            {
                yield return StartCoroutine(JumpToPosition(initialBlock.pos, finalBlock.pos, 0.5f));
            }
            else
            {
                yield return StartCoroutine(MoveToPosition(finalBlock.pos));
            }
        }
        isOnMovement = false;
    }
    private IEnumerator MoveToPosition(Vector3Int targetPos)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, targetPos.y, targetPos.z);
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
    private IEnumerator JumpToPosition(Vector3Int from, Vector3Int to, float duration)
    {
        Vector3 startPos = new Vector3(from.x, from.y, from.z);
        Vector3 endPos = new Vector3(to.x, to.y, to.z);
        if (startPos.y > endPos.y)
        {
            Vector3 midPos = new Vector3(endPos.x, startPos.y, endPos.z);
            yield return MakeParabola(startPos, midPos, duration / 2);
            yield return GoToHightPoint(midPos, endPos, duration / 4);
        }
        else
        {
            Vector3 midPos = new Vector3(startPos.x, endPos.y, startPos.z);
            yield return GoToHightPoint(startPos, endPos, duration / 4);
            yield return MakeParabola(midPos, endPos, duration / 2);
        }
        transform.position = endPos;
    }
    public IEnumerator GoToHightPoint(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            Vector3 pos = Vector3.Lerp(startPos,
                new Vector3(startPos.x, endPos.y, startPos.z), t);
            transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator MakeParabola(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
            float parabola = 4 * 1f * t * (1 - t);
            horizontal.y += parabola;
            transform.position = horizontal;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CharacterEnemy") && !characterHitted)
        {
            characterHitted = collision.gameObject.GetComponent<CharacterBase>();
            _ = WorldManager.Instance.OnEnemyHit();
        }
        else if (collision.gameObject.CompareTag("Interactable") && !characterHitted)
        {
            interactableObject = collision.gameObject.GetComponent<IInteractable>();
            interactableObject.OnInteractEnter();
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            interactableObject.OnInteractExit();
            interactableObject = null;
        }
    }
    public interface IInteractable
    {
        void Interact(CharacterWorldPlayer character);
        void ResumeWorldAfterInteraction();
        void OnInteractEnter();
        void OnInteractExit();
        CharacterBase GetObjectInteract();
    }
}