using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttackCharacterBanner : MonoBehaviour
{
    public OnObjectSelect onObjectSelect;
    public MenuAttackCharacter menuAttackCharacter;
    public Character character;
    public Image characterImage;
    public TMP_Text characterName;
    public Button button;
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (menuAttackCharacter.isMenuActive)
            {
                menuAttackCharacter.OnCharacterSelect(this);
            }
        });
    }
    public void SetBannerData(Character character)
    {
        characterImage.sprite = character.initialDataSO.icon;
        characterName.text = character.characterData.name;
    }
    public void OnSelect(BaseEventData eventData)
    {
        menuAttackCharacter.playerManager.MovePointerToInstant(Vector3Int.RoundToInt(character.transform.position));
    }
}
