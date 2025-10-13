using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuSkillsCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public SerializedDictionary<SkillsBaseSO, SkillCharacterBanner> banners = new SerializedDictionary<SkillsBaseSO, SkillCharacterBanner>();
    public GameObject menuSkillsCharacter;
    public GameObject menuSkillSelectSkill;
    public RectTransform containerBanners;
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public GameObject skillCharacterBanner;
    public ManagementLanguage skillDescription;
    public bool isMenuActive;
    public int index;
    public InputAction selectCharacterInput;
    public InputAction rotateSkillFormInput;
    public bool canMovePointer = false;
    public SkillCharacterBanner currentSkill;
    void OnEnable()
    {
        selectCharacterInput.Enable();
        selectCharacterInput.performed += OnSelectCharacterHandle;
        rotateSkillFormInput.Enable();
        rotateSkillFormInput.performed += OnRotateSkillFormHandle;
    }
    void OnDisable()
    {
        selectCharacterInput.Disable();
        selectCharacterInput.performed -= OnSelectCharacterHandle;
        rotateSkillFormInput.Disable();
        rotateSkillFormInput.performed -= OnRotateSkillFormHandle;
    }
    public async Task SpawnBanners()
    {
        Character character = AStarPathFinding.Instance.characterSelected;
        if (character.characterData.skills.Count > 0)
        {
            skillDescription.transform.parent.gameObject.SetActive(true);
            foreach (KeyValuePair<int, CharacterData.CharacterSkillInfo> skill in character.characterData.skills[ItemBaseSO.TypeWeapon.None])
            {
                SkillCharacterBanner banner = Instantiate(skillCharacterBanner, containerBanners.transform).GetComponent<SkillCharacterBanner>();
                banner.onObjectSelect.container = containerBanners;
                banner.onObjectSelect.viewport = viewport;
                banner.onObjectSelect.scrollRect = scrollRect;
                banner.SetBannerData(skill.Value);
                banner.menuSkillsCharacter = this;
                banners.Add(skill.Value.skillsBaseSO, banner.GetComponent<SkillCharacterBanner>());
            }
            character.characterData.GetCurrentWeapon(out CharacterData.CharacterItem weapon);

            if (weapon != null && character.characterData.skills.ContainsKey(weapon.itemBaseSO.typeWeapon))
            {
                foreach (KeyValuePair<int, CharacterData.CharacterSkillInfo> skill in character.characterData.skills[weapon.itemBaseSO.typeWeapon])
                {
                    SkillCharacterBanner banner = Instantiate(skillCharacterBanner, containerBanners.transform).GetComponent<SkillCharacterBanner>();
                    banner.onObjectSelect.container = containerBanners;
                    banner.onObjectSelect.viewport = viewport;
                    banner.onObjectSelect.scrollRect = scrollRect;
                    banner.SetBannerData(skill.Value);
                    banner.menuSkillsCharacter = this;
                    banners.Add(skill.Value.skillsBaseSO, banner.GetComponent<SkillCharacterBanner>());
                }
            }
            SetDescriptionData(banners.ElementAt(0).Value);
        }
        await Awaitable.NextFrameAsync();
    }
    public void SetDescriptionData(SkillCharacterBanner skillCharacterBanner)
    {
        skillDescription.id = skillCharacterBanner.skill.skillsBaseSO.skillIdText;
        skillDescription.otherInfo = skillCharacterBanner.skill.skillsBaseSO.GetSkillDescription(skillCharacterBanner.skill.statistics);
        skillDescription.RefreshDescription();
    }
    public void OnSkillSelect(SkillCharacterBanner banner)
    {
        AStarPathFinding.Instance.GetPositionsToUseSkill(banner.skill.skillsBaseSO, out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions);
        AStarPathFinding.Instance.EnableGrid(positions, Color.blue);
        AStarPathFinding.Instance.EnableSubGrid(banner.skill.skillsBaseSO.positionsSkillForm, Color.red);
        canMovePointer = banner.skill.skillsBaseSO.isFreeMovementSkill;
        menuSkillSelectSkill.SetActive(false);
    }
    public void OnSelectCharacterHandle(InputAction.CallbackContext context)
    {
        if (isMenuActive && !menuSkillSelectSkill.activeSelf)
        {
            bool characterFinded = false;
            foreach (KeyValuePair<Vector3Int, GameObject> pos in AStarPathFinding.Instance.currentSubGrid)
            {
                if (AStarPathFinding.Instance.grid.ContainsKey(pos.Key) && AStarPathFinding.Instance.grid[pos.Key].hasCharacter)
                {
                    characterFinded = true;
                    break;
                }
            }
            if (characterFinded)
            {
                DisableMenuAfterSelectCharacterToMakeSkill();
            }
        }
    }
    private void OnRotateSkillFormHandle(InputAction.CallbackContext context)
    {
        if (isMenuActive && !menuSkillSelectSkill.activeSelf)
        {
            bool direction = context.ReadValue<float>() > 0;
            playerManager.mouseDecal.subGridContainer.rotation = Quaternion.Euler(0, playerManager.mouseDecal.subGridContainer.localRotation.eulerAngles.y + (direction ? 90 : -90), 0);
        }
    }
    public IEnumerator EnableMenu()
    {
        yield return SpawnBanners();
        index = 0;
        if (banners.Count > 0)
        {
            if (index > banners.Count - 1)
            {
                index--;
            }
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
            yield return null;
        }
        yield return null;
        isMenuActive = true;
        playerManager.menuCharacterInfo.DisableMenu(true);
        playerManager.menuCharacterActions.DisableMenu(true, true);
        menuSkillSelectSkill.SetActive(true);
        menuSkillsCharacter.SetActive(true);
    }
    public void DisableMenu()
    {
        index = 0;
        canMovePointer = false;
        menuSkillSelectSkill.SetActive(false);
        menuSkillsCharacter.SetActive(false);
        AStarPathFinding.Instance.DisableGrid();
        AStarPathFinding.Instance.DisableSubGrid();
        isMenuActive = false;
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<SkillsBaseSO, SkillCharacterBanner>();
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Skill);
    }
    public void DisableMenuForSelectCharacterToMakeSkill()
    {
        canMovePointer = false;
        menuSkillSelectSkill.SetActive(true);
        AStarPathFinding.Instance.DisableGrid();
        AStarPathFinding.Instance.DisableSubGrid();
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
    }
    public void DisableMenuAfterSelectCharacterToMakeSkill()
    {
        List<Vector3Int> positionsToMakeSkill = new List<Vector3Int>();

        foreach (KeyValuePair<Vector3Int, GameObject> pos in AStarPathFinding.Instance.currentSubGrid)
        {
            positionsToMakeSkill.Add(new Vector3Int(pos.Key.x, 0, pos.Key.z));
        }

        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions.Add(new ActionsManager.ActionInfo
            {
                character = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Skill,
                positionsToMakeSkill = positionsToMakeSkill
            });
            playerManager.actionsManager.characterFinalActions.Add(AStarPathFinding.Instance.characterSelected, new ActionsManager.ActionInfo
            {
                character = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Skill,
                positionsToMakeSkill = positionsToMakeSkill
            });
        }
        else
        {
            playerManager.actionsManager.characterActions.Add(
                AStarPathFinding.Instance.characterSelected,
                new List<ActionsManager.ActionInfo> {
                    new ActionsManager.ActionInfo{
                        character = AStarPathFinding.Instance.characterSelected,
                        typeAction = ActionsManager.TypeAction.Skill,
                        positionsToMakeSkill = positionsToMakeSkill
                    }
                }
            );
            playerManager.actionsManager.characterFinalActions.Add(AStarPathFinding.Instance.characterSelected, new ActionsManager.ActionInfo
            {
                character = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Skill,
                positionsToMakeSkill = positionsToMakeSkill
            });
        }
        AStarPathFinding.Instance.characterSelected.lastAction = ActionsManager.TypeAction.Skill;
        
        index = 0;
        canMovePointer = false;
        menuSkillSelectSkill.SetActive(false);
        menuSkillsCharacter.SetActive(false);
        AStarPathFinding.Instance.DisableGrid();
        AStarPathFinding.Instance.DisableSubGrid();
        isMenuActive = false;
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<SkillsBaseSO, SkillCharacterBanner>();
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position));
        playerManager.actionsManager.EnableMobileInputs();
        AStarPathFinding.Instance.characterSelected = null;
    }
}