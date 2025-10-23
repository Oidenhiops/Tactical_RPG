using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionsManager : MonoBehaviour
{
    public BattlePlayerManager playerManager;
    public Animator roundStateAnimator;
    public ManagementLanguage roundStateLanguage;
    public InputAction endTurnTest;
    public SerializedDictionary<CharacterBase, List<ActionInfo>> characterActions = new SerializedDictionary<CharacterBase, List<ActionInfo>>();
    public SerializedDictionary<CharacterBase, ActionInfo> characterFinalActions = new SerializedDictionary<CharacterBase, ActionInfo>();
    public GameObject[] mobileInputs;
    public Action OnEndTurn;
    public bool _isPlayerTurn;
    public bool isPlayerTurn
    {
        get => _isPlayerTurn;
        set
        {
            if (_isPlayerTurn != value)
            {
                _isPlayerTurn = value;
                if (_isPlayerTurn)
                {
                    playerManager.canShowGridAndDecal = true;
                    playerManager.EnableVisuals();
                    EnableMobileInputs();
                }
            }
        }
    }
    public bool isChangingTurn = false;
    void Start()
    {
        playerManager.characterActions.CharacterInputs.Back.performed += OnUndoAction;
        GameManager.Instance.openCloseScene.OnFinishOpenAnimation += OnFinishOpenAnimation;
        endTurnTest.started += OnEndTurnTest;
        endTurnTest.Enable();
    }
    void OnDestroy()
    {
        endTurnTest.started -= OnEndTurnTest;
        playerManager.characterActions.CharacterInputs.Back.performed -= OnUndoAction;
        GameManager.Instance.openCloseScene.OnFinishOpenAnimation -= OnFinishOpenAnimation;

    }
    void OnFinishOpenAnimation()
    {
        _ = ChangeRoundState("game_scene_menu_round_state_start");
    }
    public async Task ChangeRoundState(string idText)
    {
        roundStateLanguage.ChangeTextById(idText);
        roundStateAnimator.Play("ShowStateOpen");
        while (true)
        {
            await Awaitable.NextFrameAsync();
            if (roundStateAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowStateIdle"))
            {
                isPlayerTurn = !isPlayerTurn;
                break;
            }
        }
    }
    public void OnEndTurnTest(InputAction.CallbackContext context)
    {
        if (!isPlayerTurn) _ = EndTurn();
    }
    void OnUndoAction(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isPause) return;
        if (playerManager.menuItemsCharacter.menuItemCharacters.activeSelf)
        {
            if (playerManager.menuItemsCharacter.currentBagItem)
            {
                playerManager.menuItemsCharacter.BackToBagItems();
            }
            else
            {
                _= playerManager.menuItemsCharacter.DisableMenu();
            }
        }
        else if (playerManager.menuSkillsCharacter.menuSkillsCharacter.activeSelf)
        {
            if (!playerManager.menuSkillsCharacter.menuSkillSelectSkill.activeSelf)
            {
                _= playerManager.menuSkillsCharacter.DisableMenuForSelectCharacterToMakeSkill();
            }
            else
            {
                _= playerManager.menuSkillsCharacter.DisableMenu();
            }
        }
        else if (playerManager.menuAllCharacters.menuAllCharacters.activeSelf)
        {
            _= playerManager.menuAllCharacters.DisableMenu();
        }
        else if (playerManager.menuLiftCharacter.menuLiftCharacter.activeSelf)
        {
            _= playerManager.menuLiftCharacter.DisableMenu();
        }
        else if (playerManager.menuAttackCharacter.menuAttackCharacter.activeSelf)
        {
            _= playerManager.menuAttackCharacter.DisableMenu();
        }
        else if (playerManager.menuCharacterSelector.menuCharacterSelector.activeSelf)
        {
            _= playerManager.menuCharacterSelector.DisableMenu();
        }
        else if (playerManager.menuCharacterActions.menuCharacterActions.activeSelf)
        {
            _= playerManager.menuCharacterActions.DisableMenu();
        }
        else if (playerManager.menuGeneralActions.menuGeneralActions.activeSelf)
        {
            _= playerManager.menuGeneralActions.DisableMenu();
        }
        else if (playerManager.menuCharacterInfo.menuCharacterInfo.activeSelf)
        {
            playerManager.menuCharacterInfo.DisableMenu();
        }
        else if (playerManager.menuThrowCharacter.menuThrowCharacter.activeSelf)
        {
            _= playerManager.menuThrowCharacter.DisableMenu();
        }
        else if (playerManager.aStarPathFinding.characterSelected)
        {
            playerManager.aStarPathFinding.characterSelected = null;
            playerManager.aStarPathFinding.DisableGrid();
        }
        else if (
            playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)].hasCharacter != null &&
            characterActions.TryGetValue(playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)].hasCharacter, out List<ActionInfo> actions))
        {
            if (actions.Count > 0 && actions[actions.Count - 1].cantUndo) return;
            switch (actions[actions.Count - 1].typeAction)
            {
                case TypeAction.Move:
                    if (playerManager.aStarPathFinding.grid[actions[actions.Count - 1].positionInGrid].hasCharacter == null)
                    {
                        playerManager.aStarPathFinding.grid[actions[actions.Count - 1].characterMakeAction.positionInGrid].hasCharacter = null;
                        actions[actions.Count - 1].characterMakeAction.transform.position = actions[actions.Count - 1].positionInGrid;
                        StartCoroutine(playerManager.MovePointerTo(actions[actions.Count - 1].positionInGrid));
                        if (actions[actions.Count - 1].positionInGrid == Vector3Int.zero)
                        {
                            playerManager.menuCharacterSelector.amountCharacters++;
                            actions[actions.Count - 1].characterMakeAction.positionInGrid = Vector3Int.zero;
                            actions[actions.Count - 1].characterMakeAction.gameObject.SetActive(false);
                            characterActions.Remove(actions[actions.Count - 1].characterMakeAction);
                        }
                        else
                        {
                            playerManager.aStarPathFinding.grid[actions[actions.Count - 1].positionInGrid].hasCharacter = actions[actions.Count - 1].characterMakeAction;
                            actions[actions.Count - 1].characterMakeAction.positionInGrid = actions[actions.Count - 1].positionInGrid;
                            if (actions.Count > 1)
                            {
                                actions.RemoveAt(actions.Count - 1);
                            }
                            else
                            {
                                characterActions.Remove(actions[actions.Count - 1].characterMakeAction);
                            }
                        }
                    }
                    else if (actions[actions.Count - 1].positionInGrid == Vector3Int.zero)
                    {
                        playerManager.aStarPathFinding.grid[actions[actions.Count - 1].characterMakeAction.positionInGrid].hasCharacter = null;
                        actions[actions.Count - 1].characterMakeAction.positionInGrid = Vector3Int.zero;
                        playerManager.menuCharacterSelector.amountCharacters++;
                        StartCoroutine(playerManager.MovePointerTo(actions[actions.Count - 1].positionInGrid));
                        actions[actions.Count - 1].characterMakeAction.gameObject.SetActive(false);
                        characterActions.Remove(actions[actions.Count - 1].characterMakeAction);
                    }
                    break;
                case TypeAction.Lift:
                    if (!playerManager.aStarPathFinding.grid[actions[actions.Count - 1].characterToMakeAction[0].positionInGrid].hasCharacter)
                    {
                        actions[actions.Count - 1].characterMakeAction.lastAction = TypeAction.None;
                        characterFinalActions.Remove(actions[actions.Count - 1].characterMakeAction);
                        actions[actions.Count - 1].characterMakeAction.characterAnimations.MakeAnimation("Idle");
                        actions[actions.Count - 1].characterMakeAction.characterStatusEffect.statusEffects.Remove(playerManager.menuLiftCharacter.statusEffectLiftSO);
                        actions[actions.Count - 1].characterToMakeAction[0].character.transform.SetParent(null);
                        if (actions[actions.Count - 1].characterToMakeAction[0].character.characterAnimations.currentAnimation.name != "Lift") actions[actions.Count - 1].characterToMakeAction[0].character.characterAnimations.MakeAnimation("Idle");
                        actions[actions.Count - 1].characterToMakeAction[0].character.transform.position = actions[actions.Count - 1].characterToMakeAction[0].positionInGrid;
                        playerManager.aStarPathFinding.grid[actions[actions.Count - 1].characterToMakeAction[0].positionInGrid].hasCharacter = actions[actions.Count - 1].characterToMakeAction[0].character;
                        actions.RemoveAt(actions.Count - 1);
                        if (actions.Count == 0) characterActions.Remove(actions[actions.Count - 1].characterMakeAction);
                    }
                    break;
                case TypeAction.Defend:
                case TypeAction.Attack:
                case TypeAction.Skill:
                    actions[actions.Count - 1].characterMakeAction.lastAction = TypeAction.None;
                    characterFinalActions.Remove(actions[actions.Count - 1].characterMakeAction);
                    if (actions.Count - 1 == 0) characterActions.Remove(actions[actions.Count - 1].characterMakeAction);
                    else actions.RemoveAt(actions.Count - 1);
                    break;
            }
        }
        else
        {
            if (playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)].hasCharacter &&
                Vector3Int.RoundToInt(playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)].hasCharacter.transform.position) == Vector3Int.zero)
            {
                playerManager.menuCharacterSelector.amountCharacters++;
                playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)].hasCharacter.gameObject.SetActive(false);
                playerManager.aStarPathFinding.grid[Vector3Int.RoundToInt(BattlePlayerManager.Instance.mouseDecal.transform.position)].hasCharacter = null;
            }
        }
    }
    public ActionInfo GetLastActionByCharacter(CharacterBase character)
    {
        return characterActions[character][characterActions[character].Count - 1];
    }
    public async Task MakeActions()
    {
        foreach (KeyValuePair<CharacterBase, ActionInfo> actions in characterFinalActions)
        {
            if (actions.Value.characterMakeAction.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
            {
                switch (actions.Value.typeAction)
                {
                    case TypeAction.Attack:
                        actions.Value.characterMakeAction.characterAnimations.MakeAnimation(actions.Value.characterMakeAction.characterAnimations.GetAnimationAttack());
                        await Awaitable.NextFrameAsync();
                        bool makedDamage = false;
                        while (true)
                        {
                            if (actions.Value.characterMakeAction.characterAnimations.currentAnimation.frameToInstance == actions.Value.characterMakeAction.characterAnimations.currentSpriteIndex && !makedDamage)
                            {
                                makedDamage = true;
                                foreach (OtherCharacterInfo otherCharacter in actions.Value.characterToMakeAction)
                                {
                                    if (otherCharacter.character.characterData.statistics[CharacterData.TypeStatistic.Hp].currentValue > 0)
                                    {
                                        otherCharacter.character.TakeDamage(actions.Value.characterMakeAction, actions.Value.characterMakeAction.characterData.statistics[CharacterData.TypeStatistic.Atk].currentValue);
                                    }
                                }
                            }
                            if (actions.Value.characterMakeAction.characterAnimations.currentAnimation.name == "Idle") break;
                            await Awaitable.NextFrameAsync();
                        }
                        actions.Key.lastAction = TypeAction.EndTurn;
                        await Task.Delay(TimeSpan.FromSeconds(0.25f));
                        characterActions[actions.Key].Add(new ActionInfo()
                        {
                            cantUndo = false,
                            positionInGrid = actions.Key.positionInGrid,
                            typeAction = TypeAction.EndTurn
                        });
                        await Task.Delay(TimeSpan.FromSeconds(0.5f));
                        break;
                    case TypeAction.Skill:
                        if (!actions.Value.skillInfo.skillsBaseSO.needSceneAnimation)
                        {
                            if (actions.Value.characterMakeAction.initialDataSO.animations.ContainsKey(actions.Value.skillInfo.skillsBaseSO.animationSkillName))
                            {
                                actions.Value.characterMakeAction.characterAnimations.MakeAnimation(actions.Value.skillInfo.skillsBaseSO.animationSkillName);
                            }
                            else
                            {
                                actions.Value.characterMakeAction.characterAnimations.MakeAnimation(actions.Value.skillInfo.skillsBaseSO.generalAnimationSkillName);
                            }
                            foreach (Vector3Int position in actions.Value.positionsToMakeSkill)
                            {
                                playerManager.aStarPathFinding.GetHighestBlockAt(new Vector3Int(position.x, 0, position.z), out GenerateMap.WalkablePositionInfo block);
                                GameObject skillEffect = Instantiate(actions.Value.skillInfo.skillsBaseSO.skillVFXPrefab, block != null ? block.pos : position, Quaternion.identity);
                                Destroy(skillEffect, actions.Value.skillInfo.skillsBaseSO.skillVFXDuration);
                                if (block != null && playerManager.aStarPathFinding.grid[block.pos].hasCharacter)
                                {
                                    actions.Value.skillInfo.skillsBaseSO.UseSkill(actions.Value.characterMakeAction, playerManager.aStarPathFinding.grid[block.pos].hasCharacter);
                                }
                            }
                            actions.Value.skillInfo.skillsBaseSO.DiscountMpAfterUseSkill(actions.Value.characterMakeAction);
                            float elapsedTime = 0f;
                            while (elapsedTime < actions.Value.skillInfo.skillsBaseSO.skillVFXDuration)
                            {
                                elapsedTime += Time.deltaTime;
                                await Awaitable.NextFrameAsync();
                            }
                        }
                        else
                        {
                            print("Need Scene Animation Non Implemented");
                        }
                        actions.Key.lastAction = TypeAction.EndTurn;
                        await Task.Delay(TimeSpan.FromSeconds(0.25f));
                        characterActions[actions.Key].Add(new ActionInfo()
                        {
                            cantUndo = false,
                            positionInGrid = actions.Key.positionInGrid,
                            typeAction = TypeAction.EndTurn
                        });
                        await Task.Delay(TimeSpan.FromSeconds(0.5f));
                        break;
                    case TypeAction.Defend:
                        await DefendAction(actions.Key);
                        actions.Key.lastAction = TypeAction.EndTurn;
                        characterActions[actions.Key].Add(new ActionInfo()
                        {
                            cantUndo = false,
                            positionInGrid = actions.Key.positionInGrid,
                            typeAction = TypeAction.EndTurn
                        });
                        await Awaitable.NextFrameAsync();
                        break;
                }
            }
        }
        characterFinalActions = new SerializedDictionary<CharacterBase, ActionInfo>();
        await Awaitable.NextFrameAsync();
    }
    private async Task DiscountStatusEffects()
    {
        OnEndTurn?.Invoke();
        await Awaitable.NextFrameAsync();
    }
    public async Task EndTurn()
    {
        isChangingTurn = true;
        playerManager.aStarPathFinding.characterSelected = null;
        await MakeActions();
        characterActions = new SerializedDictionary<CharacterBase, List<ActionInfo>>();
        characterFinalActions = new SerializedDictionary<CharacterBase, ActionInfo>();
        await ChangeRoundState();
        await DiscountStatusEffects();
    }
    public async Task DefendAction(CharacterBase character)
    {
        StatusEffectDefendSO statusEffectDefendSO = Resources.Load<StatusEffectDefendSO>("Prefabs/ScriptableObjects/StatusEffects/StatusEffectDefend");
        if (character.characterStatusEffect.statusEffects.ContainsKey(statusEffectDefendSO))
        {
            statusEffectDefendSO.ReloadEffect(character);
        }
        else
        {
            character.characterStatusEffect.statusEffects.Add(statusEffectDefendSO, 0);
            statusEffectDefendSO.ApplyEffect(character);
        }
        character.characterAnimations.MakeAnimation("Defend");
        await Awaitable.NextFrameAsync();
        while (true)
        {
            if (character.characterAnimations.currentAnimation.name == "Idle") break;
            await Awaitable.NextFrameAsync();
        }
        await Awaitable.NextFrameAsync();
    }
    public async Task ChangeRoundState()
    {
        await ChangeRoundState(!isChangingTurn ? isPlayerTurn ? "game_scene_menu_round_state_player_turn" : "game_scene_menu_round_state_enemy_turn" : !isPlayerTurn ? "game_scene_menu_round_state_player_turn" : "game_scene_menu_round_state_enemy_turn");
        isChangingTurn = false;
    }
    public void ActionForExecuteExist(out bool actionExist)
    {
        actionExist = false;
        foreach (KeyValuePair<CharacterBase, List<ActionInfo>> action in characterActions)
        {
            if (action.Value[action.Value.Count - 1].typeAction == TypeAction.Attack) actionExist = true;
            if (action.Value[action.Value.Count - 1].typeAction == TypeAction.Skill) actionExist = true;
            if (action.Value[action.Value.Count - 1].typeAction == TypeAction.Defend) actionExist = true;
        }
    }
    public void EnableMobileInputs()
    {
        foreach (GameObject button in mobileInputs)
        {
            button.SetActive(true);
        }
    }
    public void DisableMobileInputs()
    {
        foreach (GameObject button in mobileInputs)
        {
            button.SetActive(false);
        }
    }
    [Serializable]
    public class ActionInfo
    {
        public bool cantUndo;
        public CharacterBase characterMakeAction;
        public List<OtherCharacterInfo> characterToMakeAction;
        public List<Vector3Int> positionsToMakeSkill;
        public CharacterData.CharacterSkillInfo skillInfo;
        public TypeAction typeAction;
        public Vector3Int positionInGrid;
        public ActionInfo
        (
            bool cantUndo = false, CharacterBase characterMakeAction = null, List<OtherCharacterInfo> characterToMakeAction = null, List<Vector3Int> positionsToMakeSkill = null,
            CharacterData.CharacterSkillInfo skillInfo = null, TypeAction typeAction = TypeAction.None, Vector3Int positionInGrid = default)
        {
            this.cantUndo = cantUndo;
            this.characterMakeAction = characterMakeAction;
            this.characterToMakeAction = characterToMakeAction;
            this.positionsToMakeSkill = positionsToMakeSkill;
            this.skillInfo = skillInfo;
            this.typeAction = typeAction;
            this.positionInGrid = positionInGrid;
        }
    }
    [Serializable]
    public class OtherCharacterInfo
    {
        public CharacterBase character;
        public Vector3Int positionInGrid;
        public OtherCharacterInfo(CharacterBase character, Vector3Int positionInGrid)
        {
            this.character = character;
            this.positionInGrid = positionInGrid;
        }
    }
    public enum TypeAction
    {
        None = 0,
        Move = 1,
        Attack = 2,
        Skill = 3,
        Defend = 4,
        Lift = 5,
        Item = 6,
        EndTurn = 7
    }
}