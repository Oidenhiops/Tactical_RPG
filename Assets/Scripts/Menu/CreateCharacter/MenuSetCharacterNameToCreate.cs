using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public async Task EnableMenu()
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
    public void OnHandleBack(InputAction.CallbackContext context)
    {
        if (isMenuActive) _ = DisableMenu();
    }
    async Task DisableMenu()
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
        nameLabel.text = GenerateFantasyName();
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
                    name = GenerateFantasyName(),
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
    string GenerateFantasyName()
    {
        string[] syllablesStart = { "Ka", "Lo", "Mi", "Ra", "Th", "El", "Ar", "Va", "Zy", "Xe", "Lu", "Na" };
        string[] syllablesMiddle = { "ra", "en", "or", "il", "um", "ar", "is", "al", "on", "ir" };
        string[] syllablesEnd = { "th", "dor", "ion", "mir", "rak", "len", "var", "oth", "us", "iel" };

        int pattern = UnityEngine.Random.Range(0, 3);
        string name = "";

        switch (pattern)
        {
            case 0:
                name = syllablesStart[UnityEngine.Random.Range(0, syllablesStart.Length)] +
                        syllablesEnd[UnityEngine.Random.Range(0, syllablesEnd.Length)];
                break;
            case 1:
                name = syllablesStart[UnityEngine.Random.Range(0, syllablesStart.Length)] +
                        syllablesMiddle[UnityEngine.Random.Range(0, syllablesMiddle.Length)] +
                        syllablesEnd[UnityEngine.Random.Range(0, syllablesEnd.Length)];
                break;
            case 2:
                name = syllablesStart[UnityEngine.Random.Range(0, syllablesStart.Length)] +
                        syllablesMiddle[UnityEngine.Random.Range(0, syllablesMiddle.Length)] +
                        syllablesMiddle[UnityEngine.Random.Range(0, syllablesMiddle.Length)] +
                        syllablesEnd[UnityEngine.Random.Range(0, syllablesEnd.Length)];
                break;
        }

        return name;
    }
    public async Task DisableMenuAfterSetName()
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
}
