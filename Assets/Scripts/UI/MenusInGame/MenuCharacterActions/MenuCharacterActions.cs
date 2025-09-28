using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuCharacterActions : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameObject menuCharacterActions;
    public SerializedDictionary<TypeButton, Button> buttons = new SerializedDictionary<TypeButton, Button>();
    List<TypeButton> buttonsExepts = new List<TypeButton>();
    public bool isMenuActive;
    public IEnumerator EnableMenu()
    {
        if (!playerManager.AnyMenuIsActive())
        {
            playerManager.menuCharacterInfo.ReloadInfo(AStarPathFinding.Instance.characterSelected);
            buttonsExepts = new List<TypeButton>();
            EventSystem.current.SetSelectedGameObject(null);
            AStarPathFinding.Instance.DisableGrid();
            if (AStarPathFinding.Instance.characterSelected.lastAction != ActionsManager.TypeAction.EndTurn)
            {
                switch (AStarPathFinding.Instance.characterSelected.lastAction)
                {
                    case ActionsManager.TypeAction.Attack:
                    case ActionsManager.TypeAction.Special:
                    case ActionsManager.TypeAction.Defend:
                    case ActionsManager.TypeAction.Item:
                        buttonsExepts.Add(TypeButton.Status);
                        break;
                    case ActionsManager.TypeAction.Lift:
                        buttons[TypeButton.Lift].gameObject.SetActive(false);
                        buttons[TypeButton.Throw].gameObject.SetActive(true);
                        buttonsExepts.Add(TypeButton.Throw);
                        buttonsExepts.Add(TypeButton.Status);
                        break;
                    default:
                        for (int i = 0; i < Enum.GetValues(typeof(TypeButton)).Length; i++)
                        {
                            if ((TypeButton)i != TypeButton.Lift && (TypeButton)i != TypeButton.Attack)
                            {
                                buttonsExepts.Add((TypeButton)i);
                            }
                            else if ((TypeButton)i != TypeButton.Attack)
                            {
                                if (AStarPathFinding.Instance.characterSelected.initialDataSO.isHumanoid && AStarPathFinding.Instance.GetPositionsToAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions))
                                {
                                    buttonsExepts.Add(TypeButton.Attack);
                                    SendCharactersToAttack(positions);
                                    playerManager.menuAttackCharacter.positionsToAttack = positions;
                                }
                            }
                            else if ((TypeButton)i != TypeButton.Lift)
                            {
                                buttons[TypeButton.Lift].gameObject.SetActive(true);
                                buttons[TypeButton.Throw].gameObject.SetActive(false);
                                if (AStarPathFinding.Instance.characterSelected.initialDataSO.isHumanoid && AStarPathFinding.Instance.GetPositionsToLift(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions))
                                {
                                    buttonsExepts.Add(TypeButton.Lift);
                                    SendCharactersToLift(positions);
                                    playerManager.menuLiftCharacter.positionsToLift = positions;
                                }
                            }
                        }
                        break;
                }
            }
            else
            {
                buttonsExepts.Add(TypeButton.Status);
            }
            DisableButtonsExept();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(GetFirtsButtonActive());
            menuCharacterActions.SetActive(true);
            yield return null;
            isMenuActive = true;
        }
    }
    public void SendCharactersToLift(SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> data)
    {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> position in data)
        {
            characters.Add(position.Value.hasCharacter);
        }
        playerManager.menuLiftCharacter.characters = characters.ToArray();
    }
    public void SendCharactersToAttack(SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> data)
    {
        List<Character> characters = new List<Character>();
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> position in data)
        {
            characters.Add(position.Value.hasCharacter);
        }
        playerManager.menuAttackCharacter.characters = characters.ToArray();
    }
    public void BackToMenuWhitButton(TypeButton typeButton)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttons[typeButton].gameObject);
        menuCharacterActions.SetActive(true);
        playerManager.menuCharacterInfo.ReloadInfo(AStarPathFinding.Instance.characterSelected);
        isMenuActive = true;
    }
    public void DisableMenu(bool conservSelectedCharacter = false, bool conservCharacterInfo = false)
    {
        menuCharacterActions.SetActive(false);
        if (!conservCharacterInfo) playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);        
        if (!conservSelectedCharacter) AStarPathFinding.Instance.characterSelected = null;
        isMenuActive = false;
    }
    public GameObject GetFirtsButtonActive()
    {
        foreach (KeyValuePair<TypeButton, Button> button in buttons)
        {
            if (button.Value.interactable) return button.Value.gameObject;
        }
        return null;
    }
    public void DisableButtonsExept()
    {
        foreach (KeyValuePair<TypeButton, Button> button in buttons)
        {
            if (buttonsExepts.Contains(button.Key))
            {
                button.Value.interactable = true;
            }
            else
            {
                button.Value.interactable = false;
            }
        }
    }
    public void HandleAttack()
    {
        if (isMenuActive) StartCoroutine(playerManager.menuAttackCharacter.EnableMenu());
    }
    public void HandleSpecial()
    {
        if (isMenuActive) print("Special");
    }
    public void HandleDefend()
    {
        if (isMenuActive)
        {
            AStarPathFinding.Instance.characterSelected.lastAction = ActionsManager.TypeAction.Defend;
            if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
            {
                actions.Add(new ActionsManager.ActionInfo
                {
                    character = AStarPathFinding.Instance.characterSelected,
                    typeAction = ActionsManager.TypeAction.Defend,
                });
            }
            else
            {
                playerManager.actionsManager.characterActions.Add(AStarPathFinding.Instance.characterSelected, new List<ActionsManager.ActionInfo>
                {
                    {
                        new ActionsManager.ActionInfo
                        {
                        character = AStarPathFinding.Instance.characterSelected,
                        typeAction = ActionsManager.TypeAction.Defend
                        }
                    }
                });
            }
            DisableMenu();
        }
    }
    public void HandleLift()
    {
        if (isMenuActive) StartCoroutine(playerManager.menuLiftCharacter.EnableMenu());
    }
    public void HandleThrow()
    {
        if (isMenuActive)
        {
            StartCoroutine(playerManager.menuThrowCharacter.EnableMenu());
        }
    }
    public void HandleItem()
    {
        if (isMenuActive) StartCoroutine(playerManager.menuItemsCharacter.EnableMenu());
    }
    public void HandleStatus()
    {
        if (isMenuActive) print("Item");
    }
    public enum TypeButton
    {
        None = 0,
        Attack = 1,
        Special = 2,
        Defend = 3,
        Lift = 4,
        Throw = 5,
        Item = 6,
        Status = 7,
    }
}
