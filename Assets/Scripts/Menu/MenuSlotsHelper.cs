using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuSlotsHelper : MonoBehaviour, MenuSelectCharacterToCreate.IMenuSelectCharacterToCreate
{
    public GameManagerHelper gameManagerHelper;
    public GameObject slotButton;
    public InputAction backButton;
    public GameObject playButton;
    public GameObject buttonsContainer;
    public SlotInfo[] slotInfos = new SlotInfo[3];
    public MenuSelectCharacterToCreate menuCreateCharacter;
    public bool isMenuActive;
    void OnEnable()
    {
        backButton.started += UnloadSlotsMenu;
        backButton.Enable();
        EnableMenu();
    }
    void OnDisable()
    {
        backButton.started -= UnloadSlotsMenu;
    }
    public void SetButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(slotButton);
    }
    void EnableMenu()
    {
        for (int i = 0; i < GameData.Instance.gameDataInfo.gameDataSlots.Count; i++)
        {
            if (GameData.Instance.gameDataInfo.gameDataSlots[i].isUse)
            {
                slotInfos[i].noData.SetActive(false);
                slotInfos[i].data.SetActive(true);
                slotInfos[i].playTime.text = GetPlayTime(GameData.Instance.gameDataInfo.gameDataSlots[i].createdDate, GameData.Instance.gameDataInfo.gameDataSlots[i].lastSaveDate);
                slotInfos[i].mainCharacter.text = GetMainCharacterName(GameData.Instance.gameDataInfo.gameDataSlots[i]);
                slotInfos[i].level.text = GetMainCharacterLevel(GameData.Instance.gameDataInfo.gameDataSlots[i]);
                slotInfos[i].currentZone.text = GameData.Instance.gameDataInfo.gameDataSlots[i].currentZone.ToString().Replace("Scene", "");
            }
            else
            {
                slotInfos[i].noData.SetActive(true);
                slotInfos[i].data.SetActive(false);
            }
        }
        buttonsContainer.SetActive(false);
        isMenuActive = true;
    }
    string GetPlayTime(string createdDate, string lastSaveDate)
    {
        DateTime start = DateTime.ParseExact(createdDate, "yyyy-MM-dd HH:mm:ss", null);
        DateTime end   = DateTime.ParseExact(lastSaveDate, "yyyy-MM-dd HH:mm:ss", null);

        TimeSpan playTime = end - start;

        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            (int)playTime.TotalHours,
            playTime.Minutes,
            playTime.Seconds);
    }
    string GetMainCharacterName(GameData.GameDataSlot gameDataSlot)
    {
        return gameDataSlot.principalCharacterName;
    }
    string GetMainCharacterLevel(GameData.GameDataSlot gameDataSlot)
    {
        if (gameDataSlot.characters.ContainsKey(gameDataSlot.principalCharacterName))
        {
            return gameDataSlot.characters[gameDataSlot.principalCharacterName].level.ToString();
        }
        else if (gameDataSlot.dieCharacters.ContainsKey(gameDataSlot.principalCharacterName))
        {
            return gameDataSlot.dieCharacters[gameDataSlot.principalCharacterName].level.ToString();
        }
        return "1";
    }
    void UnloadSlotsMenu(InputAction.CallbackContext context)
    {
        if (isMenuActive)
        {
            buttonsContainer.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(playButton);
            gameObject.SetActive(false);
            isMenuActive = false;
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonBack"), 1, false);
        }
    }
    public void LoadOrCreateSlot(int slotIndex)
    {
        GameData.Instance.systemDataInfo.currentGameDataIndex = slotIndex;
        if (GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].isUse)
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            gameManagerHelper.ChangeScene(GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].currentZone);
            gameManagerHelper.SaveSystemData();
        }
        else
        {
            _ = menuCreateCharacter.EnableMenu();
        }
    }
    public void DeleteSlot(int index)
    {
        GameData.Instance.gameDataInfo.gameDataSlots[index] = new GameData.GameDataSlot();
        GameData.Instance.SaveGameData();
        slotInfos[index].noData.SetActive(true);
        slotInfos[index].data.SetActive(false);
    }

    public void DisableOtherMenu()
    {
        isMenuActive = false;
    }

    public void EnableOtherMenu()
    {
        isMenuActive = true;
    }

    [Serializable] public class SlotInfo
    {
        public GameObject noData;
        public GameObject data;
        public TMP_Text playTime;
        public TMP_Text mainCharacter;
        public TMP_Text level;
        public TMP_Text currentZone;
    }
}
