using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCharacterBanner : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    public CharacterData.CharacterSkillInfo skill;
    public ManagementLanguage managementLanguage;
    public OnObjectSelect onObjectSelect;
    public TMP_Text skillLevel;
    public Image skillFillAmount;
    public TMP_Text skillCost;
    public MenuSkillsCharacter menuSkillsCharacter;
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
    public virtual void OnHandleSelect(BaseEventData eventData)
    {
        menuSkillsCharacter.SetDescriptionData(this);
        menuSkillsCharacter.index = transform.GetSiblingIndex();
    }
    public virtual void OnSubmit(BaseEventData eventData)
    {
        menuSkillsCharacter.OnSkillSelect(this);
    }
}
