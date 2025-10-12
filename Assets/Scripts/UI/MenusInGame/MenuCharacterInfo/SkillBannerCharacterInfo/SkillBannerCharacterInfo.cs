using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillBannerCharacterInfo : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public CharacterData.CharacterSkillInfo skill;
    public ManagementLanguage managementLanguage;
    public TMP_Text skillLevel;
    public Image skillFillAmount;
    public TMP_Text skillCost;
    public Image skillBg;
    public OnObjectSelect onObjectSelect;
    public void SetBannerData(CharacterData.CharacterSkillInfo characterSkill)
    {
        skill = characterSkill;
        skill.skillsBaseSO = characterSkill.skillsBaseSO;
        managementLanguage.id = characterSkill.skillsBaseSO.skillIdText;
        managementLanguage.RefreshDialog();
        skillLevel.text = characterSkill.level.ToString();
        skillCost.text = characterSkill.statistics[CharacterData.TypeStatistic.Sp].baseValue.ToString();
        skillFillAmount.fillAmount = (float)characterSkill.statistics[CharacterData.TypeStatistic.Exp].currentValue / characterSkill.statistics[CharacterData.TypeStatistic.Exp].maxValue;
    }
    public void OnHandleSelect(BaseEventData eventData)
    {
        OnSelectBanner();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnDeselectBanner();
    }
    public void OnSelectBanner()
    {
        skillBg.color = Color.yellow;
        onObjectSelect.OnSelect();
    }
    public void OnDeselectBanner()
    {
        skillBg.color = Color.white;
    }
}
