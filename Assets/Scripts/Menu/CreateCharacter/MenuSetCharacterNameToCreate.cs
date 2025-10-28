using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuSetCharacterNameToCreate : MonoBehaviour
{
    public SerializedDictionary<Button, TMP_Text> characterNameButtons = new SerializedDictionary<Button, TMP_Text>();
    public TMP_InputField nameLabel;
    public MenuSelectCharacterToCreate menuSelectCharacterToCreate;
    public CharacterBase characterView;
    public InputAction backAction;
    public bool isMenuActive;
    public async Awaitable EnableMenu()
    {
        try
        {
            characterView.initialDataSO = menuSelectCharacterToCreate.characterSelected;
            await characterView.InitializeCharacter();
            backAction.Enable();
            backAction.performed += OnHandleBack;
            menuSelectCharacterToCreate.menuCharacterInfo.isMenuActive = false;
            menuSelectCharacterToCreate.isMenuActive = false;
            menuSelectCharacterToCreate.container.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(characterNameButtons.ElementAt(0).Key.gameObject);
            isMenuActive = true;
            gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void OnHandleBack(InputAction.CallbackContext context)
    {
        if (isMenuActive) _ = DisableMenu();
    }
    async Awaitable DisableMenu()
    {
        try
        {
            if (isMenuActive)
            {
                isMenuActive = false;
                backAction.Disable();
                backAction.performed -= OnHandleBack;
                nameLabel.text = "";
                await Awaitable.NextFrameAsync();
                menuSelectCharacterToCreate.menuCharacterInfo.isMenuActive = true;
                menuSelectCharacterToCreate.isMenuActive = true;
                menuSelectCharacterToCreate.container.SetActive(true);
                gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void OnButtonSelect(Button buttonSelected)
    {
        nameLabel.text += characterNameButtons[buttonSelected].text;
    }
    public void OnButtonDelete()
    {
        if (nameLabel.text.Length > 0)
        {
            nameLabel.text = nameLabel.text.Remove(nameLabel.text.Length - 1);
        }
    }
    public void OnButtonRandom()
    {
        nameLabel.text = GameData.Instance.charactersDataDBSO.GenerateFantasyName();
    }
    public void OnButtonAccept()
    {
        if (nameLabel.text != "")
        {
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].isUse = true;
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].principalCharacterName = nameLabel.text;
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].currentZone = GameManager.TypeScene.CityScene;
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].lastSaveDate = GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].createdDate;
            GameData.Instance.SetStartingItems();
            CharacterData character = new CharacterData
            {
                id = menuSelectCharacterToCreate.characterSelected.id,
                subId = menuSelectCharacterToCreate.characterSelected.subId,
                name = nameLabel.text,
                level = 1,
                mastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
            };
            character.statistics = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneStatistics();
            character.mastery = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneMastery();
            character.skills = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneSkills();
            foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in character.statistics)
            {
                if (statistic.Key != CharacterData.TypeStatistic.Exp)
                {
                    statistic.Value.RefreshValue();
                    statistic.Value.SetMaxValue();
                }
                else
                {
                    statistic.Value.baseValue = 15;
                    statistic.Value.RefreshValue();
                }
            }

            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters.Add(character.name, character);

            foreach (var companion in GameData.Instance.charactersDataDBSO.companionsCharacters)
            {
                CharacterData companionCharacter = new CharacterData
                {
                    id = companion.initialDataSO.id,
                    subId = companion.initialDataSO.subId,
                    name = GameData.Instance.charactersDataDBSO.GenerateFantasyName(),
                    level = 1,
                    mastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
                };
                companionCharacter.statistics = companion.initialDataSO.CloneStatistics();
                companionCharacter.mastery = companion.initialDataSO.CloneMastery();
                companionCharacter.skills = companion.initialDataSO.CloneSkills();
                foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in companionCharacter.statistics)
                {
                    if (statistic.Key != CharacterData.TypeStatistic.Exp)
                    {
                        statistic.Value.RefreshValue();
                        statistic.Value.SetMaxValue();
                    }
                    else
                    {
                        statistic.Value.baseValue = 15;
                        statistic.Value.RefreshValue();
                    }
                }
                GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].characters.Add(companionCharacter.name, companionCharacter);
            }
            _ = DisableMenuAfterSetName();
        }
    }
    public async Awaitable DisableMenuAfterSetName()
    {
        try
        {
            if (isMenuActive)
            {
                backAction.Disable();
                backAction.performed -= OnHandleBack;
                nameLabel.text = "";
                GameData.Instance.SaveGameData();
                GameData.Instance.LoadGameDataInfo();
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(menuSelectCharacterToCreate.lastButtonSelected);
                gameObject.SetActive(false);
                menuSelectCharacterToCreate.container.SetActive(true);
                menuSelectCharacterToCreate.gameObject.SetActive(false);
                menuSelectCharacterToCreate.otherMenu.SetActive(true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
