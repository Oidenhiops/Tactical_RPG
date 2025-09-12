using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuLiftCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public SerializedDictionary<Character, LiftCharactersBanner> banners = new SerializedDictionary<Character, LiftCharactersBanner>();
    public RectTransform containerBanners;
    public GameObject menuLiftCharacter;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public Character[] characters;
    public bool isMenuActive;
    public bool isThrowingCharacter;
    public int index;
    public async Task SpawnBanners()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            LiftCharactersBanner characterBanner = Instantiate(Resources.Load<GameObject>("Prefabs/UI/LiftCharactersBanner/LiftCharactersBanner"), containerBanners).GetComponent<LiftCharactersBanner>();
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
    public Character[] SortCharacters(Character[] charactersToSort)
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
        menuLiftCharacter.SetActive(false);
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.positionInGrid));
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<Character, LiftCharactersBanner>();
        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions.Add(new ActionsManager.ActionInfo
            {
                character = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Lift,
                otherCharacterInfo = new List<ActionsManager.OtherCharacterInfo>
                {
                    new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                }
            });
        }
        else
        {
            playerManager.actionsManager.characterActions.Add(
                AStarPathFinding.Instance.characterSelected,
                new List<ActionsManager.ActionInfo> {
                    new ActionsManager.ActionInfo{
                        character = AStarPathFinding.Instance.characterSelected,
                        typeAction = ActionsManager.TypeAction.Lift,
                        otherCharacterInfo = new List<ActionsManager.OtherCharacterInfo>
                        {
                            new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                        }
                    }
                }
            );
        }
        if (banner.character.characterAnimations.currentAnimation.name != "Lift")
        {
            banner.character.characterAnimations.MakeAnimation("Lifted");
        }
        AStarPathFinding.Instance.characterSelected.characterAnimations.MakeAnimation("Lift");
        AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(banner.character.transform.position)].hasCharacter = null;
        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actionsFinded))
        {
            actionsFinded[actionsFinded.Count - 1].otherCharacterInfo[0].character.transform.position = AStarPathFinding.Instance.characterSelected.positionInGrid + Vector3Int.up;
            actionsFinded[actionsFinded.Count - 1].otherCharacterInfo[0].character.transform.SetParent(AStarPathFinding.Instance.characterSelected.transform);
        }
        AStarPathFinding.Instance.characterSelected.lastAction = ActionsManager.TypeAction.Lift;
        AStarPathFinding.Instance.characterSelected = null;
    }
    public IEnumerator EnableMenu()
    {
        yield return SpawnBanners();
        index = 0;
        if (banners.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (index > banners.Count - 1)
            {
                index--;
            }
            EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
            banners.ElementAt(index).Value.onObjectSelect.ScrollTo(index);
            yield return null;
            playerManager.menuCharacterInfo.ReloadInfo(banners.ElementAt(index).Value.character);
        }
        yield return null;
        isMenuActive = true;
        playerManager.menuCharacterActions.DisableMenu(true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
        menuLiftCharacter.SetActive(true);
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(banners.ElementAt(0).Key.transform.position));
    }
    public void DisableMenu()
    {
        menuLiftCharacter.SetActive(false);
        isMenuActive = false;
        playerManager.menuCharacterActions.menuCharacterActions.SetActive(true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(true);
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<Character, LiftCharactersBanner>();
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Lift);
        playerManager.MovePointerToInstant(AStarPathFinding.Instance.characterSelected.positionInGrid);
    }
}
