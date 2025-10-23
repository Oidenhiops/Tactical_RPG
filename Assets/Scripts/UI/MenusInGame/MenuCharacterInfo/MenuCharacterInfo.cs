using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuCharacterInfo : MonoBehaviour
{
    public GameObject menuCharacterInfo;
    public Transform statusEffectsContainer;
    public GameObject statusEffectBanner;
    public GameObject skillBanner;
    public RectTransform skillsViewport;
    public RectTransform skillsContainer;
    public ScrollRect skillsScrollRect;
    public int skillBannerIndex;
    public ManagementLanguage skillDescription;
    public Image characterSprite;
    public TMP_Text characterLevel;
    public TMP_Text characterMovementRadius;
    public TMP_Text characterMovementMaxHeight;
    public TMP_Text characterName;
    public Button itemsButton;
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
    public InputAction changeSkillInput;
    void Awake()
    {
        changeSubMenuInput.Enable();
        changeSubMenuInput.started += OnHandleChangeSubMenu;
        changeSkillInput.Enable();
        changeSkillInput.started += OnHandleChangeSkill;
    }
    void OnDestroy()
    {
        changeSubMenuInput.started -= OnHandleChangeSubMenu;
        changeSkillInput.started -= OnHandleChangeSkill;
    }
    public async Task ReloadInfo(CharacterBase character, bool disableItemsContainer = false)
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
            itemsButton.interactable = true;
            itemsButton.gameObject.SetActive(true);
            foreach (KeyValuePair<CharacterData.CharacterItemInfo, CharacterData.CharacterItem> item in character.characterData.items)
            {
                if (item.Value.itemBaseSO)
                {
                    itemsInfo[item.Key.index].disableBanner.SetActive(false);
                    itemsInfo[item.Key.index].enabledBanner.SetActive(true);
                    itemsInfo[item.Key.index].managementLanguage.id = item.Value.itemBaseSO.idText;
                    itemsInfo[item.Key.index].managementLanguage.RefreshDialog();
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
            itemsButton.interactable = false;
            itemsButton.gameObject.SetActive(false);
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

        skillBannerIndex = 0;
        foreach (Transform child in skillsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        if (character.characterData.skills.Count > 0)
        {
            skillDescription.transform.parent.gameObject.SetActive(true);
            foreach (KeyValuePair<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>> typeSkill in character.characterData.skills[ItemBaseSO.TypeWeapon.None])
            {
                foreach (KeyValuePair<string, CharacterData.CharacterSkillInfo> skill in typeSkill.Value)
                {
                    SkillBannerCharacterInfo banner = Instantiate(skillBanner, skillsContainer.transform).GetComponent<SkillBannerCharacterInfo>();
                    banner.onObjectSelect.container = skillsContainer;
                    banner.onObjectSelect.viewport = skillsViewport;
                    banner.onObjectSelect.scrollRect = skillsScrollRect;
                    banner.menuCharacterInfo = this;
                    banner.SetBannerData(skill.Value);
                }
            }
            character.characterData.GetCurrentWeapon(out CharacterData.CharacterItem weapon);

            if (weapon != null && character.characterData.skills.ContainsKey(weapon.itemBaseSO.typeWeapon))
            {
                foreach (KeyValuePair<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>> typeSkill in character.characterData.skills[weapon.itemBaseSO.typeWeapon])
                {
                    foreach (KeyValuePair<string, CharacterData.CharacterSkillInfo> skill in typeSkill.Value)
                    {
                        SkillBannerCharacterInfo banner = Instantiate(skillBanner, skillsContainer.transform).GetComponent<SkillBannerCharacterInfo>();
                        banner.onObjectSelect.container = skillsContainer;
                        banner.onObjectSelect.viewport = skillsViewport;
                        banner.onObjectSelect.scrollRect = skillsScrollRect;
                        banner.menuCharacterInfo = this;
                        banner.SetBannerData(skill.Value);
                    }
                }
            }
            skillDescription.id = skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().skill.skillsBaseSO.skillIdText;
            skillDescription.otherInfo = skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().skill.skillsBaseSO.GetSkillDescription(skillsContainer.GetChild(0).GetComponent<SkillBannerCharacterInfo>().skill.statistics);
            skillDescription.RefreshDescription();
            await Awaitable.NextFrameAsync();
            skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().OnSelectBanner();
        }
        else
        {
            skillDescription.transform.parent.gameObject.SetActive(false);
        }

        if (disableItemsContainer && subMenuIndex == 2)
        {
            EnableSubMenu(1);
        }
        else
        {
            EnableSubMenu(subMenuIndex);
        }

        menuCharacterInfo.SetActive(true);
        isMenuActive = true;
    }
    public void ChangeSubMenu()
    {
        int discount = itemsButton.IsInteractable() ? 0 : 1;
        subMenuIndex += 1;
        if (subMenuIndex > 2 - discount)
        {
            subMenuIndex = 0;
        }
        EnableSubMenu(subMenuIndex);
    }
    public void DeselectAllBanners()
    {
        for (int i = 0; i < skillsContainer.childCount; i++)
        {
            skillsContainer.GetChild(i).GetComponent<SkillBannerCharacterInfo>().OnDeselectBanner();
        }
    }
    public void OnHandleChangeSkill(InputAction.CallbackContext context)
    {
        if (subMenusInfo[1].subMenuContainer.activeSelf && isMenuActive)
        {
            skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().OnDeselectBanner();
            int direction = context.ReadValue<float>() > 0 ? 1 : -1;
            skillBannerIndex += direction;
            if (skillBannerIndex >= skillsContainer.childCount)
            {
                skillBannerIndex = 0;
            }
            else if (skillBannerIndex < 0)
            {
                skillBannerIndex = skillsContainer.childCount - 1;
            }

            ManagementLanguage managementLanguage = skillDescription.GetComponent<ManagementLanguage>();
            managementLanguage.id = skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().skill.skillsBaseSO.skillIdText;
            managementLanguage.otherInfo = skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().skill.skillsBaseSO.GetSkillDescription(skillsContainer.GetChild(0).GetComponent<SkillBannerCharacterInfo>().skill.statistics);
            skillsContainer.GetChild(skillBannerIndex).GetComponent<SkillBannerCharacterInfo>().OnSelectBanner();
            managementLanguage.RefreshDescription();
        }
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
    public void SetDescriptionData(SkillBannerCharacterInfo skillBannerCharacterInfo)
    {
        skillDescription.id = skillBannerCharacterInfo.skill.skillsBaseSO.skillIdText;
        skillDescription.otherInfo = skillBannerCharacterInfo.skill.skillsBaseSO.GetSkillDescription(skillBannerCharacterInfo.skill.statistics);
        skillDescription.RefreshDescription();
    }
    public void DisableMenu(bool conservCharacter = false)
    {
        menuCharacterInfo.SetActive(false);
        isMenuActive = false;
        subMenuIndex = 0;
        if (!conservCharacter && BattlePlayerManager.Instance) BattlePlayerManager.Instance.aStarPathFinding.characterSelected = null;
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
