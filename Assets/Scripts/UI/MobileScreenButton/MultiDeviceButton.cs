using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
public class MultiDeviceButton : MonoBehaviour
{
    public SerializedDictionary<DeviceInfo, GameObject[]> buttons;
    void Start()
    {
        ValidateScreenButton(GameManager.Instance.principalDevice, GameManager.Instance.currentDevice);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDeviceChanged += ValidateScreenButton;
        }
    }
    void OnDestroy()
    {
        GameManager.Instance.OnDeviceChanged -= ValidateScreenButton;
    }
    void ValidateScreenButton(GameManager.TypeDevice principalDevice, GameManager.TypeDevice currentDevice)
    {
        foreach (var button in buttons)
        {
            foreach (var deviceButton in button.Value)
            {
                deviceButton.SetActive(false);
            }
        }
        foreach (var button in buttons)
        {
            if (button.Key.principalDevice == principalDevice && button.Key.possibleDevices.Contains(currentDevice))
            {
                foreach (var deviceButton in button.Value)
                {
                    deviceButton.SetActive(true);
                }
            }
        }
    }
    [System.Serializable] public class DeviceInfo
    {
        public GameManager.TypeDevice principalDevice;
        public GameManager.TypeDevice[] possibleDevices;
    }
}
