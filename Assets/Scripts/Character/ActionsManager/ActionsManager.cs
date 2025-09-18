using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionsManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public InputAction endTurnTest;
    public SerializedDictionary<Character, List<ActionInfo>> characterActions = new SerializedDictionary<Character, List<ActionInfo>>();
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
                }
            }
        }
    }
    public bool isChangingTurn = false;
    void Start()
    {
        playerManager.characterActions.CharacterInputs.Back.started += OnUndoAction;
        endTurnTest.Enable();
        endTurnTest.started += OnEndTurnTest;
    }
    public void OnEndTurnTest(InputAction.CallbackContext context)
    {
        if (!isPlayerTurn) _ = EndTurn();
    }
    void OnUndoAction(InputAction.CallbackContext context)
    {
        if (!isPlayerTurn) return;
        if (playerManager.menuAllCharacters.menuAllCharacters.activeSelf)
        {
            playerManager.menuAllCharacters.DisableMenu();
        }
        else if (playerManager.menuLiftCharacter.menuLiftCharacter.activeSelf)
        {
            playerManager.menuLiftCharacter.DisableMenu();
        }
        else if (playerManager.menuAttackCharacter.menuAttackCharacter.activeSelf)
        {
            playerManager.menuAttackCharacter.DisableMenu();
        }
        else if (playerManager.menuCharacterSelector.menuCharacterSelector.activeSelf)
        {
            playerManager.menuCharacterSelector.DisableMenu();
        }
        else if (playerManager.menuCharacterActions.menuCharacterActions.activeSelf)
        {
            playerManager.menuCharacterActions.DisableMenu();
        }
        else if (playerManager.menuGeneralActions.menuGeneralActions.activeSelf)
        {
            playerManager.menuGeneralActions.DisableMenu();
        }
        else if (playerManager.menuCharacterInfo.menuCharacterInfo.activeSelf)
        {
            playerManager.menuCharacterInfo.DisableMenu();
        }
        else if (playerManager.menuThrowCharacter.menuThrowCharacter.activeSelf)
        {
            playerManager.menuThrowCharacter.DisableMenuBack();
        }
        else if (AStarPathFinding.Instance.characterSelected)
        {
            AStarPathFinding.Instance.characterSelected = null;
            AStarPathFinding.Instance.DisableGrid();
        }
        else if (
            AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)].hasCharacter != null &&
            characterActions.TryGetValue(AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)].hasCharacter, out List<ActionInfo> actions))
        {
            if (actions.Count > 0 && actions[actions.Count - 1].cantUndo) return;
            switch (actions[actions.Count - 1].typeAction)
            {
                case TypeAction.Move:
                    if (AStarPathFinding.Instance.grid[actions[actions.Count - 1].positionInGrid].hasCharacter == null)
                    {
                        AStarPathFinding.Instance.grid[actions[actions.Count - 1].character.positionInGrid].hasCharacter = null;
                        actions[actions.Count - 1].character.transform.position = actions[actions.Count - 1].positionInGrid;
                        StartCoroutine(playerManager.MovePointerTo(actions[actions.Count - 1].positionInGrid));
                        if (actions[actions.Count - 1].positionInGrid == Vector3Int.zero)
                        {
                            playerManager.menuCharacterSelector.amountCharacters++;
                            actions[actions.Count - 1].character.positionInGrid = Vector3Int.zero;
                            actions[actions.Count - 1].character.gameObject.SetActive(false);
                            characterActions.Remove(actions[actions.Count - 1].character);
                        }
                        else
                        {
                            AStarPathFinding.Instance.grid[actions[actions.Count - 1].positionInGrid].hasCharacter = actions[actions.Count - 1].character;
                            actions[actions.Count - 1].character.positionInGrid = actions[actions.Count - 1].positionInGrid;
                            if (actions.Count > 1)
                            {
                                actions.RemoveAt(actions.Count - 1);
                            }
                            else
                            {
                                characterActions.Remove(actions[actions.Count - 1].character);
                            }
                        }
                    }
                    else if (actions[actions.Count - 1].positionInGrid == Vector3Int.zero)
                    {
                        AStarPathFinding.Instance.grid[actions[actions.Count - 1].character.positionInGrid].hasCharacter = null;
                        actions[actions.Count - 1].character.positionInGrid = Vector3Int.zero;
                        playerManager.menuCharacterSelector.amountCharacters++;
                        StartCoroutine(playerManager.MovePointerTo(actions[actions.Count - 1].positionInGrid));
                        actions[actions.Count - 1].character.gameObject.SetActive(false);
                        characterActions.Remove(actions[actions.Count - 1].character);
                    }
                    break;
                case TypeAction.Lift:
                    if (!AStarPathFinding.Instance.grid[actions[actions.Count - 1].otherCharacterInfo[0].positionInGrid].hasCharacter)
                    {
                        actions[actions.Count - 1].character.lastAction = TypeAction.None;
                        actions[actions.Count - 1].character.characterAnimations.MakeAnimation("Idle");
                        actions[actions.Count - 1].otherCharacterInfo[0].character.transform.SetParent(null);
                        if (actions[actions.Count - 1].otherCharacterInfo[0].character.characterAnimations.currentAnimation.name != "Lift") actions[actions.Count - 1].otherCharacterInfo[0].character.characterAnimations.MakeAnimation("Idle");
                        actions[actions.Count - 1].otherCharacterInfo[0].character.transform.position = actions[actions.Count - 1].otherCharacterInfo[0].positionInGrid;
                        AStarPathFinding.Instance.grid[actions[actions.Count - 1].otherCharacterInfo[0].positionInGrid].hasCharacter = actions[actions.Count - 1].otherCharacterInfo[0].character;
                        actions.RemoveAt(actions.Count - 1);
                        if (actions.Count == 0) characterActions.Remove(actions[actions.Count - 1].character);
                    }
                    break;
                case TypeAction.Defend:
                case TypeAction.Attack:
                    actions[actions.Count - 1].character.lastAction = TypeAction.None;
                    if (actions.Count - 1 == 0) characterActions.Remove(actions[actions.Count - 1].character);
                    else actions.RemoveAt(actions.Count - 1);
                    break;
            }
        }
        else
        {
            if (AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)].hasCharacter &&
                Vector3Int.RoundToInt(AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)].hasCharacter.transform.position) == Vector3Int.zero)
            {
                playerManager.menuCharacterSelector.amountCharacters++;
                AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)].hasCharacter.gameObject.SetActive(false);
                AStarPathFinding.Instance.grid[Vector3Int.RoundToInt(PlayerManager.Instance.mouseDecal.transform.position)].hasCharacter = null;
            }
        }
    }
    public ActionInfo GetLastActionByCharacter(Character character)
    {
        return characterActions[character][characterActions[character].Count - 1];
    }
    public async Task MakeActions()
    {
        foreach (KeyValuePair<Character, List<ActionInfo>> actions in characterActions)
        {
            switch (actions.Value[actions.Value.Count - 1].typeAction)
            {
                case TypeAction.Attack:
                    actions.Value[actions.Value.Count - 1].character.characterAnimations.MakeAnimation(actions.Value[actions.Value.Count - 1].character.characterAnimations.GetAnimationAttack());
                    await Awaitable.NextFrameAsync();
                    bool makedDamage = false;
                    while (true)
                    {
                        if (actions.Value[actions.Value.Count - 1].character.characterAnimations.currentAnimation.frameToInstance == actions.Value[actions.Value.Count - 1].character.characterAnimations.currentSpriteIndex && !makedDamage)
                        {
                            makedDamage = true;
                            foreach (OtherCharacterInfo otherCharacter in actions.Value[actions.Value.Count - 1].otherCharacterInfo)
                            {
                                otherCharacter.character.TakeDamage(actions.Value[actions.Value.Count - 1].character, true);
                            }
                        }
                        if (actions.Value[actions.Value.Count - 1].character.characterAnimations.currentAnimation.name == "Idle") break;
                        await Awaitable.NextFrameAsync();
                    }
                    actions.Key.lastAction = TypeAction.EndTurn;
                    while (true)
                    {
                        if (actions.Value[actions.Value.Count - 1].otherCharacterInfo[0].character.characterAnimations.currentAnimation.name == "Idle") break;
                        await Awaitable.NextFrameAsync();
                    }
                    characterActions[actions.Key].Add(new ActionInfo()
                    {
                        cantUndo = false,
                        positionInGrid = actions.Key.positionInGrid,
                        typeAction = TypeAction.EndTurn
                    });
                    await Task.Delay(TimeSpan.FromSeconds(0.5f));
                    break;
                case TypeAction.Special:
                    await Awaitable.NextFrameAsync();
                    break;
                case TypeAction.Defend:
                    DefendAction(actions.Key);
                    await Awaitable.NextFrameAsync();
                    break;
            }
        }
        await Awaitable.NextFrameAsync();
    }
    public async Task EndTurn()
    {
        isChangingTurn = true;
        AStarPathFinding.Instance.characterSelected = null;
        await MakeActions();
        foreach (KeyValuePair<Character, List<ActionInfo>> actions in characterActions)
        {
            actions.Key.startPositionInGrid = actions.Key.positionInGrid;
            actions.Key.lastAction = TypeAction.None;
            actions.Key.lastAction = TypeAction.None;
        }
        characterActions = new SerializedDictionary<Character, List<ActionInfo>>();
        await ChangeRoundState();
    }
    public void DefendAction(Character character)
    {
        character.characterData.statistics[CharacterData.TypeStatistic.Def].buffValue += 50;
        character.characterData.statistics[CharacterData.TypeStatistic.Def].RefreshValue();
        character.characterData.statistics[CharacterData.TypeStatistic.Def].SetMaxValue();
    }
    public async Task ChangeRoundState()
    {
        await playerManager.ChangeRoundState(!isChangingTurn ? isPlayerTurn ? 21 : 22 : !isPlayerTurn ? 21 : 22);
        isChangingTurn = false;
    }
    public void AttackOrSpecialActionExist(out bool attackActionExist, out bool specialActionExist)
    {
        attackActionExist = false;
        specialActionExist = false;

        foreach (KeyValuePair<Character, List<ActionInfo>> action in characterActions)
        {
            if (action.Value[action.Value.Count - 1].typeAction == TypeAction.Attack) attackActionExist = true;
            if (action.Value[action.Value.Count - 1].typeAction == TypeAction.Special) specialActionExist = true;
        }
    }
    [Serializable] public class ActionInfo
    {
        public bool cantUndo;
        public Character character;
        public List<OtherCharacterInfo> otherCharacterInfo;
        public TypeAction typeAction;
        public Vector3Int positionInGrid;
        public ActionInfo(Character character = null, List<OtherCharacterInfo> otherCharacterInfo = null, TypeAction typeAction = TypeAction.None, Vector3Int positionInGrid = default)
        {
            this.character = character;
            this.otherCharacterInfo = otherCharacterInfo;
            this.typeAction = typeAction;
            this.positionInGrid = positionInGrid;
        }
    }
    [Serializable] public class OtherCharacterInfo
    {
        public Character character;
        public Vector3Int positionInGrid;
        public OtherCharacterInfo(Character character, Vector3Int positionInGrid)
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
        Special = 3,
        Defend = 4,
        Lift = 5,
        Item = 6,
        EndTurn = 7
    }
}