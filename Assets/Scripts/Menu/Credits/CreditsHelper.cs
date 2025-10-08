using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsHelper : MonoBehaviour
{
    public Button creditsButton;
    public InputAction backButton;
    public GameManagerHelper gameManagerHelper;
    public GameObject lastButtonSelected;
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
        lastButtonSelected = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(creditsButton.gameObject);
    }
    void UnloadCreditScene(InputAction.CallbackContext context)
    {
        UnloadScene();
    }
    public void UnloadScene()
    {
        gameManagerHelper.lastButtonSelected = lastButtonSelected;
        gameManagerHelper.UnloadScene();
    }
}
