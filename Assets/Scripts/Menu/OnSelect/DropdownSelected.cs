using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownSelected : MonoBehaviour, ISubmitHandler, IPointerDownHandler
{
    public TMP_Dropdown dropdown;
    public bool isInit;
    void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isInit)
        {
            isInit = false;
        }
        _= ScrollTo();
    }
    void OnDropdownValueChanged(int index)
    {
        isInit = false;
    }
    public async Awaitable ScrollTo()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(0.15f));
            OnObjectSelect component = transform.GetChild(transform.childCount - 1).transform.GetChild(0).transform.GetChild(0).transform.GetChild(dropdown.value + 1).GetComponent<OnObjectSelect>();
            if (component)
            {
                component.ScrollTo(dropdown.value);
                isInit = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    public void OnSubmit(BaseEventData eventData)
    {
        if (isInit)
        {
            isInit = false;
        }
        _= ScrollTo();
    }
}
