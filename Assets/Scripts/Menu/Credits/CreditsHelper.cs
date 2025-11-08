using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsHelper : MonoBehaviour, GameManagerHelper.IScene
{
    public Button creditsButton;
    public InputAction backButton;
    public GameManagerHelper gameManagerHelper;
    public GameObject lastButtonSelected;
    public Animator menuAnimator;
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
        gameManagerHelper.sceneData = this;
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
        gameManagerHelper.PlayASoundButton("TouchButtonBack");
        GameManager.Instance.UnloadAdditiveScene(GameManager.TypeScene.CreditsScene, this, lastButtonSelected);
    }
    public bool AnimationEnded()
    {
        return menuAnimator.GetCurrentAnimatorStateInfo(0).IsName("MenuExit") && menuAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
    }
    public void PlayEndAnimation()
    {
        menuAnimator.SetBool("exit", true);
    }
}
