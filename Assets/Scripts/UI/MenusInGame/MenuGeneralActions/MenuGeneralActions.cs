using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuGeneralActions : MonoBehaviour
{
    public BattlePlayerManager playerManager;
    public GameObject menuGeneralActions;
    public Button charactersButton;
    public Button executeButton;
    public Button endTurnButton;
    public async Awaitable EnableMenu()
    {
        try
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            await Awaitable.NextFrameAsync();
            playerManager.actionsManager.DisableMobileInputs();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(GetSelectedButton());
            playerManager.aStarPathFinding.DisableGrid();
            menuGeneralActions.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void BackToMenuWhitButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
    public async Awaitable DisableMenu()
    {
        try
        {
            await Awaitable.NextFrameAsync();
            playerManager.actionsManager.EnableMobileInputs();
            if (playerManager.aStarPathFinding.characterSelected && playerManager.aStarPathFinding.LastCharacterActionPermitActions()) playerManager.aStarPathFinding.EnableGrid(playerManager.aStarPathFinding.GetWalkableTiles(playerManager.aStarPathFinding.characterSelected), Color.magenta);
            executeButton.interactable = false;
            endTurnButton.interactable = false;
            menuGeneralActions.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public GameObject GetSelectedButton()
    {
        playerManager.actionsManager.ActionForExecuteExist(out bool actionExist);
        if (actionExist)
        {
            executeButton.interactable = true;
            endTurnButton.interactable = true;
            return executeButton.gameObject;
        }
        executeButton.interactable = false;
        if (AnyCharacterIsActive())
        {
            endTurnButton.interactable = true;
            return endTurnButton.gameObject;
        }
        else endTurnButton.interactable = false;
        return charactersButton.gameObject;
    }

    private bool AnyCharacterIsActive()
    {
        foreach (var character in playerManager.characters)
        {
            if (character.gameObject.activeSelf) return true;
        }
        return false;
    }
    public void ExecuteButton()
    {
        if (!GameManager.Instance.isPause)
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            _ = ExecuteAction();
        }
    }
    public async Awaitable ExecuteAction()
    {
        try
        {
            await Awaitable.NextFrameAsync();
            menuGeneralActions.SetActive(false);
            BattlePlayerManager.Instance.canShowGridAndDecal = false;
            BattlePlayerManager.Instance.characterPlayerMakingActions = true;
            BattlePlayerManager.Instance.DisableVisuals();
            await BattlePlayerManager.Instance.actionsManager.MakeActions();
            BattlePlayerManager.Instance.canShowGridAndDecal = true;
            BattlePlayerManager.Instance.characterPlayerMakingActions = false;
            _ = EnableMenu();
            BattlePlayerManager.Instance.mouseDecal.decal.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void EndTurnButton()
    {
        if (!GameManager.Instance.isPause)
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            BattlePlayerManager.Instance.canShowGridAndDecal = false;
            BattlePlayerManager.Instance.DisableVisuals();
            menuGeneralActions.SetActive(false);
            _ = BattlePlayerManager.Instance.actionsManager.EndTurn();
        }
    }
    public void CharactersButton()
    {
        if (!GameManager.Instance.isPause) _ = BattlePlayerManager.Instance.menuAllCharacters.EnableMenu();
    }
    public void BonusButton()
    {
        if (!GameManager.Instance.isPause) print("bonus");
    }
}
