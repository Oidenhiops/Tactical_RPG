using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharButton : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
    public TMP_Text charLabel;
    public MenuSetCharacterNameToCreate menuSetCharacterNameToCreate;
    public Button charButton;

    public void OnPointerClick(PointerEventData eventData)
    {
        menuSetCharacterNameToCreate.OnButtonSelect(charButton);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        menuSetCharacterNameToCreate.OnButtonSelect(charButton);
    }
}
