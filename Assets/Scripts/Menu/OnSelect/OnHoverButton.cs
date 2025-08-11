using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnHoverButton : MonoBehaviour, IPointerEnterHandler
{
    Button button;
    void OnEnable()
    {
        button = GetComponent<Button>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        button.Select();
    }
}
