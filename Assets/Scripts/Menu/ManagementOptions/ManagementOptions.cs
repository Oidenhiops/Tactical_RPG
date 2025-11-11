using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManagementOptions : MonoBehaviour, GameManagerHelper.IScene
{
    public static ManagementOptions Instance { get; private set; }
    public TMP_Dropdown dropdownLanguage;
    public TMP_Dropdown dropdownResolution;
    public SoundInfo[] soundInfo;
    public WindowModeButtonsInfo windowModeButtonsInfo;
    public ButtonsBackInfo[] buttonsBackInfos;
    public ButtonBackInfo buttonBackInfo;
    public GameObject homeButton;
    public GameObject muteCheck;
    public InputAction backButton;
    public GameManagerHelper gameManagerHelper;
    public GameObject lastButtonSelected;
    public Animator menuAnimator;
    public bool isMenuActive;
    void OnEnable()
    {
        _ = EnableMenu();
    }
    async Awaitable EnableMenu()
    {
        Instance = this;
        gameManagerHelper.sceneData = this;
        lastButtonSelected = EventSystem.current.currentSelectedGameObject;
        Time.timeScale = 0;
        GameManager.Instance.isPause = true;
        InitializeLanguageDropdown();
        InitializeResolutionDropdown();
        SetFullScreenButtonsSprite();
        SetVolumeSliders();
        buttonsBackInfos[0].buttonToBack.Select();
        backButton.started += BackHandle;
        backButton.Enable();
        muteCheck.SetActive(GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute);
        if (SceneManager.GetSceneByName("HomeScene").isLoaded) homeButton.SetActive(false);
        await Awaitable.NextFrameAsync();
        isMenuActive = true;
    }
    void OnDisable()
    {
        isMenuActive = false;
        backButton.started -= BackHandle;
        Time.timeScale = 1;
        GameManager.Instance.isPause = false;
    }
    public void InitializeLanguageDropdown()
    {
        dropdownLanguage.options.Clear();
        foreach (GameData.TypeLanguage language in Enum.GetValues(typeof(GameData.TypeLanguage)))
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData
            {
                text = language.ToString()
            };
            dropdownLanguage.options.Add(option);
        }
        for (int i = 0; i < dropdownLanguage.options.Count; i++)
        {
            if (dropdownLanguage.options[i].text == GameData.Instance.systemDataInfo.configurationsInfo.currentLanguage.ToString())
            {
                dropdownLanguage.value = i;
                break;
            }
        }
    }
    public void BackHandle(InputAction.CallbackContext context)
    {
        if (buttonBackInfo.button)
        {
            buttonBackInfo.button.interactable = true;
            buttonBackInfo.button.Select();
            buttonBackInfo.menu.SetActive(false);
            buttonBackInfo = new ButtonBackInfo();
            gameManagerHelper.PlayASoundButton("TouchButtonBack");
        }
        else
        {
            gameManagerHelper.PlayASoundButton("TouchButtonBack");
            gameManagerHelper.lastButtonSelected = lastButtonSelected;
            GameManager.Instance.UnloadAdditiveScene(GameManager.TypeScene.OptionsScene, this, lastButtonSelected);
        }
    }
    public void SetSelectedButtonToBack(int buttonId)
    {
        foreach (var buttonsBack in buttonsBackInfos)
        {
            if (buttonsBack.id == buttonId)
            {
                buttonsBackInfos[buttonId].buttonToBack.interactable = false;
                buttonBackInfo.button = buttonsBack.buttonToBack;
                buttonBackInfo.menu = buttonsBack.menu;
                if (buttonsBack.buttonsToSelect.Length > 0)
                {
                    if (buttonId == 0)
                    {
                        EventSystem.current.SetSelectedGameObject
                        (
                            GameManager.Instance.currentDevice == GameManager.TypeDevice.MOBILE ?
                            buttonsBackInfos[0].buttonsToSelect[1] : buttonsBackInfos[0].buttonsToSelect[0]
                        );
                    }
                    else
                    {
                        EventSystem.current.SetSelectedGameObject(buttonsBackInfos[buttonId].buttonsToSelect[0]);
                    }
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            else
            {
                buttonsBack.buttonToBack.interactable = true;
            }
        }
    }
    public void ChangeLanguage()
    {
        GameData.TypeLanguage language = (GameData.TypeLanguage)dropdownLanguage.value;
        GameData.Instance.ChangeLanguage(language);
        GameData.Instance.SaveSystemData();
    }
    public void InitializeResolutionDropdown()
    {
        dropdownResolution.options.Clear();
        foreach (var resolution in GameData.Instance.allResolutions)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData
            {
                text = $"{resolution.width}X{resolution.height}"
            };
            dropdownResolution.options.Add(option);
        }
        for (int i = 0; i < dropdownResolution.options.Count; i++)
        {
            GameData.ResolutionsInfo resolutionsInfo = GetCurrentResolution(dropdownResolution.options[i].text);
            if (resolutionsInfo.width == GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution.width &&
                resolutionsInfo.height == GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution.height)
            {
                dropdownResolution.value = i;
                break;
            }
        }
    }
    public void SetVolumeSliders()
    {
        FindSoundInfo(AudioManager.TypeSound.Master).slider.value = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.MASTERValue;
        FindSoundInfo(AudioManager.TypeSound.BGM).slider.value = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.BGMalue;
        FindSoundInfo(AudioManager.TypeSound.SFX).slider.value = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.SFXalue;
    }
    public SoundInfo FindSoundInfo(AudioManager.TypeSound typeSound)
    {
        foreach (var sound in soundInfo)
        {
            if (sound.typeSound == typeSound)
            {
                return sound;
            }
        }
        return null;
    }
    public SoundInfo FindSoundInfo(Slider slider)
    {
        foreach (var sound in soundInfo)
        {
            if (sound.slider == slider)
            {
                return sound;
            }
        }
        return null;
    }
    public void SetMixerValues()
    {
        AudioManager.Instance.SetAudioMixerData();
    }
    public void SetSoundValue(Slider slider)
    {
        if (isMenuActive)
        {
            switch (FindSoundInfo(slider).typeSound)
            {
                case AudioManager.TypeSound.Master:
                    GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.MASTERValue = slider.value;
                    break;
                case AudioManager.TypeSound.BGM:
                    GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.BGMalue = slider.value;
                    break;
                case AudioManager.TypeSound.SFX:
                    GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.SFXalue = slider.value;
                    break;
            }
            gameManagerHelper.PlayASoundButtonUniqueInstance("SliderButton");
            SetVolumeSliders();
            SetMixerValues();
        }
    }
    public void SetMute()
    {
        GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute = !GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute;
        muteCheck.SetActive(GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute);
        SetMixerValues();
    }
    public void SetFullScreen(bool isFullScreen)
    {
        GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.isFullScreen = isFullScreen;
        SetFullScreenButtonsSprite();
        Screen.SetResolution(
            GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution.width,
            GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution.height,
            GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.isFullScreen);
        GameData.Instance.SaveSystemData();
    }
    public void SetFullScreenButtonsSprite()
    {
        bool isFullScreen = GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.isFullScreen;
        if (isFullScreen)
        {
            windowModeButtonsInfo.buttonsImage[0].sprite = windowModeButtonsInfo.spriteOn;
            windowModeButtonsInfo.buttonsImage[1].sprite = windowModeButtonsInfo.spriteOff;
            return;
        }
        windowModeButtonsInfo.buttonsImage[0].sprite = windowModeButtonsInfo.spriteOff;
        windowModeButtonsInfo.buttonsImage[1].sprite = windowModeButtonsInfo.spriteOn;
    }
    public void ChangeResolution()
    {
        GameData.ResolutionsInfo currentResolution = GetCurrentResolution(dropdownResolution.options[dropdownResolution.value].text);
        GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution = currentResolution;
        Screen.SetResolution(
            currentResolution.width,
            currentResolution.height,
            GameData.Instance.systemDataInfo.configurationsInfo.resolutionConfiguration.isFullScreen);
        GameData.Instance.SaveSystemData();
    }
    public GameData.ResolutionsInfo GetCurrentResolution(string resolution)
    {
        int index = resolution.IndexOf("X");
        int width = int.Parse(resolution.Substring(0, index));
        int height = int.Parse(resolution.ToString().Substring(index + 1));
        return new GameData.ResolutionsInfo(width, height);
    }
    public bool AnimationEnded()
    {
        return menuAnimator.GetCurrentAnimatorStateInfo(0).IsName("MenuExit") && menuAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
    }
    public void PlayEndAnimation()
    {
        menuAnimator.SetBool("exit", true);
    }
    [Serializable]
    public class SoundInfo
    {
        public AudioManager.TypeSound typeSound;
        public Slider slider;
    }
    [Serializable]
    public class WindowModeButtonsInfo
    {
        public Sprite spriteOn;
        public Sprite spriteOff;
        public Image[] buttonsImage;
    }
    [Serializable]
    public class ControlsInfo
    {
        public GameManager.TypeDevice typeDevice;
        public GameObject container;
    }
    [Serializable]
    public class ButtonsBackInfo
    {
        public Button buttonToBack;
        public GameObject[] buttonsToSelect;
        public GameObject menu;
        public int id;
    }
    [Serializable] public class ButtonBackInfo
    {
        public Button button;
        public GameObject menu;
    }
}