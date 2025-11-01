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
    public TMP_InputField nameLabel;
    public MenuSelectCharacterToCreate menuSelectCharacterToCreate;
    public CharacterBase characterView;
    public InputAction backAction;
    public bool isMenuActive;
    public Button initialButton;
    public List<TMP_Text> alphabetLettersTexts;
    public int specialCharIndex = 0;
    public SerializedDictionary<int, List<string>> specialCharButtonsSets;
    public List<TMP_Text> specialCharButtonsTexts;
    public async Awaitable EnableMenu()
    {
        try
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            characterView.initialDataSO = menuSelectCharacterToCreate.characterSelected;
            await characterView.InitializeCharacter();
            backAction.Enable();
            backAction.performed += OnHandleBack;
            menuSelectCharacterToCreate.menuCharacterInfo.isMenuActive = false;
            menuSelectCharacterToCreate.isMenuActive = false;
            menuSelectCharacterToCreate.container.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(initialButton.gameObject);
            isMenuActive = true;
            specialCharIndex = 0;
            for (int i = 0; i < specialCharButtonsTexts.Count; i++)
            {
                specialCharButtonsTexts[i].text = specialCharButtonsSets[specialCharIndex][i];
            }
            if (alphabetLettersTexts.First().text == alphabetLettersTexts.First().text.ToUpper()) ToUpperOrLowercase();
            await Awaitable.NextFrameAsync();
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
                AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonBack"), 1, false);
                await Awaitable.NextFrameAsync();
                gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void ToUpperOrLowercase()
    {
        bool isUpper = alphabetLettersTexts.First().text == alphabetLettersTexts.First().text.ToUpper();
        foreach (TMP_Text letterText in alphabetLettersTexts)
        {
            if (!isUpper) letterText.text = letterText.text.ToUpper();
            else letterText.text = letterText.text.ToLower();
        }
    }
    public void SwapSpecialChars()
    {
        specialCharIndex++;
        if (specialCharIndex >= specialCharButtonsSets.Count) specialCharIndex = 0;

        for (int i = 0; i < specialCharButtonsTexts.Count; i++)
        {
            specialCharButtonsTexts[i].text = specialCharButtonsSets[specialCharIndex][i];
        }
    }
    public void OnButtonSelect(Button buttonSelected)
    {
        nameLabel.text += buttonSelected.GetComponentInChildren<TMP_Text>().text;
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
            character.statistics = menuSelectCharacterToCreate.characterSelected.CloneStatistics();
            character.mastery = menuSelectCharacterToCreate.characterSelected.CloneMastery();
            character.skills = menuSelectCharacterToCreate.characterSelected.CloneSkills();
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
                menuSelectCharacterToCreate.container.SetActive(true);
                menuSelectCharacterToCreate.gameObject.SetActive(false);
                menuSelectCharacterToCreate.otherMenu.GetComponent<MenuSelectCharacterToCreate.IMenuSelectCharacterToCreate>().EnableOtherMenu();
                menuSelectCharacterToCreate.otherMenu.SetActive(true);
                await Awaitable.NextFrameAsync();
                gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
