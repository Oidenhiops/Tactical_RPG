using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuGeneralActions : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameObject menuGeneralActions;
    public Button executeButton;
    public GameObject charactersButton;
    public GameObject endTurnButton;
    public async Task EnableMenu()
    {
        await Awaitable.NextFrameAsync();        
        playerManager.actionsManager.DisableMobileInputs();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(GetSelectedButton());
        playerManager.aStarPathFinding.DisableGrid();
        menuGeneralActions.SetActive(true);
    }
    public void BackToMenuWhitButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
    public async Task DisableMenu()
    {
        await Awaitable.NextFrameAsync();
        playerManager.actionsManager.EnableMobileInputs();
        if (playerManager.aStarPathFinding.characterSelected && playerManager.aStarPathFinding.LastCharacterActionPermitActions()) playerManager.aStarPathFinding.EnableGrid(playerManager.aStarPathFinding.GetWalkableTiles(), Color.magenta);
        executeButton.interactable = false;
        menuGeneralActions.SetActive(false);
    }
    public GameObject GetSelectedButton()
    {
        playerManager.actionsManager.ActionForExecuteExist(out bool actionExist);
        if (actionExist)
        {
            executeButton.interactable = true;
            return executeButton.gameObject;
        }
        executeButton.interactable = false;
        return endTurnButton;
    }
    public void ExecuteButton()
    {
        if (!GameManager.Instance.isPause) _ = ExecuteAction();
    }
    public async Task ExecuteAction()
    {
        await Awaitable.NextFrameAsync();
        menuGeneralActions.SetActive(false);
        PlayerManager.Instance.canShowGridAndDecal = false;
        PlayerManager.Instance.characterPlayerMakingActions = true;
        PlayerManager.Instance.DisableVisuals();
        await PlayerManager.Instance.actionsManager.MakeActions();
        PlayerManager.Instance.canShowGridAndDecal = true;
        PlayerManager.Instance.characterPlayerMakingActions = false;
        _= EnableMenu();
        PlayerManager.Instance.mouseDecal.decal.gameObject.SetActive(true);
    }
    public void EndTurnButton()
    {
        if (!GameManager.Instance.isPause)
        {
            PlayerManager.Instance.canShowGridAndDecal = false;
            PlayerManager.Instance.DisableVisuals();
            menuGeneralActions.SetActive(false);
            _ = PlayerManager.Instance.actionsManager.EndTurn();
        }
    }
    public void CharactersButton()
    {
        if (!GameManager.Instance.isPause) _= PlayerManager.Instance.menuAllCharacters.EnableMenu();
    }
    public void BonusButton()
    {
        if (!GameManager.Instance.isPause) print("bonus");
    }
}
