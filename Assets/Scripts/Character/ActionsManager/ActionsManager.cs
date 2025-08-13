using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionsManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public List<ActionInfo> actions;
    void Start()
    {
        playerManager.characterActions.CharacterInputs.Back.started += OnUndoAction;
    }
    void OnUndoAction(InputAction.CallbackContext context)
    {
        if (AStarPathFinding.Instance.characterSelected)
        {
            AStarPathFinding.Instance.DisableGrid();
        }
        else if (actions.Count > 0)
        {
            switch (actions[actions.Count - 1].typeAction)
            {
                case TypeAction.Move:
                    AStarPathFinding.Instance.grid[actions[actions.Count - 1].positionInGrid].hasCharacter = null;
                    actions.RemoveAt(actions.Count - 1);
                    AStarPathFinding.Instance.grid[actions[actions.Count - 1].positionInGrid].hasCharacter = actions[actions.Count - 1].character;
                    actions[actions.Count - 1].character.transform.position = actions[actions.Count - 1].positionInGrid;
                    actions[actions.Count - 1].character.currentPositionInGrid = actions[actions.Count - 1].positionInGrid;
                    break;
                case TypeAction.Spawn:
                    AStarPathFinding.Instance.grid[Vector3Int.zero].hasCharacter = null;
                    
                    AStarPathFinding.Instance.DisableGrid();
                    actions[actions.Count - 1].character.gameObject.SetActive(false);
                    actions.RemoveAt(actions.Count - 1);
                    break;
                case TypeAction.Despawn:
                    actions[actions.Count - 1].character.gameObject.SetActive(true);
                    actions.RemoveAt(actions.Count - 1);
                    if (actions[actions.Count - 1].typeAction == TypeAction.Move)
                    {
                        AStarPathFinding.Instance.grid[actions[actions.Count - 1].positionInGrid].hasCharacter = actions[actions.Count - 1].character;
                        actions[actions.Count - 1].character.transform.position = actions[actions.Count - 1].positionInGrid;
                        actions[actions.Count - 1].character.currentPositionInGrid = actions[actions.Count - 1].positionInGrid;
                    }
                    break;
            }
        }
    }
    void ReposPlayerUndoMove()
    {
        
    }
    public void AddAction(ActionInfo actionInfo)
    {
        actions.Add(actionInfo);
    }
    public bool MoveActionExist(Character character)
    {
        foreach (ActionInfo action in actions)
        {
            if (action.character == character && action.typeAction == TypeAction.Move) return true;
        }
        return false;
    }
    [Serializable]
    public class ActionInfo
    {
        public Character character;
        public TypeAction typeAction;
        public Vector3Int positionInGrid;
        public ActionInfo(Character character, TypeAction typeAction, Vector3Int positionInGrid)
        {
            this.character = character;
            this.typeAction = typeAction;
            this.positionInGrid = positionInGrid;
        }
    }
    public enum TypeAction
    {
        None = 0,
        Spawn = 1,
        Despawn = 2,
        Move = 3,
        Attack = 4,
    }
}
