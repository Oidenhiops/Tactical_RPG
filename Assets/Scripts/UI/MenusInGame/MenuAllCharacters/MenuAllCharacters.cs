using System;
using System.Linq;
using System.Text.RegularExpressions;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAllCharacters : MonoBehaviour
{
    public BattlePlayerManager playerManager;
    public SerializedDictionary<CharacterBase, AllCharactersBanner> banners = new SerializedDictionary<CharacterBase, AllCharactersBanner>();
    public RectTransform containerBanners;
    public GameObject menuAllCharacters;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public GameObject allCharactersBannerPrefab;
    public int index;
    public async Awaitable SpawnBanners()
    {
        try
        {
            CharacterBase[] characters = SortCharacters(FindObjectsByType<CharacterBase>(FindObjectsSortMode.None));

            for (int i = 0; i < characters.Length; i++)
            {
                AllCharactersBanner characterBanner = Instantiate(allCharactersBannerPrefab, containerBanners).GetComponent<AllCharactersBanner>();
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
    public void OnCharacterSelect(AllCharactersBanner banner)
    {
        if (!GameManager.Instance.isPause) _= playerManager.menuCharacterInfo.ReloadInfo(banner.character);
    }
    public async Awaitable EnableMenu()
    {
        try
        {
            await SpawnBanners();
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
                await Awaitable.NextFrameAsync();
                _ = playerManager.menuCharacterInfo.ReloadInfo(banners.ElementAt(index).Value.character);
                playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(true);
            }
            playerManager.menuGeneralActions.menuGeneralActions.SetActive(false);
            menuAllCharacters.SetActive(true);
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
            menuAllCharacters.SetActive(false);
            playerManager.menuGeneralActions.menuGeneralActions.SetActive(true);
            playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
            foreach (Transform child in containerBanners.transform)
            {
                Destroy(child.gameObject);
            }
            banners = new SerializedDictionary<CharacterBase, AllCharactersBanner>();
            playerManager.menuGeneralActions.BackToMenuWhitButton(playerManager.menuGeneralActions.charactersButton);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
