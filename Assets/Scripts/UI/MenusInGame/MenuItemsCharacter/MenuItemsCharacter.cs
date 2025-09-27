using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuItemsCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameObject menuItemCharacters;
    public int index;
    public SerializedDictionary<int, BannerItemsCharacter> gearBanners = new SerializedDictionary<int, BannerItemsCharacter>();
    public SerializedDictionary<int, BannerItemsCharacter> bagBanners = new SerializedDictionary<int, BannerItemsCharacter>();
    public BannerItemsCharacter currentBagItem;
    public async Task ReloadGearBanners()
    {
        var character = AStarPathFinding.Instance.characterSelected;
        for (int i = 0; i < gearBanners.Count; i++)
        {
            if (character.characterData.items[i].itemBaseSO)
            {
                gearBanners[i].SetBannerData(character.characterData.items[i]);
                gearBanners[i].EnableBanner();
            }
            else
            {
                gearBanners[i].item = new CharacterData.CharacterItem();
                gearBanners[i].DisableBanner();
            }
        }
        await Awaitable.NextFrameAsync();
    }
    public async Task ReloadBagBanners()
    {
        for (int i = 0; i < bagBanners.Count; i++)
        {
            if (GameData.Instance.saveData.bagItems[i].itemBaseSO)
            {
                bagBanners[i].SetBannerData(GameData.Instance.saveData.bagItems[i]);
                bagBanners[i].EnableBanner();
            }
            else
            {
                bagBanners[i].item = new CharacterData.CharacterItem();
                bagBanners[i].DisableBanner();
            }
        }
        await Awaitable.NextFrameAsync();
    }
    public void OnItemBagSelect(BannerItemsCharacter bagItem)
    {
        bagItem.GetComponent<Button>().interactable = false;
        currentBagItem = bagItem;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(gearBanners.ElementAt(0).Value.gameObject);
    }
    public void OnItemSelect(BannerItemsCharacter gearItem)
    {
        CharacterData.CharacterItem itemToBag = new CharacterData.CharacterItem()
        {
            itemId = gearItem.item.itemId,
            itemBaseSO = gearItem.item.itemBaseSO,
            itemStatistics = gearItem.item.itemStatistics
        };
        CharacterData.CharacterItem itemToGear = new CharacterData.CharacterItem()
        {
            itemId = currentBagItem.item.itemId,
            itemBaseSO = currentBagItem.item.itemBaseSO,
            itemStatistics = currentBagItem.item.itemStatistics
        };
        if (itemToGear.itemBaseSO && itemToGear.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Weapon ||
            itemToGear.itemBaseSO && itemToGear.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Monster)
        {
            if (itemToGear.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Weapon)
            {
                if (AStarPathFinding.Instance.characterSelected.initialDataSO.isHumanoid)
                {
                    if (ContainsOtherWeapon(out CharacterData.CharacterItem weapon))
                    {
                        if (itemToBag.itemBaseSO && weapon.itemBaseSO && itemToBag.itemBaseSO.typeObject == weapon.itemBaseSO.typeObject)
                        {
                            SetItemData(itemToBag, itemToGear, gearItem);
                        }
                    }
                    else
                    {
                        SetItemData(itemToBag, itemToGear, gearItem);
                    }
                }
            }
            else if (itemToGear.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Monster)
            {
                if (!AStarPathFinding.Instance.characterSelected.initialDataSO.isHumanoid)
                {
                    if (ContainsOtherWeapon(out CharacterData.CharacterItem weapon))
                    {
                        if (itemToBag.itemBaseSO.typeObject == weapon.itemBaseSO.typeObject)
                        {
                            SetItemData(itemToBag, itemToGear, gearItem);
                        }
                    }
                    else
                    {
                        SetItemData(itemToBag, itemToGear, gearItem);
                    }
                }
            }
            else
            {
                SetItemData(itemToBag, itemToGear, gearItem);
            }
        }
        else
        {
            SetItemData(itemToBag, itemToGear, gearItem);
        }
    }
    public void SetItemData(CharacterData.CharacterItem itemToBag, CharacterData.CharacterItem itemToGear, BannerItemsCharacter gearItem)
    {
        if (itemToBag.itemBaseSO)
        {
            itemToBag.itemBaseSO.DesEquipItem(AStarPathFinding.Instance.characterSelected, itemToBag);
        }
        if (itemToGear.itemBaseSO)
        {
            itemToGear.itemBaseSO.EquipItem(AStarPathFinding.Instance.characterSelected, itemToGear);
        }
        AStarPathFinding.Instance.characterSelected.characterData.items[gearItem.index] = itemToGear;
        GameData.Instance.saveData.bagItems[currentBagItem.index] = itemToBag;
        currentBagItem = null;
        foreach (var statistics in AStarPathFinding.Instance.characterSelected.characterData.statistics)
        {
            statistics.Value.RefreshValue();
        }
        StartCoroutine(ReloadItems());
        BackToBagItems();
    }
    public bool ContainsOtherWeapon(out CharacterData.CharacterItem weapon)
    {
        foreach (KeyValuePair<int, CharacterData.CharacterItem> item in AStarPathFinding.Instance.characterSelected.characterData.items)
        {
            if (item.Value.itemBaseSO)
            {                
                if (item.Value.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Weapon ||
                    item.Value.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Weapon)
                {
                    weapon = item.Value;
                    return true;
                }
            }
        }
        weapon = null;
        return false;
    }
    public void BackToBagItems()
    {
        bagBanners.ElementAt(index).Value.gameObject.GetComponent<Button>().interactable = true;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(bagBanners.ElementAt(index).Value.gameObject);
        bagBanners.ElementAt(index).Value.onObjectSelect.ScrollTo(index);
    }
    public IEnumerator ReloadItems()
    {
        yield return null;
        yield return ReloadGearBanners();
        yield return ReloadBagBanners();
        playerManager.menuCharacterInfo.ReloadInfo(AStarPathFinding.Instance.characterSelected, true);
    }
    public IEnumerator EnableMenu()
    {
        yield return ReloadGearBanners();
        yield return ReloadBagBanners();
        index = 0;
        if (bagBanners.Count > 0)
        {
            if (index > bagBanners.Count - 1)
            {
                index--;
            }
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(bagBanners.ElementAt(index).Value.gameObject);
            bagBanners.ElementAt(index).Value.onObjectSelect.ScrollTo(index);
            yield return null;
            playerManager.menuCharacterInfo.ReloadInfo(AStarPathFinding.Instance.characterSelected, true);
            playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(true);
        }
        playerManager.menuCharacterActions.menuCharacterActions.SetActive(false);
        menuItemCharacters.SetActive(true);
    }
    public void DisableMenu()
    {
        menuItemCharacters.SetActive(false);
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Item);
    }
}
