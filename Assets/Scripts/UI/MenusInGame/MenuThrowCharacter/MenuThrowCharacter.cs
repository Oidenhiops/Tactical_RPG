using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuThrowCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameObject menuThrowCharacter;
    public bool isThrowingCharacter;
    void OnDestroy()
    {

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
            StartCoroutine(ThrowCharacterToPosition(AStarPathFinding.Instance.characterSelected.transform.position + Vector3.up, playerManager.currentMousePos, playerManager.actionsManager.GetLastActionByCharacter(AStarPathFinding.Instance.characterSelected).otherCharacterInfo[0].character, 1));
        }
    }
    IEnumerator ThrowCharacterToPosition(Vector3 from, Vector3 to, Character character, float duration)
    {
        AStarPathFinding.Instance.DisableGrid();
        isThrowingCharacter = true;
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
        character.characterAnimations.MakeAnimation("Idle");
        isThrowingCharacter = false;
        AStarPathFinding.Instance.characterSelected.lastAction = ActionsManager.TypeAction.EndTurn;
        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions[actions.Count - 1].otherCharacterInfo[0].character.startPositionInGrid = Vector3Int.RoundToInt(endPos);
            actions[actions.Count - 1].otherCharacterInfo[0].character.positionInGrid = Vector3Int.RoundToInt(endPos);
            AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(endPos)].hasCharacter = actions[actions.Count - 1].otherCharacterInfo[0].character;
            if (actions[actions.Count - 1].character.startPositionInGrid != actions[actions.Count - 1].character.positionInGrid)
            {
                if (actions[actions.Count - 1].character.lastAction != ActionsManager.TypeAction.EndTurn)
                {
                    actions[actions.Count - 1].otherCharacterInfo[0].character.lastAction = ActionsManager.TypeAction.Throwing;
                }
            }
            actions[actions.Count - 1].otherCharacterInfo[0].character.transform.SetParent(null);
        }
        playerManager.actionsManager.characterActions.Remove(AStarPathFinding.Instance.characterSelected);
        DisableMenuActive();
    }
}