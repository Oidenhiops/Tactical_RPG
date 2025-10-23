using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuThrowCharacter : MonoBehaviour
{
    public BattlePlayerManager playerManager;
    public GameObject menuThrowCharacter;
    public TMP_Text heightTextValue;
    public TMP_Text distanceTextValue;
    public bool isThrowingCharacter;
    public async Task EnableMenu()
    {
        await Awaitable.NextFrameAsync();
        menuThrowCharacter.SetActive(true);
        _= playerManager.menuCharacterActions.DisableMenu(true, true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
        heightTextValue.text = playerManager.aStarPathFinding.characterSelected.characterData.GetMovementMaxHeight().ToString();
        distanceTextValue.text = playerManager.aStarPathFinding.characterSelected.characterData.GetThrowRadius().ToString();
        playerManager.aStarPathFinding.EnableGrid(playerManager.aStarPathFinding.GetTilesToThrow(), playerManager.menuLiftCharacter.gridColor);
    }
    public async Task DisableMenu()
    {
        await Awaitable.NextFrameAsync();
        menuThrowCharacter.SetActive(false);
        _ = playerManager.menuCharacterActions.EnableMenu();
        playerManager.MovePointerToInstant(playerManager.aStarPathFinding.characterSelected.positionInGrid);
    }
    public async Task DisableMenuAfterThrowCharacter()
    {
        await Awaitable.NextFrameAsync();
        menuThrowCharacter.SetActive(false);
        playerManager.aStarPathFinding.characterSelected = null;
        playerManager.actionsManager.EnableMobileInputs();
    }
    public void OnHandleTrow(InputAction.CallbackContext context)
    {
        if (menuThrowCharacter.activeSelf && playerManager.aStarPathFinding.grid[playerManager.currentMousePos].hasCharacter == null && !GameManager.Instance.isPause)
        {
            if (playerManager.actionsManager.characterActions.TryGetValue(playerManager.aStarPathFinding.characterSelected, out List<ActionsManager.ActionInfo> actions))
            {
                isThrowingCharacter = false;
                StartCoroutine(ThrowCharacterToPosition(playerManager.aStarPathFinding.characterSelected.positionInGrid + Vector3.up, playerManager.currentMousePos, actions[actions.Count - 1].characterToMakeAction[0].character, 1));
            }
            else
            {
                isThrowingCharacter = false;
                StartCoroutine(ThrowCharacterToPosition(playerManager.aStarPathFinding.characterSelected.positionInGrid + Vector3.up, playerManager.currentMousePos, playerManager.aStarPathFinding.characterSelected.transform.GetChild(1).GetComponent<CharacterBase>(), 1));
            }
        }
    }
    IEnumerator ThrowCharacterToPosition(Vector3 from, Vector3 to, CharacterBase character, float duration)
    {        
        playerManager.aStarPathFinding.DisableGrid();
        playerManager.aStarPathFinding.characterSelected.characterAnimations.MakeAnimation("Throw");
        while (true)
        {
            if (playerManager.aStarPathFinding.characterSelected.characterAnimations.currentAnimation.name == "Throw" &&
                playerManager.aStarPathFinding.characterSelected.characterAnimations.currentSpriteIndex == 
                playerManager.aStarPathFinding.characterSelected.characterAnimations.currentAnimation.frameToInstance)
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
        playerManager.aStarPathFinding.characterSelected.lastAction = ActionsManager.TypeAction.EndTurn;
        if (playerManager.actionsManager.characterActions.TryGetValue(playerManager.aStarPathFinding.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions[actions.Count - 1].characterToMakeAction[0].character.startPositionInGrid = Vector3Int.RoundToInt(endPos);
            actions[actions.Count - 1].characterToMakeAction[0].character.positionInGrid = Vector3Int.RoundToInt(endPos);
            playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(endPos)].hasCharacter = actions[actions.Count - 1].characterToMakeAction[0].character;
            actions[actions.Count - 1].characterToMakeAction[0].character.transform.SetParent(playerManager.aStarPathFinding.characterSelected.transform.parent);
        }
        else
        {
            if (playerManager.aStarPathFinding.characterSelected.transform.GetChild(1).TryGetComponent(out CharacterBase component))
            {
                component.startPositionInGrid = Vector3Int.RoundToInt(endPos);
                component.positionInGrid = Vector3Int.RoundToInt(endPos);
                playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(endPos)].hasCharacter = component;
                component.transform.SetParent(playerManager.aStarPathFinding.characterSelected.transform.parent);
            }
        }
        if (playerManager.actionsManager.characterFinalActions.ContainsKey(playerManager.aStarPathFinding.characterSelected))
        {
            playerManager.actionsManager.characterFinalActions[playerManager.aStarPathFinding.characterSelected] = new ActionsManager.ActionInfo()
            {
                characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                typeAction = ActionsManager.TypeAction.Lift
            };
        }
        else
        {
            playerManager.actionsManager.characterFinalActions.Add(playerManager.aStarPathFinding.characterSelected, new ActionsManager.ActionInfo()
            {
                characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                typeAction = ActionsManager.TypeAction.Lift
            });
        }
        playerManager.aStarPathFinding.characterSelected.characterStatusEffect.statusEffects.Remove(playerManager.menuLiftCharacter.statusEffectLiftSO);
        CancelCharacterActions(playerManager.aStarPathFinding.characterSelected);
        isThrowingCharacter = false;
        yield return null;
        _= DisableMenuAfterThrowCharacter();
    }
    void CancelCharacterActions(CharacterBase character)
    {
        if (BattlePlayerManager.Instance.actionsManager.characterActions.TryGetValue(character, out List<ActionsManager.ActionInfo> actions))
        {
            if (actions[actions.Count - 1].characterToMakeAction != null && actions[actions.Count - 1].characterToMakeAction.Count > 0)
            {
                if (BattlePlayerManager.Instance.actionsManager.characterActions.TryGetValue(actions[actions.Count - 1].characterToMakeAction[0].character, out List<ActionsManager.ActionInfo> otherActions))
                {
                    otherActions[otherActions.Count - 1].cantUndo = true;
                }
            }
            if (actions[actions.Count - 1].typeAction == ActionsManager.TypeAction.Lift)
            {
                actions[actions.Count - 1].cantUndo = true;
                actions[actions.Count - 1].characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>();
            }
            else
            {
                BattlePlayerManager.Instance.actionsManager.characterActions[character].Add(new ActionsManager.ActionInfo
                {
                    cantUndo = true,
                    characterMakeAction = character,
                    typeAction = ActionsManager.TypeAction.EndTurn,
                    positionInGrid = Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)
                });
            }
        }
    }
}