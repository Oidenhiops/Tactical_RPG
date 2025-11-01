using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuItemsCharacter : MonoBehaviour
{
    public GameObject panelGear;
    public GameObject panelBag;
    public BattlePlayerManager playerManager;
    public GameObject menuItemCharacters;
    public int bagIndex;
    public int gearIndex;
    public SerializedDictionary<int, BannerItemsCharacter> gearBanners = new SerializedDictionary<int, BannerItemsCharacter>();
    public SerializedDictionary<int, BannerItemsCharacter> bagBanners = new SerializedDictionary<int, BannerItemsCharacter>();
    public BannerItemsCharacter currentBagItem;
    public async Awaitable ReloadGearBanners()
    {
        try
        {
            var character = playerManager.aStarPathFinding.characterSelected;
            foreach (KeyValuePair<CharacterData.CharacterItemInfo, CharacterData.CharacterItem> item in character.characterData.items)
            {
                if (item.Value.itemBaseSO)
                {
                    gearBanners[item.Key.index].SetBannerData(character.characterData.items[item.Key]);
                    gearBanners[item.Key.index].EnableBanner();
                }
                else
                {
                    gearBanners[item.Key.index].item = new CharacterData.CharacterItem();
                    gearBanners[item.Key.index].DisableBanner();
                }
            }
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable ReloadBagBanners()
    {
        try
        {
            for (int i = 0; i < bagBanners.Count; i++)
            {
                if (GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].bagItems[i].itemBaseSO)
                {
                    bagBanners[i].SetBannerData(GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].bagItems[i]);
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
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void OnItemBagSelect(BannerItemsCharacter bagItem)
    {
        if (!GameManager.Instance.isPause)
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            currentBagItem = bagItem;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(gearBanners.ElementAt(gearIndex).Value.gameObject);
            bagItem.GetComponentInChildren<Button>().interactable = false;
            TogglePanels(true);
        }
    }
    public void OnItemSelect(BannerItemsCharacter gearItem)
    {
        if (!GameManager.Instance.isPause)
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);

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

            if (gearBanners[gearIndex].bannerInfo.typeCharacterItem == CharacterData.TypeCharacterItem.Weapon)
            {
                if (!itemToGear.itemBaseSO)
                {
                    SetItemData(itemToBag, itemToGear);
                }
                else
                {
                    if (itemToGear.itemBaseSO.typeObject == ItemBaseSO.TypeObject.Weapon)
                    {
                        if (playerManager.aStarPathFinding.characterSelected.initialDataSO.isHumanoid)
                        {
                            if (itemToGear.itemBaseSO.typeWeapon != ItemBaseSO.TypeWeapon.Monster)
                            {
                                SetItemData(itemToBag, itemToGear);
                            }
                        }
                        else
                        {
                            if (itemToGear.itemBaseSO.typeWeapon == ItemBaseSO.TypeWeapon.Monster)
                            {
                                SetItemData(itemToBag, itemToGear);
                            }
                        }
                    }
                }
            }
            else
            {
                if (!itemToGear.itemBaseSO)
                {
                    SetItemData(itemToBag, itemToGear);
                }
                else
                {
                    if (itemToGear.itemBaseSO.typeObject != ItemBaseSO.TypeObject.Weapon)
                    {
                        SetItemData(itemToBag, itemToGear);
                    }
                }
            }
        }
    }
    public void SetItemData(CharacterData.CharacterItem itemToBag, CharacterData.CharacterItem itemToGear)
    {
        if (itemToBag.itemBaseSO)
        {
            itemToBag.itemBaseSO.DesEquipItem(playerManager.aStarPathFinding.characterSelected, itemToBag);
        }
        if (itemToGear.itemBaseSO)
        {
            itemToGear.itemBaseSO.EquipItem(playerManager.aStarPathFinding.characterSelected, itemToGear);
        }
        foreach (KeyValuePair<CharacterData.CharacterItemInfo, CharacterData.CharacterItem> item in playerManager.aStarPathFinding.characterSelected.characterData.items)
        {
            if (item.Key.index == gearIndex)
            {
                playerManager.aStarPathFinding.characterSelected.characterData.items[item.Key] = itemToGear;
                break;
            }
        }
        GameData.Instance.gameDataInfo.gameDataSlots[GameData.Instance.systemDataInfo.currentGameDataIndex].bagItems[currentBagItem.bannerInfo.index] = itemToBag;
        currentBagItem = null;
        foreach (var statistics in playerManager.aStarPathFinding.characterSelected.characterData.statistics)
        {
            statistics.Value.RefreshValue();
        }
        playerManager.aStarPathFinding.GetTilesToAttack(out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions);
        playerManager.menuCharacterActions.SendCharactersToAttack(positions);
        TogglePanels(false);
        _= ReloadItems();
        BackToBagItems();
    }
    public void TogglePanels(bool panelBagIsActive)
    {
        panelBag.SetActive(panelBagIsActive);
        panelGear.SetActive(!panelBagIsActive);
    }
    public void BackToBagItems()
    {
        if (!GameManager.Instance.isPause)
        {
            TogglePanels(false);
            currentBagItem = null;
            bagBanners.ElementAt(bagIndex).Value.gameObject.GetComponent<Button>().interactable = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(bagBanners.ElementAt(bagIndex).Value.gameObject);
            bagBanners.ElementAt(bagIndex).Value.onObjectSelect.ScrollTo(bagIndex);
        }
    }
    public async Awaitable ReloadItems()
    {
        try
        {
            await Awaitable.NextFrameAsync();
            await ReloadGearBanners();
            await ReloadBagBanners();
            _ = playerManager.menuCharacterInfo.ReloadInfo(playerManager.aStarPathFinding.characterSelected, true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable EnableMenu()
    {
        try
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TouchButtonAdvance"), 1, true);
            await ReloadGearBanners();
            await ReloadBagBanners();
            bagIndex = 0;
            if (bagBanners.Count > 0)
            {
                if (bagIndex > bagBanners.Count - 1)
                {
                    bagIndex--;
                }
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(bagBanners.ElementAt(bagIndex).Value.gameObject);
                bagBanners.ElementAt(bagIndex).Value.onObjectSelect.ScrollTo(bagIndex);
                await Awaitable.NextFrameAsync();
                _ = playerManager.menuCharacterInfo.ReloadInfo(playerManager.aStarPathFinding.characterSelected, true);
                playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(true);
            }
            playerManager.menuCharacterActions.menuCharacterActions.SetActive(false);
            menuItemCharacters.SetActive(true);
            TogglePanels(false);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable DisableMenu()
    {
        try
        {
            await Awaitable.NextFrameAsync();
            menuItemCharacters.SetActive(false);
            gearIndex = 0;
            bagIndex = 0;
            playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Item);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}