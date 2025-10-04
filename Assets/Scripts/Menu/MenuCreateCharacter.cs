using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuCreateCharacter : MonoBehaviour
{
    public InputAction backButton;
    public InputAction movementButton;
    public InputAction selectButton;
    public GameObject otherMenu;
    public GameObject lastButtonSelected;
    public Vector2Int index;
    public SerializedDictionary<int, List<GameObject>> characters = new SerializedDictionary<int, List<GameObject>>();
    public GameObject characterPreviewPrefab;
    public Transform charactersContainer;
    public Transform gridCellMouse;
    public GameObject menuCharacterSetName;
    public InitialDataSO characterSelected;
    public TMP_InputField inputField;
    public bool isMenuActive;
    void OnEnable()
    {
        backButton.started += UnloadMenuCreateCharacter;
        movementButton.started += ChangeIndex;
        selectButton.started += SelectCharacter;
        backButton.Enable();
        movementButton.Enable();
        selectButton.Enable();
        lastButtonSelected = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(otherMenu);
        StartCoroutine(EnableMenu());
        otherMenu.SetActive(false);
    }
    void OnDisable()
    {
        backButton.started -= UnloadMenuCreateCharacter;
        movementButton.started -= ChangeIndex;
        index = Vector2Int.zero;
        foreach (Transform child in charactersContainer.transform)
        {
            Destroy(child.gameObject);
        }
        characters = new SerializedDictionary<int, List<GameObject>>();
    }
    public IEnumerator EnableMenu()
    {
        int xPos = -1;
        bool xPosLock = false;
        for (int x = 0; x < GameData.Instance.charactersDataDBSO.data.Count; x++)
        {
            for (int y = 0; y < GameData.Instance.charactersDataDBSO.data[x].Count; y++)
            {
                if (GameData.Instance.charactersDataDBSO.data[x][y].isUnlocked)
                {
                    if (!xPosLock)
                    {
                        xPosLock = true;
                        xPos++;
                    }
                    Character character = Instantiate(characterPreviewPrefab, charactersContainer).GetComponent<Character>();
                    character.initialDataSO = GameData.Instance.charactersDataDBSO.data[x][y].initialDataSO;
                    _ = character.InitializeCharacter();
                    character.transform.localPosition = new Vector3(y, 0, xPos);
                    if (characters.ContainsKey(xPos))
                    {
                        characters[xPos].Add(character.gameObject);
                    }
                    else
                    {

                        characters.Add(xPos, new List<GameObject> { character.gameObject });
                    }
                }
                else
                {
                    break;
                }
            }
            xPosLock = false;
        }
        AddCharactersToInfiniteLoop();
        yield return null;
        isMenuActive = true;
    }
    public void AddCharactersToInfiniteLoop()
    {
        int finalPosLeft = 1 + characters.Count - 1;
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < characters[x].Count; y++)
            {
                Character character = Instantiate(characterPreviewPrefab, charactersContainer).GetComponent<Character>();
                character.initialDataSO = characters[x][y].GetComponent<Character>().initialDataSO;
                _ = character.InitializeCharacter();
                character.transform.localPosition = new Vector3(y, 0, finalPosLeft + x);
            }
        }
        int finalPosRight = -1;
        for (int x = characters.Count - 1; x > characters.Count - 6; x--)
        {
            for (int y = 0; y < characters[x].Count; y++)
            {
                Character character = Instantiate(characterPreviewPrefab, charactersContainer).GetComponent<Character>();
                character.initialDataSO = characters[x][y].GetComponent<Character>().initialDataSO;
                _ = character.InitializeCharacter();
                character.transform.localPosition = new Vector3(y, 0, finalPosRight);
            }
            finalPosRight--;
        }
    }
    public void SelectCharacter(InputAction.CallbackContext context)
    {
        if (isMenuActive)
        {
            if (characterSelected) return;
            characterSelected = characters[index.x][index.y].GetComponent<Character>().initialDataSO;
            inputField.text = "";
            menuCharacterSetName.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }
    }
    public void ChangeIndex(InputAction.CallbackContext context)
    {
        if (characterSelected) return;
        Vector2 direction = context.ReadValue<Vector2>();
        direction.x *= -1;
        index += Vector2Int.RoundToInt(direction);
        if (index.x < 0)
        {
            index.x = characters.Count - 1;
        }
        else if (index.x > characters.Count - 1)
        {
            index.x = 0;
        }
        if (index.y > characters[index.x].Count - 1)
        {
            index.y = characters[index.x].Count - 1;
        }
        else if (index.y < 0)
        {
            index.y = characters[index.x].Count - 1;
        }
        gridCellMouse.transform.localPosition = new Vector3(index.y, 0, index.x);
    }
    void UnloadMenuCreateCharacter(InputAction.CallbackContext context)
    {
        if (isMenuActive)
        {
            UnloadMenu();
        }
    }
    public void UnloadMenu()
    {
        if (!menuCharacterSetName.activeSelf)
        {
            otherMenu.SetActive(true);
            gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lastButtonSelected);
            isMenuActive = false;
        }
        else
        {
            inputField.text = "";
            EventSystem.current.SetSelectedGameObject(null);
            characterSelected = null;
            menuCharacterSetName.SetActive(false);
        }
    }
    public void CreateCharacter()
    {
        if (inputField.text != "")
        {
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].isUse = true;
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].principalCharacterName = inputField.text;
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].currentZone = "City";
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].lastSaveDate = GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].createdDate;
            CharacterData character = new CharacterData
            {
                id = characterSelected.id,
                subId = characterSelected.subId,
                name = inputField.text,
                level = 1,
                mastery = new SerializedDictionary<CharacterData.TypeMastery, CharacterData.CharacterMasteryInfo>()
            };
            character.statistics = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneStatistics();
            character.mastery = GameData.Instance.charactersDataDBSO.data[character.id][character.subId].initialDataSO.CloneMastery();
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
            inputField.text = "";
            menuCharacterSetName.SetActive(false);
            otherMenu.SetActive(true);
            gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lastButtonSelected);
            isMenuActive = false;
        }
    }
}