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
    public void EnableMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(GetSelectedButton());
        AStarPathFinding.Instance.DisableGrid();
        menuGeneralActions.SetActive(true);
    }
    public void BackToMenuWhitButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
    public void DisableMenu()
    {
        if (menuGeneralActions.activeSelf)
        {
            if (AStarPathFinding.Instance.characterSelected && AStarPathFinding.Instance.LastCharacterActionPermitActions()) AStarPathFinding.Instance.EnableGrid(AStarPathFinding.Instance.GetWalkableTiles(), Color.magenta);
            executeButton.interactable = false;
            menuGeneralActions.SetActive(false);
        }
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
        _ = ExecuteAction();
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
        EnableMenu();
        PlayerManager.Instance.mouseDecal.decal.gameObject.SetActive(true);
    }
    public void EndTurnButton()
    {
        PlayerManager.Instance.canShowGridAndDecal = false;
        PlayerManager.Instance.DisableVisuals();
        menuGeneralActions.SetActive(false);
        _ = PlayerManager.Instance.actionsManager.EndTurn();
    }
    public void CharactersButton()
    {
        StartCoroutine(PlayerManager.Instance.menuAllCharacters.EnableMenu());
    }
    public void BonusButton()
    {
        
    }
}
