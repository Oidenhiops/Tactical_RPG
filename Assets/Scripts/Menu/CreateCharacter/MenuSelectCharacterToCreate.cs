using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuSelectCharacterToCreate : MonoBehaviour
{
    public InputAction backButton;
    public InputAction movementButton;
    public InputAction selectButton;
    public GameObject otherMenu;
    public GameObject lastButtonSelected;
    public Vector2Int index;
    public GameObject container;
    public SerializedDictionary<int, List<GameObject>> characters = new SerializedDictionary<int, List<GameObject>>();
    public GameObject characterPreviewPrefab;
    public Transform charactersContainer;
    public Transform gridCellMouse;
    public MenuSetCharacterNameToCreate menuCharacterSetName;
    public InitialDataSO characterSelected;
    public MenuCharacterInfo menuCharacterInfo;
    public bool isMenuActive;
    public async Awaitable SpawnCharacters()
    {
        try
        {
            int xPos = -1;
            bool xPosLock = false;
            for (int x = 1; x <= GameData.Instance.charactersDataDBSO.data.Count; x++)
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
                        CharacterBase character = Instantiate(characterPreviewPrefab, charactersContainer).GetComponent<CharacterBase>();
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
            await Awaitable.NextFrameAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable EnableMenu()
    {
        try
        {
            backButton.started += UnloadMenuCreateCharacter;
            movementButton.started += ChangeIndex;
            selectButton.started += SelectCharacter;
            backButton.Enable();
            movementButton.Enable();
            selectButton.Enable();
            lastButtonSelected = EventSystem.current.currentSelectedGameObject;
            await SpawnCharacters();
            await Awaitable.NextFrameAsync();
            otherMenu.SetActive(false);
            _ = menuCharacterInfo.ReloadInfo(characters[index.x][index.y].GetComponent<CharacterBase>());
            gameObject.SetActive(true);
            isMenuActive = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void AddCharactersToInfiniteLoop()
    {
        int finalPosLeft = 1 + characters.Count - 1;
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < characters[x].Count; y++)
            {
                CharacterBase character = Instantiate(characterPreviewPrefab, charactersContainer).GetComponent<CharacterBase>();
                character.initialDataSO = characters[x][y].GetComponent<CharacterBase>().initialDataSO;
                _ = character.InitializeCharacter();
                character.transform.localPosition = new Vector3(y, 0, finalPosLeft + x);
            }
        }
        int finalPosRight = -1;
        for (int x = characters.Count - 1; x > characters.Count - 6; x--)
        {
            for (int y = 0; y < characters[x].Count; y++)
            {
                CharacterBase character = Instantiate(characterPreviewPrefab, charactersContainer).GetComponent<CharacterBase>();
                character.initialDataSO = characters[x][y].GetComponent<CharacterBase>().initialDataSO;
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
            characterSelected = characters[index.x][index.y].GetComponent<CharacterBase>().initialDataSO;
            _ = menuCharacterSetName.EnableMenu();
        }
    }
    public void ChangeIndex(InputAction.CallbackContext context)
    {
        if (!isMenuActive) return;
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
        _= menuCharacterInfo.ReloadInfo(characters[index.x][index.y].GetComponent<CharacterBase>());
    }
    void UnloadMenuCreateCharacter(InputAction.CallbackContext context)
    {
        if (isMenuActive)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lastButtonSelected);
            backButton.started -= UnloadMenuCreateCharacter;
            movementButton.started -= ChangeIndex;
            selectButton.started -= SelectCharacter;
            index = Vector2Int.zero;
            foreach (Transform child in charactersContainer.transform)
            {
                Destroy(child.gameObject);
            }
            gridCellMouse.localPosition = Vector3.zero;
            characters = new SerializedDictionary<int, List<GameObject>>();
            menuCharacterInfo.DisableMenu();
            isMenuActive = false;
            otherMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}