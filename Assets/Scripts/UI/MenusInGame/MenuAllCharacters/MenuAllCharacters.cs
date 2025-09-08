using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAllCharacters : MonoBehaviour
{
    public PlayerManager playerManager;
    public SerializedDictionary<Character, AllCharactersBanner> banners = new SerializedDictionary<Character, AllCharactersBanner>();
    public RectTransform containerBanners;
    public GameObject menuAllCharacters;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public int index;
    public async Task SpawnBanners()
    {
        Character[] characters = SortCharacters(FindObjectsByType<Character>(FindObjectsSortMode.None));

        for (int i = 0; i < characters.Length; i++)
        {
            AllCharactersBanner characterBanner = Instantiate(Resources.Load<GameObject>("Prefabs/UI/AllCharactersBanner/AllCharactersBanner"), containerBanners).GetComponent<AllCharactersBanner>();
            characterBanner.menuAllCharacters = this;
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
    public void OnCharacterSelect(AllCharactersBanner banner)
    {
        playerManager.menuCharacterInfo.ReloadInfo(banner.character);
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
            playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(true);
        }
        playerManager.menuGeneralActions.menuGeneralActions.SetActive(false);
        menuAllCharacters.SetActive(true);
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(banners.ElementAt(0).Key.transform.position));
    }
    public void DisableMenu()
    {
        menuAllCharacters.SetActive(false);
        playerManager.menuGeneralActions.menuGeneralActions.SetActive(true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<Character, AllCharactersBanner>();
        playerManager.menuGeneralActions.BackToMenuWhitButton(playerManager.menuGeneralActions.charactersButton);
    }
}
