using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterWorldPlayer : CharacterBase
{
    public Vector3Int movementDirection;
    public bool isOnMovement;
    public Vector3 detectorOffset;
    public Vector3 detectorSize = Vector3.one;
    public CharacterBase characterHitted;
    void Start()
    {
        WorldManager.Instance.characterActions.CharacterInputs.Movement.started += HandleMovement;
        WorldManager.Instance.characterActions.CharacterInputs.Movement.canceled += HandleMovement;
    }
    void OnDestroy()
    {
        WorldManager.Instance.characterActions.CharacterInputs.Movement.started -= HandleMovement;
        WorldManager.Instance.characterActions.CharacterInputs.Movement.canceled -= HandleMovement;
    }
    void Update()
    {
        if (!WorldManager.Instance.enemyHitted && !GameManager.Instance.isPause && GameManager.Instance.startGame)
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
            if (EnemyHit())
            {
                _ = WorldManager.Instance.OnEnemyHit();
            }
        }
    }
    void HandleMovement(InputAction.CallbackContext context)
    {
        if (!WorldManager.Instance.enemyHitted && !GameManager.Instance.isPause && GameManager.Instance.startGame && !isOnMovement)
        {
            if (!context.performed)
            {
                movementDirection = Vector3Int.zero;
            }
        }
    }
    void HandleAction(InputAction.CallbackContext context)
    {
        if (!WorldManager.Instance.enemyHitted && !GameManager.Instance.isPause && GameManager.Instance.startGame)
        {
            
        }
    }
    bool EnemyHit()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + detectorOffset, detectorSize / 2, Quaternion.identity, LayerMask.GetMask("CharacterEnemy"));

        foreach (var col in colliders)
        {
            if (col.transform != transform)
            {
                characterHitted = col.GetComponent<CharacterBase>();
                return true;
            }
        }
        return false;
    }
    public override void MoveCharacter(Vector3Int targetPosition)
    {
        if (isOnMovement) return;

        Vector3Int finalPosition = targetPosition + positionInGrid;
        WorldManager.Instance.aStarPathFinding.GetHighestBlockAt(finalPosition, out GenerateMap.WalkablePositionInfo block);
        if (block != null)
        {
            if (block.pos.y < transform.position.y + characterData.GetMovementMaxHeight())
            {
                if (characterAnimations.currentAnimation.name != "Walk")
                {
                    characterAnimations.MakeAnimation("Walk");
                }
                isOnMovement = true;
                StartCoroutine(MoveTo(positionInGrid, block.pos));
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
    public IEnumerator MoveTo(Vector3Int initialPos, Vector3Int finalPos)
    {
        if (initialPos.x == finalPos.x)
        {
            nextDirection.x = initialPos.z < finalPos.z ? 1 : -1;
        }
        else
        {
            nextDirection.x = initialPos.x < finalPos.x ? -1 : 1;
        }
        if (initialPos.z == finalPos.z)
        {
            nextDirection.z = initialPos.x < finalPos.x ? 1 : -1;
        }
        else
        {
            nextDirection.z = initialPos.z < finalPos.z ? 1 : -1;
        }
        if (initialPos.y != finalPos.y)
        {
            yield return StartCoroutine(JumpToPosition(initialPos, finalPos, 0.5f));
        }
        else
        {
            yield return StartCoroutine(MoveToPosition(finalPos));
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
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + detectorOffset, detectorSize);
    }
}
