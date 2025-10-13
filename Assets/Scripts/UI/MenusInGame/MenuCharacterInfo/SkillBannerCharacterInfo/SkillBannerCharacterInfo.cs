using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillBannerCharacterInfo : SkillCharacterBanner, IDeselectHandler
{
    public Image skillBg;
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
        onObjectSelect.OnSelect();
    }
    public void OnDeselectBanner()
    {
        skillBg.color = Color.white;
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        
    }
}
