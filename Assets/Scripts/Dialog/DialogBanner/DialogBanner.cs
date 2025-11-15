using UnityEngine;
using UnityEngine.EventSystems;

public class DialogBanner : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    public DialogBaseSO newDialog;
    public DialogManager dialogManager;
    public ManagementLanguage managementLanguage;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!dialogManager.isResettingDialog) _ = dialogManager.OnHandleSelectBanner(this);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!dialogManager.isResettingDialog) _ = dialogManager.OnHandleSelectBanner(this);
    }
}
