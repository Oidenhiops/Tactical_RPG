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
    public SerializedDictionary<Character, AttackCharacterBanner> banners = new SerializedDictionary<Character, AttackCharacterBanner>();
    public RectTransform containerBanners;
    public GameObject menuAttackCharacter;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public Character[] characters;
    public bool isMenuActive;
    public int index;
    public async Task SpawnBanners()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            AttackCharacterBanner characterBanner = Instantiate(Resources.Load<GameObject>("Prefabs/UI/AttackCharactersBanner/AttackCharactersBanner"), containerBanners).GetComponent<AttackCharacterBanner>();
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
    public void OnCharacterSelect(AttackCharacterBanner banner)
    {
        menuAttackCharacter.SetActive(false);
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<Character, AttackCharacterBanner>();
        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions.Add(new ActionsManager.ActionInfo
            {
                character = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Attack,
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
                        typeAction = ActionsManager.TypeAction.Attack,
                        otherCharacterInfo = new List<ActionsManager.OtherCharacterInfo>
                        {
                            new ActionsManager.OtherCharacterInfo(banner.character, Vector3Int.RoundToInt(banner.character.transform.position))
                        }
                    }
                }
            );
        }
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
        menuAttackCharacter.SetActive(true);
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(banners.ElementAt(0).Key.transform.position));
    }
    public void DisableMenu()
    {
        menuAttackCharacter.SetActive(false);
        isMenuActive = false;
        playerManager.menuCharacterActions.menuCharacterActions.SetActive(true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(true);
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<Character, AttackCharacterBanner>();
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Attack);
        playerManager.MovePointerToInstant(AStarPathFinding.Instance.characterSelected.positionInGrid);
    }
}
