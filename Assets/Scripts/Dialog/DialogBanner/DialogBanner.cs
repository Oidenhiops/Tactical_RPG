using UnityEngine;
using UnityEngine.EventSystems;

public class DialogBanner : MonoBehaviour, ISubmitHandler
{
    public DialogBaseSO newDialog;
    public DialogManager dialogManager;
    public ManagementLanguage managementLanguage;
    public void OnSubmit(BaseEventData eventData)
    {
        if (!dialogManager.isResettingDialog) _ = dialogManager.OnHandleSelectBanner(this);
    }
}
