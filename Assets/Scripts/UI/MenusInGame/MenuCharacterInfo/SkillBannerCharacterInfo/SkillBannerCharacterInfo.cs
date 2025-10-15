using UnityEngine;
using UnityEngine.EventSystems;

public class SkillBannerCharacterInfo : SkillCharacterBanner, IDeselectHandler, IPointerClickHandler
{
    public MenuCharacterInfo menuCharacterInfo;
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
    public void OnPointerClick(PointerEventData eventData)
    {
        menuCharacterInfo.DeselectAllBanners();
        OnSelectBanner();
    }
}
