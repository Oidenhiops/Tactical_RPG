using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllCharactersBanner : MonoBehaviour, ISelectHandler
{
    public OnObjectSelect onObjectSelect;
    public MenuAllCharacters menuAllCharacters;
    public Character character;
    public Image characterImage;
    public TMP_Text characterName;
    public void SetBannerData(Character character)
    {
        characterImage.sprite = character.initialDataSO.icon;
        characterName.text = character.characterData.name;
    }
    public void OnHandleSelect(BaseEventData eventData)
    {
        menuAllCharacters.OnCharacterSelect(this);
        menuAllCharacters.playerManager.MovePointerToInstant(Vector3Int.RoundToInt(character.transform.position));
    }
}
