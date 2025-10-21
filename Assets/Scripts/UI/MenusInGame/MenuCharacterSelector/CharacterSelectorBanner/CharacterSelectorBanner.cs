using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectorBanner : MonoBehaviour, ISelectHandler
{
    public OnObjectSelect onObjectSelect;
    public MenuCharacterSelector menuCharacterSelector;
    public Image characterImage;
    public GameObject blocker;
    public TMP_Text characterName;
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (menuCharacterSelector.amountCharacters - 1 >= 0  && menuCharacterSelector.isMenuActive)
            {
                menuCharacterSelector.OnCharacterSelect(gameObject);
            }
        });
    }
    public void SetBannerData(CharacterBase character)
    {
        characterImage.sprite = character.initialDataSO.icon;
        characterName.text = character.characterData.name;
    }
    public void OnHandleSelect(BaseEventData eventData)
    {
        menuCharacterSelector.index = gameObject.transform.GetSiblingIndex();
    }
}
