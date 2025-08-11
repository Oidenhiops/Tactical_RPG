using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public ManagementOpenCloseScene openCloseScene;
    public bool isPause;
    public bool isWebGlBuild;
    public bool lockDevice = false;
    public TypeDevice principalDevice;
    public TypeDevice _currentDevice;
    public event Action<TypeDevice, TypeDevice> OnDeviceChanged;
    public Action OnChangeScene;
    public TypeDevice currentDevice
    {
        get => _currentDevice;
        set
        {
            if (_currentDevice != value)
            {
                _currentDevice = value;
                OnDeviceChanged?.Invoke(principalDevice, _currentDevice);
            }
        }
    }
    public bool _startGame;
    public Action<bool> OnStartGame;
    public InputAction pauseButton;
    public string currentScene;
    string[] _excludedScenesForPause = { "CreditsScene", "HomeScene", "OptionsScene" };
    public bool startGame
    {
        get => _startGame;
        set
        {
            if (_startGame != value)
            {
                _startGame = value;
                OnStartGame?.Invoke(_startGame);
            }
        }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        pauseButton.started += PauseHandle;
        pauseButton.Enable();
    }
    void Start()
    {
        SetInitialDevice();
        OnDeviceChanged += ValidateActiveMouse;
        ValidateActiveMouse(principalDevice,currentDevice);
    }
    void LateUpdate()
    {
        CheckCurrentDevice();
    }
    public void PauseHandle(InputAction.CallbackContext context)
    {
        if (!_excludedScenesForPause.Contains(SceneManager.GetActiveScene().name) && startGame)
        {
            ChangeSceneSelector(TypeScene.OptionsScene);
        }
    }
    public void ChangeSceneSelector(TypeScene typeScene)
    {
        switch (typeScene)
        {
            case TypeScene.OptionsScene:
                if (!SceneManager.GetSceneByName("OptionsScene").isLoaded) SceneManager.LoadScene("OptionsScene", LoadSceneMode.Additive);
                break;
            case TypeScene.CreditsScene:
                if (!SceneManager.GetSceneByName("CreditsScene").isLoaded) SceneManager.LoadScene("CreditsScene", LoadSceneMode.Additive);
                break;
            case TypeScene.GameOverScene:
                if (!SceneManager.GetSceneByName("GameOverScene").isLoaded) SceneManager.LoadScene("GameOverScene", LoadSceneMode.Additive);
                break;
            default:
                _ = ChangeScene(typeScene);
                break;
        }
    }
    async Awaitable ChangeScene(TypeScene typeScene)
    {
        try
        {
            startGame = false;
            currentScene = typeScene.ToString();
            openCloseScene.openCloseSceneAnimator.SetBool("Out", true);
            OnChangeScene?.Invoke();
            await AudioManager.Instance.FadeOut();
            AudioManager.Instance.ChangeBGM(GameData.Instance.saveData.bgmSceneData[currentScene]);
            while (!openCloseScene.openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).IsName("OpenCloseSceneIdle")) await Task.Delay(TimeSpan.FromSeconds(0.01)); ;
            if (typeScene == TypeScene.Reload)
            {
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
            }
            else if (typeScene == TypeScene.Exit)
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(typeScene.ToString());
            }
            await Task.Delay(TimeSpan.FromSeconds(0.05));
            _ = openCloseScene.WaitFinishCloseAnimation();
            _ = AudioManager.Instance.FadeIn();
            await Task.Delay(TimeSpan.FromSeconds(0.05));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Task.Delay(TimeSpan.FromSeconds(0.05));
        }
    }
    public void ValidateActiveMouse(TypeDevice principalDevice, TypeDevice typeDevice)
    {
        if (principalDevice == TypeDevice.PC)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }
    public void SetInitialDevice()
    {
        if (!isWebGlBuild)
        {
            if (Gamepad.current != null)
            {
                principalDevice = TypeDevice.PC;
                currentDevice = TypeDevice.GAMEPAD;
            }
            else if (Touchscreen.current != null)
            {
                principalDevice = TypeDevice.MOBILE;
                currentDevice = TypeDevice.MOBILE;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.LinuxPlayer)
            {
                principalDevice = TypeDevice.PC;
                currentDevice = TypeDevice.PC;
            }
        }
        else
        {
            currentDevice = TypeDevice.PC;
        }
    }
    void CheckCurrentDevice()
    {
        if (lockDevice) return;
        if (!isWebGlBuild)
        {
            if (ValidateDeviceIsMobile())
            {
                currentDevice = TypeDevice.MOBILE;
            }
            else if (ValidateIsGamepad())
            {
                currentDevice = TypeDevice.GAMEPAD;
            }
            else if (ValidateDeviceIsPc())
            {
                currentDevice = TypeDevice.PC;
            }
        }
        else
        {
            currentDevice = TypeDevice.PC;
        }
    }
    bool ValidateDeviceIsPc()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return false;
        bool validateAnyPcInput = 
            keyboard.anyKey.wasPressedThisFrame ||
            mouse.leftButton.wasPressedThisFrame ||
            mouse.rightButton.wasPressedThisFrame ||
            mouse.scroll.ReadValue() != Vector2.zero ||
            mouse.delta.ReadValue() != Vector2.zero;
        return validateAnyPcInput;
    }
    bool ValidateIsGamepad()
    {
        var gamePad = Gamepad.current;
        if (gamePad == null || Gamepad.all.Count == 0 || !IsRealGamepadConnected()) return false;
        bool validateAnyGamepadInput =
            gamePad.buttonSouth.wasPressedThisFrame ||
            gamePad.buttonNorth.wasPressedThisFrame ||
            gamePad.buttonEast.wasPressedThisFrame ||
            gamePad.buttonWest.wasPressedThisFrame ||
            gamePad.leftStick.ReadValue().magnitude > 0.1f ||
            gamePad.rightStick.ReadValue().magnitude > 0.1f ||
            gamePad.dpad.ReadValue().magnitude > 0.1f ||
            gamePad.leftTrigger.wasPressedThisFrame ||
            gamePad.rightTrigger.wasPressedThisFrame;        
        return gamePad != null && validateAnyGamepadInput && !ValidateDeviceIsPc();
    }
    bool IsRealGamepadConnected()
    {
        return Gamepad.all.Any(g =>
            g.displayName != "Gamepad" &&
            g.enabled &&
            g.wasUpdatedThisFrame
        );
    }
    bool ValidateDeviceIsMobile()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return false;
        foreach (var touch in touchscreen.touches)
        {
            if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.delta.ReadValue().magnitude > 0.01f)
                return true;
        }
        return false;
    }
    public enum TypeScene
    {
        HomeScene = 0,
        OptionsScene = 1,
        GameScene = 2,
        CreditsScene = 3,
        Reload = 4,
        Exit = 5,
        GameOverScene = 6,
    }
    public enum TypeDevice
    {
        None,
        PC,
        GAMEPAD,
        MOBILE,
    }
}
