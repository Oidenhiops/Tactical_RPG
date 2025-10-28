using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuLiftCharacter : MonoBehaviour
{
    public BattlePlayerManager playerManager;
    public SerializedDictionary<CharacterBase, LiftCharactersBanner> banners = new SerializedDictionary<CharacterBase, LiftCharactersBanner>();
    public RectTransform containerBanners;
    public GameObject menuLiftCharacter;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public CharacterBase[] characters;
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positionsToLift;
    public Color gridColor;
    public bool isMenuActive;
    public GameObject liftCharactersBannerPrefab;
    public int index;
    public StatusEffectBaseSO statusEffectLiftSO;
    public async Awaitable SpawnBanners()
    {
        try
        {
            for (int i = 0; i < characters.Length; i++)
            {
                LiftCharactersBanner characterBanner = Instantiate(liftCharactersBannerPrefab, containerBanners).GetComponent<LiftCharactersBanner>();
                characterBanner.menuLiftCharacter = this;
                characterBanner.onObjectSelect.container = containerBanners;
                characterBanner.onObjectSelect.scrollRect = ScrollRect;
                characterBanner.onObjectSelect.viewport = viewport;
                characterBanner.SetBannerData(characters[i]);
                characterBanner.name = characters[i].characterData.name;
                characterBanner.character = characters[i];
                banners.Add(characters[i], characterBanner);
            }
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public CharacterBase[] SortCharacters(CharacterBase[] charactersToSort)
    {
        return charactersToSort
            .OrderBy(c =>
            {
                Match match = Regex.Match(c.gameObject.name, @"\d+");
                if (match.Success)
                    return int.Parse(match.Value);
                return int.MaxValue;
            })
            .ToArray();
    }
    public void OnCharacterSelect(LiftCharactersBanner banner)
    {
        if (!GameManager.Instance.isPause)
        {
            foreach (Transform child in containerBanners.transform)
            {
                Destroy(child.gameObject);
            }
            banners = new SerializedDictionary<CharacterBase, LiftCharactersBanner>();
            if (playerManager.actionsManager.characterActions.TryGetValue(playerManager.aStarPathFinding.characterSelected, out List<ActionsManager.ActionInfo> actions))
            {
                actions.Add(new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Lift,
                    characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                {
                    new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                }
                });
                playerManager.actionsManager.characterFinalActions.Add(playerManager.aStarPathFinding.characterSelected, new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Lift,
                    characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                {
                    new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                }
                });
            }
            else
            {
                playerManager.actionsManager.characterActions.Add(
                    playerManager.aStarPathFinding.characterSelected,
                    new List<ActionsManager.ActionInfo> {
                    new ActionsManager.ActionInfo{
                        characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                        typeAction = ActionsManager.TypeAction.Lift,
                        characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                        {
                            new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                        }
                    }
                    }
                );
                playerManager.actionsManager.characterFinalActions.Add(playerManager.aStarPathFinding.characterSelected, new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Lift,
                    characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                {
                    new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                }
                });
            }
            if (banner.character.characterAnimations.currentAnimation.name != "Lift")
            {
                banner.character.characterAnimations.MakeAnimation("Lifted");
            }
            playerManager.aStarPathFinding.characterSelected.characterAnimations.MakeAnimation("Lift");
            playerManager.aStarPathFinding.characterSelected.characterStatusEffect.statusEffects.Add(statusEffectLiftSO, 0);
            playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(banner.character.transform.position)].hasCharacter = null;
            if (playerManager.actionsManager.characterActions.TryGetValue(playerManager.aStarPathFinding.characterSelected, out List<ActionsManager.ActionInfo> actionsFinded))
            {
                actionsFinded[actionsFinded.Count - 1].characterToMakeAction[0].character.transform.position = playerManager.aStarPathFinding.characterSelected.positionInGrid + Vector3Int.up;
                actionsFinded[actionsFinded.Count - 1].characterToMakeAction[0].character.transform.SetParent(playerManager.aStarPathFinding.characterSelected.transform);
            }
            playerManager.aStarPathFinding.characterSelected.lastAction = ActionsManager.TypeAction.Lift;
            _ = DisableMenuAfterCharacterSelect();
        }
    }
    public async Awaitable EnableMenu()
    {
        try
        {
            await SpawnBanners();
            playerManager.aStarPathFinding.EnableGrid(positionsToLift, gridColor);
            index = 0;
            if (banners.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(null);
                if (index > banners.Count - 1)
                {
                    index--;
                }
                EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
                _ = playerManager.menuCharacterInfo.ReloadInfo(banners.ElementAt(index).Value.character);
                banners.ElementAt(index).Value.onObjectSelect.ScrollTo(index);
                await Awaitable.NextFrameAsync();
            }
            await Awaitable.NextFrameAsync();
            isMenuActive = true;
            _ = playerManager.menuCharacterActions.DisableMenu(true, true);
            menuLiftCharacter.SetActive(true);
            playerManager.MovePointerToInstant(Vector3Int.RoundToInt(banners.ElementAt(0).Key.transform.position));
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
            playerManager.aStarPathFinding.DisableGrid();
            menuLiftCharacter.SetActive(false);
            isMenuActive = false;
            foreach (Transform child in containerBanners.transform)
            {
                Destroy(child.gameObject);
            }
            banners = new SerializedDictionary<CharacterBase, LiftCharactersBanner>();
            playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Lift);
            playerManager.MovePointerToInstant(playerManager.aStarPathFinding.characterSelected.positionInGrid);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable DisableMenuAfterCharacterSelect()
    {
        try
        {
            await Awaitable.NextFrameAsync();
            playerManager.aStarPathFinding.DisableGrid();
            menuLiftCharacter.SetActive(false);
            playerManager.menuCharacterInfo.DisableMenu(true);
            isMenuActive = false;
            foreach (Transform child in containerBanners.transform)
            {
                Destroy(child.gameObject);
            }
            playerManager.actionsManager.EnableMobileInputs();
            banners = new SerializedDictionary<CharacterBase, LiftCharactersBanner>();
            playerManager.MovePointerToInstant(playerManager.aStarPathFinding.characterSelected.positionInGrid);
            playerManager.aStarPathFinding.characterSelected = null;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
