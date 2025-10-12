using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BannerSkillsCharacter : MonoBehaviour
{
    public CharacterData.CharacterSkillInfo bannerInfo;
    public ManagementLanguage managementLanguage;
    public void SetBannerData(CharacterData.CharacterSkillInfo characterSkill)
    {
        bannerInfo.skillsBaseSO = characterSkill.skillsBaseSO;
        managementLanguage.id = characterSkill.skillsBaseSO.skillIdText;
        managementLanguage.RefreshText();
    }
}
