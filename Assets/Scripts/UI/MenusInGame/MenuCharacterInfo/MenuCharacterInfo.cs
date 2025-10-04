using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuCharacterInfo : MonoBehaviour
{
    public GameObject menuCharacterInfo;
    public Transform statusEffectsContainer;
    public Image characterSprite;
    public TMP_Text characterLevel;
    public TMP_Text characterMovementRadius;
    public TMP_Text characterMovementMaxHeight;
    public TMP_Text characterName;
    public GameObject statusEffectBanner;
    public Transform itemsContainer;
    public SerializedDictionary<int, ItemInfo> itemsInfo = new SerializedDictionary<int, ItemInfo>();
    public SubMenuInfo[] subMenusInfo;
    public SerializedDictionary<CharacterData.TypeStatistic, UiInfo> uiInfo = new SerializedDictionary<CharacterData.TypeStatistic, UiInfo>();
    public SerializedDictionary<CharacterData.TypeStatistic, TMP_Text> aptitudes = new SerializedDictionary<CharacterData.TypeStatistic, TMP_Text>();
    public SerializedDictionary<CharacterData.TypeMastery, MasteryInfo> masteries = new SerializedDictionary<CharacterData.TypeMastery, MasteryInfo>();
    public bool isMenuActive;
    public Color subMenuSelected;
    public Color subMenuDeselected;
    public int subMenuIndex = 0;
    public InputAction changeSubMenuInput;
    void Awake()
    {
        changeSubMenuInput.Enable();
        changeSubMenuInput.started += OnHandleChangeSubMenu;
    }
    void OnDestroy()
    {
        changeSubMenuInput.started -= OnHandleChangeSubMenu;
    }
    public void ReloadInfo(Character character, bool disableItemsContainer = false)
    {
        characterSprite.sprite = character.initialDataSO.icon;
        characterName.text = character.characterData.name;
        characterLevel.text = character.characterData.level.ToString();
        characterMovementMaxHeight.text = character.characterData.GetMovementMaxHeight().ToString();
        characterMovementRadius.text = character.characterData.GetMovementRadius().ToString();
        foreach (KeyValuePair<CharacterData.TypeStatistic, UiInfo> statisticsUi in uiInfo)
        {
            if (statisticsUi.Key != CharacterData.TypeStatistic.None)
            {
                statisticsUi.Value.characterStatistic.text = character.characterData.statistics[statisticsUi.Key].currentValue.ToString();
            }
            else
            {
                statisticsUi.Value.characterStatistic.text = (character.characterData.statistics[CharacterData.TypeStatistic.Exp].maxValue - character.characterData.statistics[CharacterData.TypeStatistic.Exp].currentValue).ToString();
            }
        }
        foreach (Transform child in statusEffectsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (KeyValuePair<StatusEffectBaseSO, int> statusEffect in character.characterStatusEffect.statusEffects)
        {
            StatusEffectBanner banner = Instantiate(statusEffectBanner, statusEffectsContainer.transform).GetComponent<StatusEffectBanner>();
            banner.SetData(statusEffect.Key, statusEffect.Value);
        }
        if (!disableItemsContainer)
        {
            itemsContainer.gameObject.SetActive(true);
            foreach (KeyValuePair<CharacterData.CharacterItemInfo, CharacterData.CharacterItem> item in character.characterData.items)
            {
                if (item.Value.itemBaseSO)
                {
                    itemsInfo[item.Key.index].disableBanner.SetActive(false);
                    itemsInfo[item.Key.index].enabledBanner.SetActive(true);
                    itemsInfo[item.Key.index].managementLanguage.id = item.Value.itemBaseSO.idText;
                    itemsInfo[item.Key.index].managementLanguage.RefreshText();
                    itemsInfo[item.Key.index].itemSprite.sprite = item.Value.itemBaseSO.icon;
                }
                else
                {
                    itemsInfo[item.Key.index].disableBanner.SetActive(true);
                    itemsInfo[item.Key.index].enabledBanner.SetActive(false);
                }
            }
        }
        else
        {
            itemsContainer.gameObject.SetActive(false);
        }
        foreach (KeyValuePair<CharacterData.TypeStatistic, TMP_Text> aptitude in aptitudes)
        {
            aptitude.Value.text = character.characterData.statistics[aptitude.Key].aptitudeValue + "%";
        }
        foreach (KeyValuePair<CharacterData.TypeMastery, MasteryInfo> mastery in masteries)
        {
            mastery.Value.masteryRange.text = character.characterData.mastery[mastery.Key].masteryRange.ToString();
            mastery.Value.masteryLevel.text = character.characterData.mastery[mastery.Key].masteryLevel.ToString();
            mastery.Value.masteryLevelFill.fillAmount = character.characterData.mastery[mastery.Key].currentExp / (float)character.characterData.mastery[mastery.Key].maxExp;
        }
        EnableSubMenu(0);
        menuCharacterInfo.SetActive(true);
        isMenuActive = true;
    }
    public void ChangeSubMenu()
    {
        subMenuIndex += 1;
        if (subMenuIndex > 2)
        {
            subMenuIndex = 0;
        }
        EnableSubMenu(subMenuIndex);
    }
    void OnHandleChangeSubMenu(InputAction.CallbackContext context)
    {
        if (isMenuActive) ChangeSubMenu();
    }
    public void EnableSubMenu(int indexMenu)
    {
        subMenuIndex = indexMenu;
        for (int i = 0; i < subMenusInfo.Length; i++)
        {
            if (i == indexMenu)
            {
                subMenusInfo[i].buttonSprite.color = subMenuSelected;
                subMenusInfo[i].subMenuContainer.SetActive(true);
            }
            else
            {
                subMenusInfo[i].buttonSprite.color = subMenuDeselected;
                subMenusInfo[i].subMenuContainer.SetActive(false);
            }
        }
    }
    public void DisableMenu(bool conservCharacter = false)
    {
        menuCharacterInfo.SetActive(false);
        isMenuActive = false;
        if (!conservCharacter && AStarPathFinding.Instance) AStarPathFinding.Instance.characterSelected = null;
    }
    [Serializable]
    public class ItemInfo
    {
        public GameObject disableBanner;
        public GameObject enabledBanner;
        public Image itemSprite;
        public ManagementLanguage managementLanguage;
    }
    [Serializable]
    public class SubMenuInfo
    {
        public Image buttonSprite;
        public GameObject subMenuContainer;
    }
    [Serializable]
    public class UiInfo
    {
        public TMP_Text characterStatistic;
    }
    [Serializable]
    public class MasteryInfo
    {
        public TMP_Text masteryRange;
        public TMP_Text masteryLevel;
        public Image masteryLevelFill;
    }
}
