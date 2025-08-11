using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class MenuHelper : MonoBehaviour
{
    public Button lastButtonSelected;
    public CinemachineCamera cinemachineCamera;
    public void SelectButton()
    {
        lastButtonSelected.Select();
    }
    public void ChangeLastButtonSelected(Button button)
    {
        lastButtonSelected = button;
    }
}
