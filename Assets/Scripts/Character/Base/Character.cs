using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool isInitialize;
    public bool isCharacterPlayer;
    public TypeCharacter typeCharacter;
    public InitialDataSO initialDataSO;
    public CharacterModel characterModel;
    public CharacterData characterData;
    public Vector3Int direction = new Vector3Int();
    public Vector3Int nextDirection;
    public CharacterAnimation characterAnimations;
    public Vector3Int positionInGrid;
    public Vector3Int startPositionInGrid;
    public ActionsManager.TypeAction lastAction;
    public void OnEnable()
    {
        if (isInitialize) characterAnimations.MakeAnimation("Idle");
    }
    void LateUpdate()
    {
        CameraInfo.Instance.CamDirection(out Vector3 camForward, out Vector3 camRight);
        Vector3 camRelativeDir = (nextDirection.x * camRight + nextDirection.z * camForward).normalized;
        Vector3 movementDirection = new Vector3(camRelativeDir.x, 0, camRelativeDir.z).normalized;
        direction = new Vector3Int(Mathf.RoundToInt(movementDirection.x), Mathf.RoundToInt(movementDirection.y), Mathf.RoundToInt(movementDirection.z));
        ChangeDirectionModel();
    }
    public async Awaitable InitializeCharacter()
    {
        try
        {
            await InitializeCharacterData();
            await InitializeAnimations();
            isInitialize = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    async Awaitable InitializeCharacterData()
    {
        initialDataSO = GameData.Instance.charactersDataDBSO.data[characterData.id];
        await Awaitable.NextFrameAsync();
    }
    async Awaitable InitializeAnimations()
    {
        try
        {
            characterAnimations.SetInitialData(ref initialDataSO);
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    public void MoveCharacter(Vector3Int targetPosition)
    {
        List<Vector3Int> path = AStarPathFinding.Instance.FindPath(positionInGrid, targetPosition);

        if (path != null && path.Count > 0)
        {
            AStarPathFinding.Instance.grid[path[0]].hasCharacter = null;
            if (isCharacterPlayer) PlayerManager.Instance.characterPlayerMakingActions = true;
            AStarPathFinding.Instance.grid[targetPosition].hasCharacter = this;
            StartCoroutine(FollowPath(path));
        }
    }
    private IEnumerator FollowPath(List<Vector3Int> path)
    {
        positionInGrid = path[path.Count - 1];
        nextDirection = Vector3Int.zero;
        characterAnimations.MakeAnimation("Walk");
        for (int i = 1; i < path.Count; i++)
        {
            if (path[i - 1].x == path[i].x)
            {
                nextDirection.x = path[i - 1].z < path[i].z ? 1 : -1;
            }
            else
            {
                nextDirection.x = path[i - 1].x < path[i].x ? -1 : 1;
            }
            if (path[i - 1].z == path[i].z)
            {
                nextDirection.z = path[i - 1].x < path[i].x ? 1 : -1;
            }
            else
            {
                nextDirection.z = path[i - 1].z < path[i].z ? 1 : -1;
            }
            if (path[i - 1].y != path[i].y)
            {
                yield return StartCoroutine(JumpToPosition(path[i - 1], path[i], 0.5f));
            }
            else
            {
                yield return StartCoroutine(MoveToPosition(path[i]));
            }
        }
        if (isCharacterPlayer)
        {
            PlayerManager.Instance.characterPlayerMakingActions = false;
            if (positionInGrid == Vector3Int.zero)
            {
                gameObject.SetActive(false);
                AStarPathFinding.Instance.characterSelected = null;
                AStarPathFinding.Instance.grid[Vector3Int.zero].hasCharacter = null;
                if (PlayerManager.Instance.actionsManager.characterActions.ContainsKey(this))
                {
                    PlayerManager.Instance.actionsManager.characterActions.Remove(this);
                }
                PlayerManager.Instance.menuCharacterSelector.amountCharacters++;
                startPositionInGrid = Vector3Int.zero;
                AStarPathFinding.Instance.DisableGrid();
            }
            else
            {
                if (PlayerManager.Instance.actionsManager.characterActions.TryGetValue(this, out List<ActionsManager.ActionInfo> actions))
                {
                    actions.Add(new ActionsManager.ActionInfo
                    {
                        character = this,
                        typeAction = ActionsManager.TypeAction.Move,
                        positionInGrid = path[0]
                    });
                }
                else
                {
                    PlayerManager.Instance.actionsManager.characterActions.Add(this, new List<ActionsManager.ActionInfo> { new ActionsManager.ActionInfo{
                        character = this,
                        typeAction = ActionsManager.TypeAction.Move,
                        positionInGrid = path[0]
                    } });
                }
                characterAnimations.MakeAnimation("Idle");
            }
        }
        else characterAnimations.MakeAnimation("Idle");
    }
    void ChangeDirectionModel()
    {
        characterModel.characterMeshRenderer.transform.localRotation = direction.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
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
    public async Task MakeAttack()
    {
        await Awaitable.NextFrameAsync();
    }
    public async Task MakeSpecial()
    {
        await Awaitable.NextFrameAsync();
    }
    [Serializable] public class CharacterModel
    {
        public MeshRenderer characterMeshRenderer;
        public MeshRenderer characterMeshRendererHand;
        public Transform leftHand;
        public Transform rightHand;
        public Mesh originalMesh;
    }
    public enum TypeCharacter
    {
        None = 0,
        Character = 1,
        GeoSymbol = 2,
        Chest = 3
    }
}