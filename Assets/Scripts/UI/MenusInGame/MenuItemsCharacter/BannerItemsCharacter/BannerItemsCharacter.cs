using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BannerItemsCharacter : MonoBehaviour, ISubmitHandler, ISelectHandler
{
    public int index;
    public OnObjectSelect onObjectSelect;
    public MenuItemsCharacter menuItemsCharacter;
    public ManagementLanguage managementLanguage;
    public CharacterData.CharacterItem item;
    public Image itemSprite;
    public GameObject disableItemBanner;
    public GameObject enableItemBanner;
    public bool isBagItem;
    public void SetBannerData(CharacterData.CharacterItem characterItem)
    {
        item = characterItem;
        itemSprite.sprite = characterItem.itemBaseSO.icon;
        managementLanguage.id = characterItem.itemId;
        managementLanguage.RefreshText();
    }
    public void EnableBanner()
    {
        disableItemBanner.SetActive(false);
        enableItemBanner.SetActive(true);
    }
    public void DisableBanner()
    {
        disableItemBanner.SetActive(true);
        enableItemBanner.SetActive(false);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        if (isBagItem)
        {
            menuItemsCharacter.OnItemBagSelect(this);
        }
        else
        {
            menuItemsCharacter.OnItemSelect(this);
        }
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (isBagItem) menuItemsCharacter.index = index;
    }
}
