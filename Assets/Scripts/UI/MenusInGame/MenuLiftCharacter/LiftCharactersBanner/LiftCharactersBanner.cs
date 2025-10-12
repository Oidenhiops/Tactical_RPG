using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LiftCharactersBanner : MonoBehaviour, ISelectHandler
{
    public OnObjectSelect onObjectSelect;
    public MenuLiftCharacter menuLiftCharacter;
    public Character character;
    public Image characterImage;
    public TMP_Text characterName;
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (menuLiftCharacter.isMenuActive)
            {
                menuLiftCharacter.OnCharacterSelect(this);
            }
        });
    }
    public void SetBannerData(Character character)
    {
        characterImage.sprite = character.initialDataSO.icon;
        characterName.text = character.characterData.name;
    }
    public void OnHandleSelect(BaseEventData eventData)
    {
        menuLiftCharacter.playerManager.MovePointerToInstant(Vector3Int.RoundToInt(character.transform.position));
        menuLiftCharacter.playerManager.menuCharacterInfo.ReloadInfo(character);
    }
}
