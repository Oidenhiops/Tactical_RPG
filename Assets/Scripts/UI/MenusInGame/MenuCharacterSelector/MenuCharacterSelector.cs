using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuCharacterSelector : MonoBehaviour
{
    public PlayerManager playerManager;
    public SerializedDictionary<Character, CharacterSelectorBanner> banners = new SerializedDictionary<Character, CharacterSelectorBanner>();
    public RectTransform containerBanners;
    public ScrollRect ScrollRect;
    public RectTransform viewport;
    public GameObject menuCharacterSelector;
    public TMP_Text amountCharactersText;
    public int index;
    public bool isMenuActive;
    public GameObject characterSelectorBannerPrefab;
    int _amountCharacters;
    Action<int> OnAmountCharacterChange;
    public int amountCharacters
    {
        get => _amountCharacters;
        set
        {
            if (_amountCharacters != value)
            {
                _amountCharacters = value;
                amountCharactersText.text = _amountCharacters.ToString();
                OnAmountCharacterChange?.Invoke(_amountCharacters);
            }
        }
    }
    void Start()
    {
        amountCharacters = 10;
        OnAmountCharacterChange += ValidateBlockBanner;
        ValidateBlockBanner(amountCharacters);
    }
    public void SpawnBanners()
    {
        for (int i = 0; i < playerManager.characters.Count(); i++)
        {
            if (!playerManager.characters[i].gameObject.activeSelf)
            {
                CharacterSelectorBanner characterSelectorBanner = Instantiate(characterSelectorBannerPrefab, containerBanners).GetComponent<CharacterSelectorBanner>();
                characterSelectorBanner.menuCharacterSelector = this;
                characterSelectorBanner.onObjectSelect.container = containerBanners;
                characterSelectorBanner.onObjectSelect.scrollRect = ScrollRect;
                characterSelectorBanner.onObjectSelect.viewport = viewport;
                characterSelectorBanner.SetBannerData(playerManager.characters[i]);
                characterSelectorBanner.name = playerManager.characters[i].characterData.name;
                banners.Add(playerManager.characters[i], characterSelectorBanner);
            }
        }
    }
    public void OnCharacterSelect(GameObject banner)
    {
        banner.SetActive(false);
        amountCharacters--;
        Character character = banners.FirstOrDefault(x => x.Value.gameObject == banner.gameObject).Key;
        character.gameObject.SetActive(true);
        character.transform.position = Vector3.zero;
        AStarPathFinding.Instance.characterSelected = character;
        AStarPathFinding.Instance.grid[Vector3Int.zero].hasCharacter = character;
        AStarPathFinding.Instance.EnableGrid(AStarPathFinding.Instance.GetWalkableTiles(), Color.magenta);
        DisableMenu();
    }
    public IEnumerator EnableMenu()
    {
        SpawnBanners();
        menuCharacterSelector.SetActive(true);
        if (banners.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (index > banners.Count - 1)
            {
                index--;
            }
            EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
            banners.ElementAt(index).Value.onObjectSelect.ScrollTo(index);
        }
        yield return null;
        isMenuActive = true;
    }
    public void DisableMenu()
    {
        menuCharacterSelector.SetActive(false);
        isMenuActive = false;
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<Character, CharacterSelectorBanner>();
    }
    void ValidateBlockBanner(int amountCharacters)
    {
        foreach (KeyValuePair<Character, CharacterSelectorBanner> banner in banners)
        {
            banner.Value.blocker.SetActive(amountCharacters <= 0);
        }
    }
}
