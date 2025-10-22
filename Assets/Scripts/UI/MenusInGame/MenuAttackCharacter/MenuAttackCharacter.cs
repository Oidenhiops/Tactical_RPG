using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAttackCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public SerializedDictionary<CharacterBase, AttackCharacterBanner> banners = new SerializedDictionary<CharacterBase, AttackCharacterBanner>();
    public RectTransform containerBanners;
    public GameObject menuAttackCharacter;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public CharacterBase[] characters;
    public SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positionsToAttack;
    public Color gridColor;
    public bool isMenuActive;
    public GameObject attackCharactersBannerPrefab;
    public int index;
    public async Task SpawnBanners()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            AttackCharacterBanner characterBanner = Instantiate(attackCharactersBannerPrefab, containerBanners).GetComponent<AttackCharacterBanner>();
            characterBanner.menuAttackCharacter = this;
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
    public void OnCharacterSelect(AttackCharacterBanner banner)
    {
        if (!GameManager.Instance.isPause)
        {
            foreach (Transform child in containerBanners.transform)
            {
                Destroy(child.gameObject);
            }
            banners = new SerializedDictionary<CharacterBase, AttackCharacterBanner>();
            if (playerManager.actionsManager.characterActions.TryGetValue(playerManager.aStarPathFinding.characterSelected, out List<ActionsManager.ActionInfo> actions))
            {
                actions.Add(new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Attack,
                    characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                {
                    new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                }
                });
                playerManager.actionsManager.characterFinalActions.Add(playerManager.aStarPathFinding.characterSelected, new ActionsManager.ActionInfo
                {
                    characterMakeAction = playerManager.aStarPathFinding.characterSelected,
                    typeAction = ActionsManager.TypeAction.Attack,
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
                        typeAction = ActionsManager.TypeAction.Attack,
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
                    typeAction = ActionsManager.TypeAction.Attack,
                    characterToMakeAction = new List<ActionsManager.OtherCharacterInfo>
                {
                    new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                }
                });
            }
            playerManager.aStarPathFinding.characterSelected.lastAction = ActionsManager.TypeAction.Attack;
            _ = DisableMenuAfterCharacterSelect();
        }
    }
    public async Task EnableMenu()
    {
        await SpawnBanners();
        playerManager.aStarPathFinding.EnableGrid(positionsToAttack, gridColor);
        index = 0;
        if (banners.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (index > banners.Count - 1)
            {
                index--;
            }
            EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
            _= playerManager.menuCharacterInfo.ReloadInfo(banners.ElementAt(index).Value.character);
            banners.ElementAt(index).Value.onObjectSelect.ScrollTo(index);
            await Awaitable.NextFrameAsync();
        }
        await Awaitable.NextFrameAsync();
        isMenuActive = true;
        _= playerManager.menuCharacterActions.DisableMenu(true, true);
        menuAttackCharacter.SetActive(true);
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(banners.ElementAt(0).Key.transform.position));
    }
    public async Task DisableMenu()
    {
        await Awaitable.NextFrameAsync();
        playerManager.aStarPathFinding.DisableGrid();
        menuAttackCharacter.SetActive(false);
        isMenuActive = false;
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<CharacterBase, AttackCharacterBanner>();
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Attack);
        playerManager.MovePointerToInstant(playerManager.aStarPathFinding.characterSelected.positionInGrid);
    }
    public async Task DisableMenuAfterCharacterSelect()
    {
        await Awaitable.NextFrameAsync();
        playerManager.aStarPathFinding.DisableGrid();
        menuAttackCharacter.SetActive(false);
        playerManager.menuCharacterInfo.DisableMenu(true);
        isMenuActive = false;
        banners = new SerializedDictionary<CharacterBase, AttackCharacterBanner>();
        playerManager.MovePointerToInstant(playerManager.aStarPathFinding.characterSelected.positionInGrid);
        playerManager.aStarPathFinding.characterSelected = null;
        playerManager.actionsManager.EnableMobileInputs();
    }
}
