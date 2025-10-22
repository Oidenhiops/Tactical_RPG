using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async Task EnableMenu()
    {
        playerManager.actionsManager.DisableMobileInputs();
        if (!playerManager.AnyMenuIsActive())
        {
            _= playerManager.menuCharacterInfo.ReloadInfo(playerManager.aStarPathFinding.characterSelected);
            buttonsExepts = new List<TypeButton>();
            EventSystem.current.SetSelectedGameObject(null);
            playerManager.aStarPathFinding.DisableGrid();
            if (playerManager.aStarPathFinding.characterSelected.lastAction != ActionsManager.TypeAction.EndTurn)
            {
                switch (playerManager.aStarPathFinding.characterSelected.lastAction)
                {
                    case ActionsManager.TypeAction.Attack:
                    case ActionsManager.TypeAction.Skill:
                    case ActionsManager.TypeAction.Defend:
                    case ActionsManager.TypeAction.Item:
                        break;
                    case ActionsManager.TypeAction.Lift:
                        buttons[TypeButton.Lift].gameObject.SetActive(false);
                        buttons[TypeButton.Throw].gameObject.SetActive(true);
                        buttonsExepts.Add(TypeButton.Throw);
                        break;
                    default:
                        for (int i = 1; i < Enum.GetValues(typeof(TypeButton)).Length; i++)
                        {
                            if ((TypeButton)i != TypeButton.Lift && (TypeButton)i != TypeButton.Attack && (TypeButton)i != TypeButton.Skill)
                            {
                                buttonsExepts.Add((TypeButton)i);
                            }
                            else if ((TypeButton)i == TypeButton.Attack)
                            {
                                if (playerManager.aStarPathFinding.GetPositionsToAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions))
                                {
                                    buttonsExepts.Add(TypeButton.Attack);
                                    SendCharactersToAttack(positions);
                                }
                            }
                            else if ((TypeButton)i == TypeButton.Skill)
                            {
                                if (playerManager.aStarPathFinding.characterSelected.characterData.skills.Count > 0)
                                {
                                    buttons[TypeButton.Skill].interactable = true;
                                    buttonsExepts.Add(TypeButton.Skill);
                                }
                                else
                                {
                                    buttons[TypeButton.Skill].interactable = false;
                                }
                            }
                            else if ((TypeButton)i == TypeButton.Lift)
                            {
                                buttons[TypeButton.Lift].gameObject.SetActive(true);
                                buttons[TypeButton.Throw].gameObject.SetActive(false);
                                if (playerManager.aStarPathFinding.GetPositionsToLift(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions))
                                {
                                    buttonsExepts.Add(TypeButton.Lift);
                                    SendCharactersToLift(positions);
                                }
                            }
                        }
                        break;
                }
            }
            DisableButtonsExept();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(GetFirtsButtonActive());
            menuCharacterActions.SetActive(true);
            await Awaitable.NextFrameAsync();
            isMenuActive = true;
        }
    }
    public void SendCharactersToLift(SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> data)
    {
        List<CharacterBase> characters = new List<CharacterBase>();
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> position in data)
        {
            characters.Add(position.Value.hasCharacter);
        }
        playerManager.menuLiftCharacter.characters = characters.ToArray();
        playerManager.menuLiftCharacter.positionsToLift = data;
    }
    public void SendCharactersToAttack(SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> data)
    {
        List<CharacterBase> characters = new List<CharacterBase>();
        foreach (KeyValuePair<Vector3Int, GenerateMap.WalkablePositionInfo> position in data)
        {
            characters.Add(position.Value.hasCharacter);
        }
        playerManager.menuAttackCharacter.characters = characters.ToArray();
        playerManager.menuAttackCharacter.positionsToAttack = data;
    }
    public void BackToMenuWhitButton(TypeButton typeButton)
    {
        if (playerManager.aStarPathFinding.GetPositionsToAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions))
        {
            buttonsExepts.Add(TypeButton.Attack);
            SendCharactersToAttack(positions);
        }
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttons[typeButton].gameObject);
        menuCharacterActions.SetActive(true);
        _= playerManager.menuCharacterInfo.ReloadInfo(playerManager.aStarPathFinding.characterSelected);
        isMenuActive = true;
    }
    public async Task DisableMenu(bool conservSelectedCharacter = false, bool conservCharacterInfo = false)
    {
        await Awaitable.NextFrameAsync();
        menuCharacterActions.SetActive(false);
        playerManager.menuAttackCharacter.positionsToAttack = new SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo>();
        playerManager.menuAttackCharacter.characters = new CharacterBase[0];
        if (!conservSelectedCharacter) playerManager.aStarPathFinding.characterSelected = null;
        if (!conservCharacterInfo)
        {
            playerManager.actionsManager.EnableMobileInputs();
            playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
        }
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
        if (isMenuActive && !GameManager.Instance.isPause) _= playerManager.menuAttackCharacter.EnableMenu();
    }
    public void HandleSpecial()
    {
        if (isMenuActive && !GameManager.Instance.isPause) _= playerManager.menuSkillsCharacter.EnableMenu();
    }
    public void HandleDefend()
    {
        if (isMenuActive && !GameManager.Instance.isPause)
        {
            playerManager.aStarPathFinding.characterSelected.lastAction = ActionsManager.TypeAction.Defend;
            if (playerManager.actionsManager.characterActions.TryGetValue(playerManager.aStarPathFinding.characterSelected, out List<ActionsManager.ActionInfo> actions))
            {
                actions.Add(new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Defend,
                });
                playerManager.actionsManager.characterFinalActions.Add(playerManager.aStarPathFinding.characterSelected, new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Defend,
                });
            }
            else
            {
                playerManager.actionsManager.characterActions.Add(playerManager.aStarPathFinding.characterSelected, new List<ActionsManager.ActionInfo>
                {
                    {
                        new ActionsManager.ActionInfo
                        {
                        characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                        typeAction = ActionsManager.TypeAction.Defend
                        }
                    }
                });
                playerManager.actionsManager.characterFinalActions.Add(playerManager.aStarPathFinding.characterSelected, new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Defend,
                });
            }
            _= DisableMenu();
        }
    }
    public void HandleLift()
    {
        if (isMenuActive && !GameManager.Instance.isPause) _= playerManager.menuLiftCharacter.EnableMenu();
    }
    public void HandleThrow()
    {
        if (isMenuActive && !GameManager.Instance.isPause) _= playerManager.menuThrowCharacter.EnableMenu();
    }
    public void HandleItem()
    {
        if (isMenuActive && !GameManager.Instance.isPause) _= playerManager.menuItemsCharacter.EnableMenu();
    }
    public enum TypeButton
    {
        None = 0,
        Attack = 1,
        Skill = 2,
        Defend = 3,
        Lift = 4,
        Throw = 5,
        Item = 6,
    }
}
