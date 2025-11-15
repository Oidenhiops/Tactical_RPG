using UnityEngine;
using UnityEngine.SceneManagement;
[CreateAssetMenu(fileName = "DialogFunctionGoToMap", menuName = "ScriptableObjects/Dialog/Functions/DialogFunctionGoToMap", order = 1)]
public class DialogFunctionGoToMap : DialogFunctionBaseSO
{
    public GameManager.TypeScene typeScene;
    public LoadSceneMode loadSceneMode;
    public GameManager.TypeLoader typeLoader; 
    public bool consertLastScene = false;
    public override async Awaitable MakeBannerFunction(CharacterBase characterWorld)
    {
        WorldManager.Instance.cantMakeActions = true;
        ManagementBattleInfo.Instance.generateMap = WorldManager.Instance.currentWorldMap;
        ManagementBattleInfo.Instance.principalCharacterEnemy = WorldManager.Instance.characterWorld.characterHitted;
        _ = GameManager.Instance.LoadScene(typeScene, loadSceneMode, typeLoader, consertLastScene);
        while (!ManagementLoaderScene.Instance.ValidateLoaderIsOnIdle()) await Awaitable.NextFrameAsync();
        if (closeDialog)
        {
            await SceneManager.UnloadSceneAsync(GameManager.TypeScene.DialogScene.ToString());
        }
        WorldManager.Instance.worldContainer.gameObject.SetActive(false);
    }
}
