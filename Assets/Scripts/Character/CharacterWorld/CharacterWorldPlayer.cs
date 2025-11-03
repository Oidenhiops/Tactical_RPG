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
    private IEnumerator WalkInStairs(GenerateMap.WalkablePositionInfo from, GenerateMap.WalkablePositionInfo to)
    {
        Vector3 startPos = new Vector3(from.pos.x, from.pos.y, from.pos.z);
        Vector3 endPos = new Vector3(to.pos.x, to.pos.y, to.pos.z);
        float duration = 0.2f;
        float elapsed = 0f;
        if (from.blockInfo.typeBlock == Block.TypeBlock.Stair && to.blockInfo.typeBlock == Block.TypeBlock.Stair)
        {
            startPos.y -= 0.3f;
            endPos.y -= 0.3f;
        }
        else if (to.blockInfo.typeBlock == Block.TypeBlock.Stair)
        {
            endPos.y -= 0.3f;
        }
        else
        {
            startPos.y -= 0.3f;
        }

        if (from.blockInfo.typeBlock != to.blockInfo.typeBlock)
        {
            Vector3 midPoint = (startPos + endPos) / 2f;
            if (from.pos.y == to.pos.y)
            {
                midPoint.y = Mathf.RoundToInt(midPoint.y);
            }
            else
            {
                if (from.blockInfo.transform.rotation.y == 0 && to.blockInfo.transform.rotation.y == 0)
                {
                    Vector3Int moveDir = Vector3Int.RoundToInt(to.blockInfo.transform.position - from.blockInfo.transform.position);
                    float dot = Vector3.Dot(moveDir, transform.forward);
                    if (dot > 0.5f)
                    {
                        midPoint.y = 0;
                    }
                    else
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = endPos.y;
                        }
                        else
                        {
                            midPoint.y = startPos.y;
                        }
                    }
                }
                else if (from.blockInfo.transform.rotation.y != 0 && to.blockInfo.transform.rotation.y == 0)
                {
                    Vector3 localToPos = from.blockInfo.transform.InverseTransformPoint(to.blockInfo.transform.position);
                    if (Mathf.Abs(localToPos.z) > Mathf.Abs(localToPos.x))
                    {
                        midPoint.y = 0;
                    }
                    else
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = endPos.y;
                        }
                        else
                        {
                            midPoint.y = startPos.y;
                        }
                    }
                }
                else if (from.blockInfo.transform.rotation.y == 0 && to.blockInfo.transform.rotation.y != 0)
                {
                    Vector3 moveDir = (to.blockInfo.transform.position - from.blockInfo.transform.position).normalized; 
                    Vector3 forward = to.blockInfo.transform.forward; Vector3 right = to.blockInfo.transform.right;
                    float forwardDot = Vector3.Dot(moveDir, forward);
                    float rightDot = Vector3.Dot(moveDir, right); 
                    if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
                    {
                        midPoint.y = 0;
                    }
                    else
                    {
                        if (from.blockInfo.typeBlock == Block.TypeBlock.Block)
                        {
                            midPoint.y = endPos.y;
                        }
                        else
                        {
                            midPoint.y = startPos.y;
                        }
                    }
                }
            }

            float halfDuration = duration / 2f;
            while (elapsed < halfDuration)
            {
                float t = elapsed / halfDuration;
                transform.position = Vector3.Lerp(startPos, midPoint, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = midPoint;
            float elapsed2 = 0f;
            while (elapsed2 < halfDuration)
            {
                float t = elapsed2 / halfDuration;  
                transform.position = Vector3.Lerp(midPoint, endPos, t);
                elapsed2 += Time.deltaTime;
                yield return null;
            }
            transform.position = endPos;
        }
        else
        {
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 pos = Vector3.Lerp(startPos, endPos, t);
                transform.position = pos;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
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