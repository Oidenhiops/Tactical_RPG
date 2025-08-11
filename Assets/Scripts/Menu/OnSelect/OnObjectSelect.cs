using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnObjectSelect : MonoBehaviour, ISelectHandler
{
    public bool isDropDown;
    public Toggle toggle;
    public TMP_Dropdown dropdown;
    public DropdownSelected dropdownSelected;
    public float marginToDetect = 5;
    public bool scrollByCode;
    public RectTransform item;
    public RectTransform viewport;
    public RectTransform container;
    public ScrollRect scrollRect;
    int index;
    float maxItemsInView;
    float itemsNonView;
    float sizeItemInViewport;
    public static bool isInit = false;
    void LateUpdate()
    {
        index = transform.GetSiblingIndex();
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (!scrollByCode)
        {
            if (isDropDown && dropdownSelected.isInit || !isDropDown) ScrollTo(index);
        }
    }
    public void ScrollTo(int pos)
    {
        int childCount = !isDropDown ? container.childCount : container.childCount - 1;
        maxItemsInView = viewport.rect.height / item.rect.height;
        itemsNonView = childCount - maxItemsInView;
        sizeItemInViewport = 1 / itemsNonView;
        if (!isDropDown && pos == 0 || isDropDown && pos == 1)
        {
            scrollRect.verticalNormalizedPosition = 1;
        }
        else if (pos == childCount)
        {
            scrollRect.verticalNormalizedPosition = 0;
        }
        else if (!IsItemVisible(item, out int direction))
        {
            if (isDropDown)
            {
                scrollRect.verticalNormalizedPosition = 1 - sizeItemInViewport * (direction < 0 ? Mathf.Abs(index - maxItemsInView) : index - 1);
            }
            else 
            {
                scrollRect.verticalNormalizedPosition = 1 - sizeItemInViewport * (direction < 0 ? Mathf.Abs(index - (maxItemsInView - 1)) : index);
            }
        }
    }
    public bool IsItemVisible(RectTransform itemToCheck, out int direction)
    {
        Vector3[] itemWorldCorners = new Vector3[4];
        Vector3[] viewportWorldCorners = new Vector3[4];

        itemToCheck.GetWorldCorners(itemWorldCorners);
        viewport.parent.GetComponent<RectTransform>().GetWorldCorners(viewportWorldCorners);

        float viewportMaxY = viewportWorldCorners[1].y;
        float itemMaxY = itemWorldCorners[1].y - 1;
        float viewportMinY = viewportWorldCorners[0].y;
        float itemMinY = itemWorldCorners[0].y + 1;

        bool isVisible = itemMinY >= viewportMinY && itemMaxY <= viewportMaxY;

        direction = viewportMaxY >= itemMaxY ? -1 : 1;

        return isVisible;
    }
}