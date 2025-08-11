using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsHelper : MonoBehaviour
{
    public Button creditsButton;
    public InputAction backButton;
    public GameManagerHelper gameManagerHelper;
    void OnEnable()
    {
        backButton.started += UnloadCreditScene;
        backButton.Enable();
    }
    void OnDisable()
    {
        backButton.started -= UnloadCreditScene;
    }
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Invoke("ActiveButton", 0.25f);
    }
    void ActiveButton()
    {
        creditsButton.Select();
    }
    void UnloadCreditScene(InputAction.CallbackContext context)
    {
        gameManagerHelper.UnloadScene();
    }
}
