using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using AYellowpaper.SerializedCollections;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    string nameSaveData = "SaveData.json";
    public SaveData saveData = new SaveData();
    public SystemData systemData = new SystemData();
    public InitialBGMSoundsConfigSO initialBGMSoundsConfigSO;
    public CharacterDataDBSO charactersDataDBSO;
    public ItemsDBSO itemsDBSO;
    public Dictionary<TypeLOCS, List<string[]>> locs = new Dictionary<TypeLOCS, List<string[]>>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameManager.Instance.currentScene = GameManager.TypeScene.HomeScene.ToString();
            _ = LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async Awaitable LoadData()
    {
        try
        {
            GetAllResolutions();
            CheckFileExistance(DataPath());
            saveData = ReadDataFromJson();
            LoadLOCS();
            InitializeResolutionData();
            InitializeBGM();
            InitializeBagItems();
            InitializeCharacterItems();
            Application.targetFrameRate = saveData.configurationsInfo.FpsLimit;
            await InitializeAudioMixerData();
            saveData.configurationsInfo.canShowFps = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }

    private void InitializeBGM()
    {
        if (saveData.bgmSceneData.TryGetValue(SceneManager.GetActiveScene().name, out InitialBGMSoundsConfigSO.BGMScenesData bgmScenesData))
        {
            AudioManager.Instance.ChangeBGM(bgmScenesData);
        }
    }
    void InitializeCharacterItems()
    {
        foreach (CharacterData characterData in saveData.characters)
        {
            foreach (KeyValuePair<CharacterData.CharacterItemInfo, CharacterData.CharacterItem> item in characterData.items)
            {
                if (item.Value.itemId != 0)
                {
                    item.Value.itemBaseSO = itemsDBSO.data[item.Value.itemId];
                }
            }
        }
    }
    void InitializeBagItems()
    {
        foreach (KeyValuePair<int, CharacterData.CharacterItem> item in saveData.bagItems)
        {
            if (item.Value.itemId != 0)
            {
                item.Value.itemBaseSO = itemsDBSO.data[item.Value.itemId];
            }
        }
    }
    private void GetAllResolutions()
    {
        Resolution[] resolutions = Screen.resolutions;
        Array.Reverse(resolutions);
        foreach (Resolution res in resolutions)
        {
            systemData.allResolutions.Add(new ResolutionsInfo(res.width, res.height));
        }
    }
    void LoadLOCS()
    {
        try
        {
            TextAsset locsSystem = Resources.Load<TextAsset>("LOCS/LOC_System");
            locs.Add(TypeLOCS.System, TransformCSV(locsSystem));
            TextAsset locsItems = Resources.Load<TextAsset>("LOCS/LOC_Items");
            locs.Add(TypeLOCS.Items, TransformCSV(locsItems));
        }
        catch
        {
            Debug.LogError("No se encontro el archivo LOC_System");
        }
    }
    List<string[]> TransformCSV(TextAsset textAsset)
    {
        string[] lines = textAsset.text.Split('\n');
        List<string[]> textData = new List<string[]>();
        foreach (string line in lines)
        {
            string[] columns = line.Split(';');
            textData.Add(columns);
        }
        return textData;
    }
    public string GetDialog(int id, TypeLOCS typeLOCS)
    {
        if (locs.TryGetValue(typeLOCS, out List<string[]> dialogs))
        {
            int languageIndex = 0;
            for (int i = 0; i < dialogs[0].Length; i++)
            {
                if (dialogs[0][i] == saveData.configurationsInfo.currentLanguage.ToString())
                {
                    languageIndex = i;
                    break;
                }
            }
            return dialogs[id][languageIndex];
        }
        else
        {
            return $"NTF {typeLOCS}: {id}";
        }
    }
    public void ChangeLanguage(TypeLanguage language)
    {
        saveData.configurationsInfo.currentLanguage = language;
        SaveGameData();
    }
    void InitializeResolutionData()
    {
        if (GameManager.Instance.currentDevice == GameManager.TypeDevice.PC)
        {
            Screen.SetResolution(
                saveData.configurationsInfo.resolutionConfiguration.currentResolution.width,
                saveData.configurationsInfo.resolutionConfiguration.currentResolution.height,
                saveData.configurationsInfo.resolutionConfiguration.isFullScreen
            );
        }
        else
        {
            Screen.SetResolution(
                Screen.width,
                Screen.height,
                true
            );
        }
    }
    async Awaitable InitializeAudioMixerData()
    {
        try
        {
            await Awaitable.NextFrameAsync();
            float decibelsBGM = 20 * Mathf.Log10(saveData.configurationsInfo.soundConfiguration.BGMalue / 100);
            float decibelsSFX = 20 * Mathf.Log10(saveData.configurationsInfo.soundConfiguration.SFXalue / 100);
            if (saveData.configurationsInfo.soundConfiguration.BGMalue == 0) decibelsBGM = -80;
            if (saveData.configurationsInfo.soundConfiguration.SFXalue == 0) decibelsSFX = -80;
            AudioManager.Instance.audioMixer.SetFloat(AudioManager.TypeSound.BGM.ToString(), decibelsBGM);
            AudioManager.Instance.audioMixer.SetFloat(AudioManager.TypeSound.SFX.ToString(), decibelsSFX);
            if (saveData.configurationsInfo.soundConfiguration.isMute)
            {
                AudioManager.Instance.audioMixer.SetFloat(AudioManager.TypeSound.Master.ToString(), -80f);
            }
            else
            {
                GameManager.Instance.StartCoroutine(AudioManager.Instance.FadeIn());
            }
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Awaitable.NextFrameAsync();
        }
    }
    public void SetStartingData()
    {
        SaveData dataInfo = new SaveData();
        dataInfo.configurationsInfo.currentLanguage = TypeLanguage.English;
        SetStartingDataSound(ref dataInfo);
        GetInitialConfigBGMS(ref dataInfo);
        SetStartingCharacter(ref dataInfo);
        dataInfo.bagItems = new SerializedDictionary<int, CharacterData.CharacterItem>()
        {
            {0, null},
            {1, null},
            {2, null},
            {3, null},
            {4, null},
            {5, null},
            {6, null},
            {7, null},
            {8, null},
            {9, null},
            {10, null},
            {11, null},
            {12, null},
            {13, null},
            {14, null},
            {15, null},
        };
        GetAllResolutions();
        if (GameManager.Instance.currentDevice == GameManager.TypeDevice.PC) SetStartingResolution(ref dataInfo);
        saveData = dataInfo;
        SaveGameData();
    }
    public void SetStartingCharacter(ref SaveData dataInfo)
    {
        for (int i = 0; i < 20; i++)
        {
            CharacterData character = new CharacterData
            {
                id = 0,
                name = "Oiden " + i,
                level = 1,
            };
            character.statistics = charactersDataDBSO.data[character.id].CloneStatistics();
            foreach (KeyValuePair<CharacterData.TypeStatistic, CharacterData.Statistic> statistic in character.statistics)
            {
                statistic.Value.baseValue = i;
                statistic.Value.RefreshValue();
                statistic.Value.SetMaxValue();
            }
            dataInfo.characters.Add(character);
        }
    }
    private void GetInitialConfigBGMS(ref SaveData dataInfo)
    {
        dataInfo.bgmSceneData = initialBGMSoundsConfigSO.Clone();
    }

    void SetStartingDataSound(ref SaveData dataInfo)
    {
        dataInfo.configurationsInfo.soundConfiguration.MASTERValue = 25;
        dataInfo.configurationsInfo.soundConfiguration.BGMalue = 25;
        dataInfo.configurationsInfo.soundConfiguration.SFXalue = 25;
    }
    void SetStartingResolution(ref SaveData dataInfo)
    {
        Screen.SetResolution(systemData.allResolutions[0].width, systemData.allResolutions[0].height, true);
        dataInfo.configurationsInfo.resolutionConfiguration.isFullScreen = true;
        dataInfo.configurationsInfo.resolutionConfiguration.currentResolution = new ResolutionsInfo(
            systemData.allResolutions[0].width,
            systemData.allResolutions[0].height);
    }
    [NaughtyAttributes.Button]
    public void SaveGameData()
    {
        WriteDataToJson();
    }
    void CheckFileExistance(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
            SetStartingData();
            string dataString = JsonUtility.ToJson(saveData);
            File.WriteAllText(filePath, dataString);
        }
    }
    SaveData ReadDataFromJson()
    {
        string dataString;
        string jsonFilePath = DataPath();
        dataString = File.ReadAllText(jsonFilePath);
        saveData = JsonUtility.FromJson<SaveData>(dataString);
        return saveData;
    }
    public void WriteDataToJson()
    {
        try
        {
            string jsonFilePath = DataPath();
            string dataString = JsonUtility.ToJson(saveData);
            File.WriteAllText(jsonFilePath, dataString);
        }
        catch (Exception e)
        {
            print(e);
        }
    }
    string DataPath()
    {
        if (Directory.Exists(Application.persistentDataPath))
        {
            return Path.Combine(Application.persistentDataPath, nameSaveData);
        }
        return Path.Combine(Application.streamingAssetsPath, nameSaveData);
    }
    [Serializable] public class SaveData
    {
        public List<CharacterData> characters = new List<CharacterData>();
        public SerializedDictionary<int, CharacterData.CharacterItem> bagItems = new SerializedDictionary<int, CharacterData.CharacterItem>();
        public ConfigurationsInfo configurationsInfo = new ConfigurationsInfo();
        public SerializedDictionary<string, InitialBGMSoundsConfigSO.BGMScenesData> bgmSceneData = new SerializedDictionary<string, InitialBGMSoundsConfigSO.BGMScenesData>();
    }
    [Serializable] public class ConfigurationsInfo
    {
        public TypeLanguage _currentLanguage;
        public Action<TypeLanguage> OnLanguageChange;
        public TypeLanguage currentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnLanguageChange?.Invoke(_currentLanguage);
                }
            }
        }
        public bool _canShowFps;
        public Action<bool> OnCanShowFpsChange;
        public bool canShowFps
        {
            get => _canShowFps;
            set
            {
                if (_canShowFps != value)
                {
                    _canShowFps = value;
                    OnCanShowFpsChange?.Invoke(_canShowFps);
                }
            }
        }
        public int FpsLimit = 0;
        public ResolutionConfiguration resolutionConfiguration = new ResolutionConfiguration();
        public SoundConfiguration soundConfiguration = new SoundConfiguration();
    }
    [Serializable] public class SoundConfiguration
    {
        public bool isMute = false;
        public float MASTERValue;
        public float BGMalue;
        public float SFXalue;
    }
    [Serializable] public class ResolutionConfiguration
    {
        public bool isFullScreen = false;
        public ResolutionsInfo currentResolution;
    }
    [Serializable] public class ResolutionsInfo
    {
        public int width = 0;
        public int height = 0;
        public ResolutionsInfo(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }
    [Serializable] public class SystemData
    {
        public List<ResolutionsInfo> allResolutions = new List<ResolutionsInfo>();
    }
    public enum TypeLanguage
    {
        English = 0,
        Español = 1,
    }
    public enum TypeLOCS
    {
        None = 0,
        System = 1,
        Dialogs = 2,
        Items = 3
    }
}