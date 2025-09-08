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
    void Start()
    {
        playerManager.characterActions.CharacterInputs.ActiveGeneralActions.performed += OnActiveMenu;
    }
    public void OnActiveMenu(InputAction.CallbackContext context)
    {
        if (playerManager.actionsManager.isPlayerTurn && !menuGeneralActions.activeSelf && !playerManager.menuCharacterActions.menuCharacterActions.activeSelf && !playerManager.menuCharacterSelector.menuCharacterSelector.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(GetSelectedButton());
            AStarPathFinding.Instance.DisableGrid();
            menuGeneralActions.SetActive(true);
        }
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
            if (AStarPathFinding.Instance.characterSelected && AStarPathFinding.Instance.LastCharacterActionPermitActions()) AStarPathFinding.Instance.EnableGrid(AStarPathFinding.Instance.GetWalkableTiles());
            executeButton.interactable = false;
            menuGeneralActions.SetActive(false);
        }
    }
    public GameObject GetSelectedButton()
    {
        playerManager.actionsManager.AttackOrSpecialActionExist(out bool attackActionExist, out bool specialActionExist);
        if (attackActionExist || specialActionExist)
        {
            executeButton.interactable = true;
            return executeButton.gameObject;
        }
        return endTurnButton;
    }
    public void ExecuteButton()
    {
        _ = ExecuteAction();
    }
    public async Task ExecuteAction()
    {
        PlayerManager.Instance.canShowGridAndDecal = false;
        PlayerManager.Instance.characterPlayerMakingActions = true;
        PlayerManager.Instance.DisableVisuals();
        await PlayerManager.Instance.actionsManager.MakeActions();
        PlayerManager.Instance.canShowGridAndDecal = true;
        PlayerManager.Instance.characterPlayerMakingActions = false;
        PlayerManager.Instance.EnableVisuals();
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
