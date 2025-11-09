using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isPause;
    public bool isWebGlBuild;
    public bool lockDevice = false;
    public TypeDevice principalDevice;
    public TypeDevice _currentDevice;
    public event Action<TypeDevice, TypeDevice> OnDeviceChanged;
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
    public TMP_Text fpsText;
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
        pauseButton.performed += PauseHandle;
        pauseButton.Enable();
    }
    void OnDestroy()
    {
        pauseButton.performed -= PauseHandle;
    }
    void Start()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        currentDevice = principalDevice;
        ManagementLoaderScene.Instance.OnFinishOpenAnimation += () => { startGame = true; };
    }
    void LateUpdate()
    {
        float fps = 1.0f / Time.deltaTime;
        fpsText.text = Mathf.RoundToInt(fps).ToString();
        CheckCurrentDevice();
    }
    public void PauseHandle(InputAction.CallbackContext context)
    {
        if (!_excludedScenesForPause.Contains(SceneManager.GetActiveScene().name) && startGame)
        {
            if (!SceneManager.GetSceneByName("OptionsScene").isLoaded)
            {
                _ = LoadScene(TypeScene.OptionsScene);
            }
            else if (ManagementOptions.Instance && ManagementOptions.Instance.isMenuActive)
            {
                _ = UnloadAdditiveScene(TypeScene.OptionsScene, true, TypeLoader.WithProgressBar, ManagementOptions.Instance, ManagementOptions.Instance.lastButtonSelected);
            }
        }
    }
    public void UnloadAdditiveScene(TypeScene typeScene, TypeLoader typeLoader)
    {
        _ = UnloadAdditiveScene(typeScene, false, typeLoader, null, null);
    }
    public void UnloadAdditiveScene(TypeScene typeScene, GameManagerHelper.IScene sceneData, GameObject lastButtonSelected)
    {
        _ = UnloadAdditiveScene(typeScene, true, TypeLoader.None, sceneData, lastButtonSelected);
    }
    public async Awaitable UnloadAdditiveScene(TypeScene typeScene, bool isMenuScene, TypeLoader typeLoader, GameManagerHelper.IScene sceneData, GameObject lastButtonSelected)
    {
        try
        {
            if (isMenuScene)
            {
                sceneData.PlayEndAnimation();
                while (!sceneData.AnimationEnded())
                {
                    await Awaitable.NextFrameAsync();
                }
                if (typeScene == TypeScene.OptionsScene)
                {
                    Time.timeScale = 1;
                    isPause = false;
                }
                if (lastButtonSelected)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(lastButtonSelected);
                }
                _ = SceneManager.UnloadSceneAsync(typeScene.ToString());
            }
            else
            {
                startGame = false;
                Instantiate(Resources.Load<GameObject>($"Prefabs/Loader/{typeLoader}"));
                await Awaitable.NextFrameAsync();
                ManagementLoaderScene.Instance.OnFinishOpenAnimation += () => { startGame = true; };
                await AudioManager.Instance.FadeOut();
                await Awaitable.NextFrameAsync();
                while (!ManagementLoaderScene.Instance.ValidateLoaderIsOnIdle()) await Awaitable.NextFrameAsync();
                if (typeScene == TypeScene.BattleScene)
                {
                    AudioManager.Instance.ChangeBGM(GameData.Instance.systemDataInfo.bgmSceneData[currentScene]);
                    await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(typeScene.ToString()));
                    WorldManager.Instance.ResumeWorldAfterBattle();
                }
                if (typeScene != TypeScene.OptionsScene || typeScene != TypeScene.CreditsScene || typeScene != TypeScene.GameOverScene)
                {
                    if (SceneManager.GetSceneByName(TypeScene.OptionsScene.ToString()).isLoaded)
                    {
                        await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(TypeScene.OptionsScene.ToString()));
                    }
                }
                _ = ManagementLoaderScene.Instance.AutoCharge();
                await AudioManager.Instance.FadeIn();
                await Task.Delay(TimeSpan.FromSeconds(0.5f));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable LoadScene(TypeScene typeScene, LoadSceneMode loadSceneMode = LoadSceneMode.Single, TypeLoader typeLoader = TypeLoader.WithProgressBar, bool consertLastScene = false)
    {
        try
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
                    startGame = false;
                    Instantiate(Resources.Load<GameObject>($"Prefabs/Loader/{typeLoader}"));
                    await Awaitable.NextFrameAsync();
                    ManagementLoaderScene.Instance.OnFinishOpenAnimation += () => { startGame = true; };
                    if (!consertLastScene) currentScene = typeScene.ToString();
                    await AudioManager.Instance.FadeOut();
                    AudioManager.Instance.ChangeBGM(GameData.Instance.systemDataInfo.bgmSceneData[typeScene.ToString()]);
                    await Awaitable.NextFrameAsync();
                    while (!ManagementLoaderScene.Instance.ValidateLoaderIsOnIdle()) await Awaitable.NextFrameAsync();
                    if (typeScene == TypeScene.Reload)
                    {
                        SceneManager.LoadScene(SceneManager.GetSceneAt(0).name, loadSceneMode);
                    }
                    else if (typeScene == TypeScene.HomeScene)
                    {
                        SceneManager.LoadScene(typeScene.ToString(), loadSceneMode);
                        GameData.Instance.LoadGameDataInfo();
                    }
                    else if (typeScene == TypeScene.Exit)
                    {
                        Application.Quit();
                    }
                    else
                    {
                        SceneManager.LoadScene(typeScene.ToString(), loadSceneMode);
                    }
                    await AudioManager.Instance.FadeIn();
                    await Task.Delay(TimeSpan.FromSeconds(0.5f));
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
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
        BattleScene = 7,
        CityScene = 8,
    }
    public enum TypeLoader
    {
        None = 0,
        WithProgressBar = 1,
        BlackOut = 2
    }
    public enum TypeDevice
    {
        None,
        PC,
        GAMEPAD,
        MOBILE,
    }
}
