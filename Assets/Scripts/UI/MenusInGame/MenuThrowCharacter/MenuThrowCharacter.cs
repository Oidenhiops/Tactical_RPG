using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuThrowCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameObject menuThrowCharacter;
    public bool isThrowingCharacter;
    void LateUpdate()
    {
        if (menuThrowCharacter.activeSelf)
        {
            Vector3Int pos = Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position);
            {
                if (Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position).x == pos.x)
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.x = Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position).z < pos.z ? 1 : -1;
                }
                else
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.x = Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position).x < pos.x ? -1 : 1;
                }
                if (Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position).z == pos.z)
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.z = Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position).x < pos.x ? 1 : -1;
                }
                else
                {
                    AStarPathFinding.Instance.characterSelected.nextDirection.z = Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position).z < pos.z ? 1 : -1;
                }
            }
        }
    }
    public IEnumerator EnableMenu()
    {
        menuThrowCharacter.SetActive(true);
        playerManager.menuCharacterActions.DisableMenu(true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
        AStarPathFinding.Instance.EnableGrid(AStarPathFinding.Instance.GetTilesToThrow());
        yield return null;
    }
    public void DisableMenuBack()
    {
        menuThrowCharacter.SetActive(false);
        StartCoroutine(playerManager.menuCharacterActions.EnableMenu());
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position));
    }
    public void DisableMenuActive()
    {
        menuThrowCharacter.SetActive(false);
        AStarPathFinding.Instance.characterSelected = null;
    }
    public void OnHandleTrow(InputAction.CallbackContext context)
    {
        if (menuThrowCharacter.activeSelf && AStarPathFinding.Instance.grid[playerManager.currentMousePos].hasCharacter == null)
        {
            if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
            {
                isThrowingCharacter = false;
                StartCoroutine(ThrowCharacterToPosition(AStarPathFinding.Instance.characterSelected.transform.position + Vector3.up, playerManager.currentMousePos, actions[actions.Count - 1].otherCharacterInfo[0].character, 1));
            }
        }
    }
    IEnumerator ThrowCharacterToPosition(Vector3 from, Vector3 to, Character character, float duration)
    {        
        AStarPathFinding.Instance.DisableGrid();
        AStarPathFinding.Instance.characterSelected.characterAnimations.MakeAnimation("Throw");
        while (true)
        {
            if (AStarPathFinding.Instance.characterSelected.characterAnimations.currentAnimation.name == "Throw" &&
                AStarPathFinding.Instance.characterSelected.characterAnimations.currentSpriteIndex == 
                AStarPathFinding.Instance.characterSelected.characterAnimations.currentAnimation.frameToInstance)
            {
                break;
            }
            yield return null;
        }
        Vector3 startPos = new Vector3(from.x, from.y, from.z);
        Vector3 endPos = new Vector3(to.x, to.y, to.z);
        float elapsed = 0f;
        float height = Mathf.Abs(to.y - from.y) * 0.5f + 1 + Mathf.Abs(from.y - to.y);
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
            float baseY = Mathf.Lerp(startPos.y, endPos.y, t);
            float parabola = 4 * height * t * (1 - t);
            horizontal.y = baseY + parabola;
            character.transform.position = horizontal;
            elapsed += Time.deltaTime;
            yield return null;
        }
        character.transform.position = endPos;
        if (character.characterAnimations.currentAnimation.name != "Lift")
        {
            character.characterAnimations.MakeAnimation("Idle");
        }
        AStarPathFinding.Instance.characterSelected.lastAction = ActionsManager.TypeAction.EndTurn;
        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions[actions.Count - 1].otherCharacterInfo[0].character.startPositionInGrid = Vector3Int.RoundToInt(endPos);
            actions[actions.Count - 1].otherCharacterInfo[0].character.positionInGrid = Vector3Int.RoundToInt(endPos);
            AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(endPos)].hasCharacter = actions[actions.Count - 1].otherCharacterInfo[0].character;
            actions[actions.Count - 1].otherCharacterInfo[0].character.transform.SetParent(null);
        }
        CancelCharacterActions(AStarPathFinding.Instance.characterSelected);
        isThrowingCharacter = false;
        DisableMenuActive();
    }
    void CancelCharacterActions(Character character)
    {
        if (PlayerManager.Instance.actionsManager.characterActions.TryGetValue(character, out List<ActionsManager.ActionInfo> actions))
        {
            if (actions[actions.Count - 1].otherCharacterInfo != null && actions[actions.Count - 1].otherCharacterInfo.Count > 0)
            {
                if (PlayerManager.Instance.actionsManager.characterActions.TryGetValue(actions[actions.Count - 1].otherCharacterInfo[0].character, out List<ActionsManager.ActionInfo> otherActions))
                {
                    otherActions[otherActions.Count - 1].cantUndo = true;
                }
            }
            if (actions[actions.Count - 1].typeAction == ActionsManager.TypeAction.Lift)
            {
                actions[actions.Count - 1].cantUndo = true;
                actions[actions.Count - 1].otherCharacterInfo = new List<ActionsManager.OtherCharacterInfo>();
            }
            else
            {
                PlayerManager.Instance.actionsManager.characterActions[character].Add(new ActionsManager.ActionInfo
                {
                    cantUndo = true,
                    character = character,
                    typeAction = ActionsManager.TypeAction.EndTurn,
                    positionInGrid = Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)
                });
            }
        }
    }
}