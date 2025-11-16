using UnityEngine;

public class Door : MonoBehaviour, CharacterWorldPlayer.IInteractable
{
    public GameObject bannerInteract;
    public GameManager.TypeScene targetScene;
    public GameObject GetObjectInteract()
    {
        return gameObject;
    }

    public void Interact(CharacterWorldPlayer character)
    {
        bannerInteract.SetActive(false);
        WorldManager.Instance.cantMakeActions = true;
        _ = GameManager.Instance.LoadScene(targetScene, UnityEngine.SceneManagement.LoadSceneMode.Additive, GameManager.TypeLoader.BlackOut, true);
    }

    public void OnInteractEnter()
    {
        bannerInteract.SetActive(true);
    }

    public void OnInteractExit()
    {
        bannerInteract.SetActive(false);
    }

    public void ResumeWorldAfterInteraction()
    {
        bannerInteract.SetActive(true);
    }
}
