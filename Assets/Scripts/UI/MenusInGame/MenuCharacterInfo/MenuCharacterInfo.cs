using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
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
    public SerializedDictionary<CharacterData.TypeStatistic, UiInfo> uiInfo = new SerializedDictionary<CharacterData.TypeStatistic, UiInfo>();
    [System.Serializable]
    public class UiInfo
    {
        public TMP_Text characterStatistic;
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
            statisticsUi.Value.characterStatistic.text = character.characterData.statistics[statisticsUi.Key].currentValue.ToString();
        }
        foreach (Transform child in statusEffectsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (KeyValuePair<StatusEffectBaseSO, CharacterStatusEffect.StatusEffectInfo> statusEffect in character.characterStatusEffect.statusEffects)
        {
            StatusEffectBanner banner = Instantiate(statusEffectBanner, statusEffectsContainer.transform).GetComponent<StatusEffectBanner>();
            banner.SetData(statusEffect.Key, statusEffect.Value.amount);
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
                    itemsInfo[item.Key.index].managementLanguage.id = item.Value.itemBaseSO.id;
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
        menuCharacterInfo.SetActive(true);
    }
    public void DisableMenu(bool conservCharacter = false)
    {
        menuCharacterInfo.SetActive(false);
        if (!conservCharacter) AStarPathFinding.Instance.characterSelected = null;
    }
    [System.Serializable] public class ItemInfo
    {
        public GameObject disableBanner;
        public GameObject enabledBanner;
        public Image itemSprite;
        public ManagementLanguage managementLanguage;
    }
}
