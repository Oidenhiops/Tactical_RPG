using UnityEngine;
using UnityEngine.EventSystems;

public class SkillBannerCharacterInfo : SkillCharacterBanner, IDeselectHandler
{
    public MenuCharacterInfo menuCharacterInfo;
    public override void OnHandleSelect(BaseEventData eventData)
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
        menuCharacterInfo.SetDescriptionData(this);
        onObjectSelect.OnSelect();
    }
    public void OnDeselectBanner()
    {
        skillBg.color = Color.white;
    }
}
