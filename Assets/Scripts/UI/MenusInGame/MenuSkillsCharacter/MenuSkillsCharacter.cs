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
    public GameObject mobileJoystick;
    public bool isMenuActive;
    public int index;
    public bool canMovePointer = false;
    public SkillCharacterBanner currentSkill;
    void OnEnable()
    {
        playerManager.characterActions.CharacterInputs.Interact.started += OnSelectCharacterHandle;
        playerManager.characterActions.CharacterInputs.Interact.started += OnHandleSelectSkill;
        playerManager.characterActions.CharacterInputs.RotateSkill.started += OnRotateSkillFormHandle;
    }
    void OnDisable()
    {
        playerManager.characterActions.CharacterInputs.Interact.started -= OnSelectCharacterHandle;
        playerManager.characterActions.CharacterInputs.Interact.started -= OnHandleSelectSkill;
        playerManager.characterActions.CharacterInputs.RotateSkill.started -= OnRotateSkillFormHandle;
    }
    public async Task SpawnBanners()
    {
        CharacterBase character = AStarPathFinding.Instance.characterSelected;
        if (character.characterData.skills.Count > 0)
        {
            skillDescription.transform.parent.gameObject.SetActive(true);
            foreach (KeyValuePair<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>> typeSkill in character.characterData.skills[ItemBaseSO.TypeWeapon.None])
            {
                foreach (KeyValuePair<string, CharacterData.CharacterSkillInfo> skill in typeSkill.Value)
                {
                    SkillCharacterBanner banner = Instantiate(skillCharacterBanner, containerBanners.transform).GetComponent<SkillCharacterBanner>();
                    banner.onObjectSelect.container = containerBanners;
                    banner.onObjectSelect.viewport = viewport;
                    banner.onObjectSelect.scrollRect = scrollRect;
                    banner.SetBannerData(skill.Value);
                    banner.menuSkillsCharacter = this;
                    banner.canUserSkill = skill.Value.skillsBaseSO.ValidateCanUseSkill(character);
                    banner.skillBg.color = banner.canUserSkill ? Color.white : Color.gray;
                    banners.Add(skill.Value.skillsBaseSO, banner.GetComponent<SkillCharacterBanner>());
                }
            }
            character.characterData.GetCurrentWeapon(out CharacterData.CharacterItem weapon);

            if (weapon != null && character.characterData.skills.ContainsKey(weapon.itemBaseSO.typeWeapon))
            {
                foreach (KeyValuePair<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>> typeSkill in character.characterData.skills[weapon.itemBaseSO.typeWeapon])
                {
                    foreach (KeyValuePair<string, CharacterData.CharacterSkillInfo> skill in typeSkill.Value)
                    {
                        SkillCharacterBanner banner = Instantiate(skillCharacterBanner, containerBanners.transform).GetComponent<SkillCharacterBanner>();
                        banner.onObjectSelect.container = containerBanners;
                        banner.onObjectSelect.viewport = viewport;
                        banner.onObjectSelect.scrollRect = scrollRect;
                        banner.SetBannerData(skill.Value);
                        banner.menuSkillsCharacter = this;
                        banner.canUserSkill = skill.Value.skillsBaseSO.ValidateCanUseSkill(character);
                        banner.skillBg.color = banner.canUserSkill ? Color.white : Color.gray;
                        banners.Add(skill.Value.skillsBaseSO, banner.GetComponent<SkillCharacterBanner>());
                    }
                }
            }
            currentSkill = banners.ElementAt(0).Value;
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
    public void OnSkillSelect()
    {
        if (!GameManager.Instance.isPause)
        {
            AStarPathFinding.Instance.GetPositionsToUseSkill(currentSkill.skill.skillsBaseSO, out SerializedDictionary<Vector3Int, GenerateMap.WalkablePositionInfo> positions);
            AStarPathFinding.Instance.EnableGrid(positions, Color.blue);
            AStarPathFinding.Instance.EnableSubGrid(currentSkill.skill.skillsBaseSO.positionsSkillForm, Color.red);
            canMovePointer = currentSkill.skill.skillsBaseSO.isFreeMovementSkill;
            menuSkillSelectSkill.SetActive(false);
            mobileJoystick.SetActive(true);
        }
    }
    public void OnSelectCharacterHandle(InputAction.CallbackContext context)
    {
        if (isMenuActive && !menuSkillSelectSkill.activeSelf && !playerManager.isDecalMovement && !GameManager.Instance.isPause)
        {
            bool characterFinded = false;
            if (!currentSkill.skill.skillsBaseSO.needCharacterToMakeSkill)
            {
                characterFinded = true;
            }
            else
            {
                foreach (KeyValuePair<Vector3Int, GameObject> pos in AStarPathFinding.Instance.currentSubGrid)
                {
                    AStarPathFinding.Instance.GetHighestBlockAt(new Vector3Int(Mathf.RoundToInt(pos.Value.transform.position.x), 0, Mathf.RoundToInt(pos.Value.transform.position.z)), out GenerateMap.WalkablePositionInfo block);
                    if (block != null && AStarPathFinding.Instance.grid.ContainsKey(block.pos) && AStarPathFinding.Instance.grid[block.pos].hasCharacter)
                    {
                        characterFinded = true;
                        break;
                    }
                }
            }
            if (characterFinded)
            {
                _= DisableMenuAfterSelectCharacterToMakeSkill();
            }
        }
    }
    private void OnRotateSkillFormHandle(InputAction.CallbackContext context)
    {
        if (isMenuActive && !menuSkillSelectSkill.activeSelf && !GameManager.Instance.isPause)
        {
            bool direction = context.ReadValue<float>() > 0;
            playerManager.mouseDecal.subGridContainer.rotation = Quaternion.Euler(0, playerManager.mouseDecal.subGridContainer.localRotation.eulerAngles.y + (direction ? 90 : -90), 0);
        }
    }
    public void OnHandleSelectSkill(InputAction.CallbackContext context)
    {
        if (isMenuActive && menuSkillSelectSkill.activeSelf && currentSkill != null && currentSkill.canUserSkill && !GameManager.Instance.isPause) OnSkillSelect();
    }
    public async Task EnableMenu()
    {
        await Awaitable.NextFrameAsync();
        await SpawnBanners();
        index = 0;
        if (banners.Count > 0)
        {
            if (index > banners.Count - 1)
            {
                index--;
            }
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
            await Awaitable.NextFrameAsync();
        }
        await Awaitable.NextFrameAsync();
        isMenuActive = true;
        playerManager.menuCharacterInfo.DisableMenu(true);
        _= playerManager.menuCharacterActions.DisableMenu(true, true);
        menuSkillSelectSkill.SetActive(true);
        menuSkillsCharacter.SetActive(true);
    }
    public async Task DisableMenu()
    {
        await Awaitable.NextFrameAsync();
        index = 0;
        canMovePointer = false;
        menuSkillSelectSkill.SetActive(false);
        menuSkillsCharacter.SetActive(false);
        isMenuActive = false;
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<SkillsBaseSO, SkillCharacterBanner>();
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Skill);
    }
    public async Task DisableMenuForSelectCharacterToMakeSkill()
    {
        await Awaitable.NextFrameAsync();
        canMovePointer = false;
        currentSkill = null;
        menuSkillSelectSkill.SetActive(true);
        mobileJoystick.SetActive(false);
        AStarPathFinding.Instance.DisableGrid();
        AStarPathFinding.Instance.DisableSubGrid();
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(banners.ElementAt(index).Value.gameObject);
    }
    public async Task DisableMenuAfterSelectCharacterToMakeSkill()
    {
        List<Vector3Int> positionsToMakeSkill = new List<Vector3Int>();

        foreach (KeyValuePair<Vector3Int, GameObject> pos in AStarPathFinding.Instance.currentSubGrid)
        {
            positionsToMakeSkill.Add(new Vector3Int(Mathf.RoundToInt(pos.Value.transform.position.x), 0, Mathf.RoundToInt(pos.Value.transform.position.z)));
        }

        if (playerManager.actionsManager.characterActions.TryGetValue(AStarPathFinding.Instance.characterSelected, out List<ActionsManager.ActionInfo> actions))
        {
            actions.Add(new ActionsManager.ActionInfo
            {
                characterMakeAction = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Skill,
                skillInfo = currentSkill.skill,
                positionsToMakeSkill = positionsToMakeSkill
            });
            playerManager.actionsManager.characterFinalActions.Add(AStarPathFinding.Instance.characterSelected, new ActionsManager.ActionInfo
            {
                characterMakeAction = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Skill,
                skillInfo = currentSkill.skill,
                positionsToMakeSkill = positionsToMakeSkill
            });
        }
        else
        {
            playerManager.actionsManager.characterActions.Add(
                AStarPathFinding.Instance.characterSelected,
                new List<ActionsManager.ActionInfo> {
                    new ActionsManager.ActionInfo{
                        characterMakeAction = AStarPathFinding.Instance.characterSelected,
                        typeAction = ActionsManager.TypeAction.Skill,
                        skillInfo = currentSkill.skill,
                        positionsToMakeSkill = positionsToMakeSkill
                    }
                }
            );
            playerManager.actionsManager.characterFinalActions.Add(AStarPathFinding.Instance.characterSelected, new ActionsManager.ActionInfo
            {
                characterMakeAction = AStarPathFinding.Instance.characterSelected,
                typeAction = ActionsManager.TypeAction.Skill,
                skillInfo = currentSkill.skill,
                positionsToMakeSkill = positionsToMakeSkill
            });
        }
        AStarPathFinding.Instance.characterSelected.lastAction = ActionsManager.TypeAction.Skill;

        await Awaitable.NextFrameAsync();
        index = 0;
        canMovePointer = false;
        menuSkillSelectSkill.SetActive(false);
        menuSkillsCharacter.SetActive(false);
        mobileJoystick.SetActive(false);
        AStarPathFinding.Instance.DisableGrid();
        AStarPathFinding.Instance.DisableSubGrid();
        foreach (Transform child in containerBanners.transform)
        {
            Destroy(child.gameObject);
        }
        banners = new SerializedDictionary<SkillsBaseSO, SkillCharacterBanner>();
        playerManager.MovePointerToInstant(Vector3Int.RoundToInt(AStarPathFinding.Instance.characterSelected.transform.position));
        playerManager.actionsManager.EnableMobileInputs();
        AStarPathFinding.Instance.characterSelected = null;
        isMenuActive = false;
    }
}