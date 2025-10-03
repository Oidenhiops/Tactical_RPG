using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuHelper : MonoBehaviour
{
    public Button lastButtonSelected;
    public InputAction handleBack;
    void Awake()
    {
        handleBack.Enable();
    }
    void OnDestroy()
    {
        handleBack.Disable();
    }
}
