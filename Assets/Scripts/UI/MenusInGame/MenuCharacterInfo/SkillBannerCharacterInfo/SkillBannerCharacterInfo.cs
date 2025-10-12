using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBannerCharacterInfo : MonoBehaviour
{
    public CharacterData.CharacterSkillInfo skill;
    public ManagementLanguage managementLanguage;
    public TMP_Text skillLevel;
    public Image skillFillAmount;
    public TMP_Text skillCost;
    public void SetBannerData(CharacterData.CharacterSkillInfo characterSkill)
    {
        skill = characterSkill;
        skill.skillsBaseSO = characterSkill.skillsBaseSO;
        managementLanguage.id = characterSkill.skillsBaseSO.skillIdText;
        managementLanguage.RefreshText();
        skillLevel.text = characterSkill.level.ToString();
        skillCost.text = characterSkill.statistics[CharacterData.TypeStatistic.Sp].baseValue.ToString();
        skillFillAmount.fillAmount = (float)characterSkill.statistics[CharacterData.TypeStatistic.Exp].currentValue / characterSkill.statistics[CharacterData.TypeStatistic.Exp].maxValue;
    }
}
