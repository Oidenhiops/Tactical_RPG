using UnityEngine;

public class CharacterWorldNpc : CharacterBase, CharacterWorldPlayer.IInteractable
{
    public GameObject bannerInteract;
    public override void OnEnableHandle()
    {
        WorldManager.Instance.currentWorldMap.OnFinishGenerateMap += OnFinishGenerateMap;
    }
    void OnDisable()
    {
        WorldManager.Instance.currentWorldMap.OnFinishGenerateMap -= OnFinishGenerateMap;
    }
    void OnFinishGenerateMap()
    {
        positionInGrid = Vector3Int.RoundToInt(transform.position);
        WorldManager.Instance.aStarPathFinding.grid[positionInGrid].hasCharacter = this;
        WorldManager.Instance.aStarPathFinding.grid[positionInGrid].isWalkable = false;
    }
    public void Interact(CharacterWorldPlayer character)
    {
        character.cantMakeActions = true;
        bannerInteract.SetActive(false);
        _ = GameManager.Instance.LoadScene(GameManager.TypeScene.DialogScene);
        _ = AwaitForShowDialog();
    }
    public async Awaitable AwaitForShowDialog()
    {
        while (true)
        {
            if (DialogManager.Instance && DialogManager.Instance.CanShowDialogs())
            {
                await DialogManager.Instance.ShowText();
                break;
            }
            await Awaitable.NextFrameAsync();
        }
    }
    
    public void OnInteractEnter()
    {
        bannerInteract.SetActive(true);
    }

    public void OnInteractExit()
    {
        bannerInteract.SetActive(false);
    }
}
