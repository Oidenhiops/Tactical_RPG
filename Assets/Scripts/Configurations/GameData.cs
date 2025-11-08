using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using AYellowpaper.SerializedCollections;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    public GameDataInfo gameDataInfo = new GameDataInfo();
    public SystemDataInfo systemDataInfo = new SystemDataInfo();
    public List<ResolutionsInfo> allResolutions = new List<ResolutionsInfo>();
    public Dictionary<TypeLOCS, Dictionary<string, string[]>> locs = new Dictionary<TypeLOCS, Dictionary<string, string[]>>();
    public InitialBGMSoundsConfigSO initialBGMSoundsConfigSO;
    public CharacterDataDBSO charactersDataDBSO;
    public ItemsDBSO itemsDBSO;
    public SkillsDBSO skillsDBSO;
    public CharactersSkinDBSO charactersSkinDBSO;
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
            CheckFileExistance();
            LoadGameDataInfo();
            LoadSystemDataInfo();
            LoadLOCS();
            InitializeResolutionData();
            InitializeBGM();
            LoadCharacterDataInfo();
            Application.targetFrameRate = systemDataInfo.configurationsInfo.FpsLimit;
            await InitializeAudioMixerData();
            systemDataInfo.configurationsInfo.canShowFps = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void LoadCharacterDataInfo()
    {
        InitializeSkinsData();
        InitializeBagItems();
        InitializeCharacterItems();
        InitializeCharacterSkills();
    }
    public void LoadGameDataInfo()
    {
        ReadGameDataFromJson();
        LoadCharacterDataInfo();
    }
    public void LoadSystemDataInfo()
    {
        systemDataInfo = ReadSystemDataFromJson();
    }
    private void InitializeBGM()
    {
        if (systemDataInfo.bgmSceneData.TryGetValue(SceneManager.GetActiveScene().name, out InitialBGMSoundsConfigSO.BGMScenesData bgmScenesData))
        {
            AudioManager.Instance.ChangeBGM(bgmScenesData);
        }
    }
    void InitializeCharacterItems()
    {
        foreach (GameDataSlot gameDataSlot in gameDataInfo.gameDataSlots)
        {
            foreach (KeyValuePair<string, CharacterData> characterData in gameDataSlot.characters)
            {
                foreach (KeyValuePair<CharacterData.CharacterItemInfo, CharacterData.CharacterItem> item in characterData.Value.items)
                {
                    if (item.Value.itemId != 0)
                    {
                        item.Value.itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][item.Value.itemId];
                    }
                }
            }
        }
    }
    void InitializeCharacterSkills()
    {
        foreach (GameDataSlot gameDataSlot in gameDataInfo.gameDataSlots)
        {
            foreach (KeyValuePair<string, CharacterData> characterData in gameDataSlot.characters)
            {
                foreach (KeyValuePair<ItemBaseSO.TypeWeapon, UnityEngine.Rendering.SerializedDictionary<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>>> item in characterData.Value.skills)
                {
                    foreach (KeyValuePair<SkillsBaseSO.TypeSkill, UnityEngine.Rendering.SerializedDictionary<string, CharacterData.CharacterSkillInfo>> skill in item.Value)
                    {
                        foreach (KeyValuePair<string, CharacterData.CharacterSkillInfo> skillInfo in skill.Value)
                        {
                            if (skillInfo.Value.skillId != "")
                            {
                                skillInfo.Value.skillsBaseSO = skillsDBSO.data[skillInfo.Value.skillId];
                            }
                        }
                    }
                }
            }
        }
    }
    void InitializeSkinsData()
    {
        foreach (GameDataSlot gameDataSlot in gameDataInfo.gameDataSlots)
        {
            foreach (KeyValuePair<string, CharacterData> characterData in gameDataSlot.characters)
            {
                characterData.Value.characterSkinData = new CharacterData.CharacterSkinData
                {
                    atlas = charactersSkinDBSO.data[characterData.Value.characterId][characterData.Value.characterSkinId].atlas,
                    atlasHands = charactersSkinDBSO.data[characterData.Value.characterId][characterData.Value.characterSkinId].atlasHands
                };
            }
        }
    }
    void InitializeBagItems()
    {
        foreach (GameDataSlot gameDataSlot in gameDataInfo.gameDataSlots)
        {
            foreach (KeyValuePair<int, CharacterData.CharacterItem> item in gameDataSlot.bagItems)
            {
                if (item.Value.itemId != 0)
                {
                    item.Value.itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][item.Value.itemId];
                }
            }
        }
    }
    private void GetAllResolutions()
    {
        Resolution[] resolutions = Screen.resolutions;
        Array.Reverse(resolutions);
        foreach (Resolution res in resolutions)
        {
            allResolutions.Add(new ResolutionsInfo(res.width, res.height));
        }
    }
    void LoadLOCS()
    {
        try
        {
            TextAsset locsSystem = Resources.Load<TextAsset>("LOCS/LOC_System");
            locs.Add(TypeLOCS.System, TransformCSV(locsSystem));
        }
        catch
        {
            Debug.LogError("No se encontro el archivo LOC_System");
        }
        try
        {
            TextAsset locsItems = Resources.Load<TextAsset>("LOCS/LOC_Items");
            locs.Add(TypeLOCS.Items, TransformCSV(locsItems));
        }
        catch
        {
            Debug.LogError("No se encontro el archivo LOC_Items");
        }
        try
        {
            TextAsset locsSkills = Resources.Load<TextAsset>("LOCS/LOC_Skills");
            locs.Add(TypeLOCS.Skills, TransformCSV(locsSkills));
        }
        catch
        {
            Debug.LogError("No se encontro el archivo LOC_Skills");
        }
        try
        {
            TextAsset locsDialogs = Resources.Load<TextAsset>("LOCS/LOC_Dialogs");
            locs.Add(TypeLOCS.Dialogs, TransformCSV(locsDialogs));
        }
        catch
        {
            Debug.LogError("No se encontro el archivo LOC_Dialogs");
        }
    }
    Dictionary<string, string[]> TransformCSV(TextAsset textAsset)
    {
        string[] lines = textAsset.text.Split('\n');
        List<string[]> textData = new List<string[]>();
        foreach (string line in lines)
        {
            string[] columns = line.Split(';');
            textData.Add(columns);
        }
        Dictionary<string, string[]> data = new Dictionary<string, string[]>();
        foreach (string[] text in textData)
        {
            data.Add(text[0], text);
        }
        return data;
    }
    public DialogData GetDialog(string id, TypeLOCS typeLOCS)
    {
        if (locs.TryGetValue(typeLOCS, out Dictionary<string, string[]> dialogs) && dialogs.ContainsKey(id))
        {
            int languageIndex = 0;
            for (int i = 0; i < dialogs["ID"].Length; i++)
            {
                if (dialogs["ID"][i] == systemDataInfo.configurationsInfo.currentLanguage.ToString())
                {
                    languageIndex = i;
                    break;
                }
            }
            return new DialogData
            {
                description = dialogs[id][languageIndex + 1],
                dialog = dialogs[id][languageIndex]
            };
        }
        return new DialogData
        {
            description = $"NTF {typeLOCS}: {id}",
            dialog = $"NTF {typeLOCS}: {id}"
        };
    }
    public void ChangeLanguage(TypeLanguage language)
    {
        systemDataInfo.configurationsInfo.currentLanguage = language;
        SaveSystemData();
    }
    void InitializeResolutionData()
    {
        if (GameManager.Instance.currentDevice == GameManager.TypeDevice.PC)
        {
            Screen.SetResolution(
                systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution.width,
                systemDataInfo.configurationsInfo.resolutionConfiguration.currentResolution.height,
                systemDataInfo.configurationsInfo.resolutionConfiguration.isFullScreen
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
            float decibelsBGM = 20 * Mathf.Log10(systemDataInfo.configurationsInfo.soundConfiguration.BGMalue / 100);
            float decibelsSFX = 20 * Mathf.Log10(systemDataInfo.configurationsInfo.soundConfiguration.SFXalue / 100);
            if (systemDataInfo.configurationsInfo.soundConfiguration.BGMalue == 0) decibelsBGM = -80;
            if (systemDataInfo.configurationsInfo.soundConfiguration.SFXalue == 0) decibelsSFX = -80;
            AudioManager.Instance.audioMixer.SetFloat(AudioManager.TypeSound.BGM.ToString(), decibelsBGM);
            AudioManager.Instance.audioMixer.SetFloat(AudioManager.TypeSound.SFX.ToString(), decibelsSFX);
            if (systemDataInfo.configurationsInfo.soundConfiguration.isMute)
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
        }
    }
    public void SetStartingData()
    {
        GameDataInfo gameData = new GameDataInfo();
        gameData.gameDataSlots = new List<GameDataSlot>()
        {
            new GameDataSlot(),
            new GameDataSlot(),
            new GameDataSlot()
        };
        SystemDataInfo systemData = new SystemDataInfo();
        systemDataInfo.configurationsInfo.currentLanguage = TypeLanguage.English;
        SetStartingDataSound(ref systemData);
        GetInitialConfigBGMS(ref systemData);
        if (GameManager.Instance.currentDevice == GameManager.TypeDevice.PC) SetStartingResolution(ref systemData);
        gameDataInfo = gameData;
        systemDataInfo = systemData;
        SaveGameData();
    }
    public void SetStartingItems()
    {
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems = new SerializedDictionary<int, CharacterData.CharacterItem>()
        {
            {0, new CharacterData.CharacterItem()},
            {1, new CharacterData.CharacterItem()},
            {2, new CharacterData.CharacterItem()},
            {3, new CharacterData.CharacterItem()},
            {4, new CharacterData.CharacterItem()},
            {5, new CharacterData.CharacterItem()},
            {6, new CharacterData.CharacterItem()},
            {7, new CharacterData.CharacterItem()},
            {8, new CharacterData.CharacterItem()},
            {9, new CharacterData.CharacterItem()},
            {10, new CharacterData.CharacterItem()},
            {11, new CharacterData.CharacterItem()},
            {12, new CharacterData.CharacterItem()},
            {13, new CharacterData.CharacterItem()},
            {14, new CharacterData.CharacterItem()},
            {15, new CharacterData.CharacterItem()},
        };

        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[0].itemId = itemsDBSO.data[ItemBaseSO.TypeObject.None][1].id;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[0].itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][1];
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[0].itemStatistics = itemsDBSO.data[ItemBaseSO.TypeObject.None][1].itemStatistics;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[1].itemId = itemsDBSO.data[ItemBaseSO.TypeObject.None][2].id;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[1].itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][2];
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[1].itemStatistics = itemsDBSO.data[ItemBaseSO.TypeObject.None][2].itemStatistics;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[2].itemId = itemsDBSO.data[ItemBaseSO.TypeObject.None][321].id;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[2].itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][321];
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[2].itemStatistics = itemsDBSO.data[ItemBaseSO.TypeObject.None][321].itemStatistics;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[3].itemId = itemsDBSO.data[ItemBaseSO.TypeObject.None][322].id;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[3].itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][322];
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[3].itemStatistics = itemsDBSO.data[ItemBaseSO.TypeObject.None][322].itemStatistics;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[4].itemId = itemsDBSO.data[ItemBaseSO.TypeObject.None][323].id;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[4].itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][323];
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[4].itemStatistics = itemsDBSO.data[ItemBaseSO.TypeObject.None][323].itemStatistics;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[5].itemId = itemsDBSO.data[ItemBaseSO.TypeObject.None][324].id;
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[5].itemBaseSO = itemsDBSO.data[ItemBaseSO.TypeObject.None][324];
        gameDataInfo.gameDataSlots[systemDataInfo.currentGameDataIndex].bagItems[5].itemStatistics = itemsDBSO.data[ItemBaseSO.TypeObject.None][324].itemStatistics;
    }
    private void GetInitialConfigBGMS(ref SystemDataInfo dataInfo)
    {
        dataInfo.bgmSceneData = initialBGMSoundsConfigSO.Clone();
    }

    void SetStartingDataSound(ref SystemDataInfo dataInfo)
    {
        dataInfo.configurationsInfo.soundConfiguration.MASTERValue = 25;
        dataInfo.configurationsInfo.soundConfiguration.BGMalue = 25;
        dataInfo.configurationsInfo.soundConfiguration.SFXalue = 25;
    }
    void SetStartingResolution(ref SystemDataInfo dataInfo)
    {
        Screen.SetResolution(allResolutions[0].width, allResolutions[0].height, true);
        dataInfo.configurationsInfo.resolutionConfiguration.isFullScreen = true;
        dataInfo.configurationsInfo.resolutionConfiguration.currentResolution = new ResolutionsInfo(allResolutions[0].width, allResolutions[0].height);
    }
    [NaughtyAttributes.Button]
    public void SaveGameData()
    {
        WriteGameDataToJson();
    }
    [NaughtyAttributes.Button]
    public void SaveSystemData()
    {
        WriteSystemDataToJson();
    }
    void CheckFileExistance()
    {
        if (!File.Exists(DataPath(TypeSaveData.SystemDataInfo)))
        {
            File.Create(DataPath(TypeSaveData.SystemDataInfo)).Close();
            SetStartingData();
            string gameDataString = JsonUtility.ToJson(gameDataInfo);
            string systemDataString = JsonUtility.ToJson(systemDataInfo);
            File.WriteAllText(DataPath(TypeSaveData.SystemDataInfo), gameDataString);
            File.WriteAllText(DataPath(TypeSaveData.SystemDataInfo), systemDataString);
        }
    }
    GameDataInfo ReadGameDataFromJson()
    {
        string dataString;
        string jsonFilePath = DataPath(TypeSaveData.GameDataInfo);
        dataString = File.ReadAllText(jsonFilePath);
        gameDataInfo = JsonUtility.FromJson<GameDataInfo>(dataString);
        return gameDataInfo;
    }
    SystemDataInfo ReadSystemDataFromJson()
    {
        string dataString;
        string jsonFilePath = DataPath(TypeSaveData.SystemDataInfo);
        dataString = File.ReadAllText(jsonFilePath);
        systemDataInfo = JsonUtility.FromJson<SystemDataInfo>(dataString);
        return systemDataInfo;
    }
    public void WriteGameDataToJson()
    {
        string jsonFilePath = DataPath(TypeSaveData.GameDataInfo);
        string dataString = JsonUtility.ToJson(gameDataInfo);
        File.WriteAllText(jsonFilePath, dataString);
    }
    public void WriteSystemDataToJson()
    {
        string jsonFilePath = DataPath(TypeSaveData.SystemDataInfo);
        string dataString = JsonUtility.ToJson(systemDataInfo);
        File.WriteAllText(jsonFilePath, dataString);
    }
    string DataPath(TypeSaveData typeSaveData)
    {
        if (Directory.Exists(Application.persistentDataPath))
        {
            return Path.Combine(Application.persistentDataPath, typeSaveData + ".json");
        }
        return Path.Combine(Application.streamingAssetsPath, typeSaveData + ".json");
    }
    [Serializable]
    public class GameDataInfo
    {
        public List<GameDataSlot> gameDataSlots = new List<GameDataSlot>();
    }
    [Serializable]
    public class GameDataSlot
    {
        public bool isUse = false;
        public string createdDate = "";
        public string lastSaveDate = "";
        public string principalCharacterName = "";
        public GameManager.TypeScene currentZone;
        public Vector3Int positionSave = Vector3Int.zero;
        public SerializedDictionary<string, CharacterData> characters = new SerializedDictionary<string, CharacterData>();
        public SerializedDictionary<string, CharacterData> dieCharacters = new SerializedDictionary<string, CharacterData>();
        public SerializedDictionary<int, CharacterData.CharacterItem> bagItems = new SerializedDictionary<int, CharacterData.CharacterItem>();
    }
    [Serializable]
    public class ConfigurationsInfo
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
    [Serializable]
    public class SoundConfiguration
    {
        public bool isMute = false;
        public float MASTERValue;
        public float BGMalue;
        public float SFXalue;
    }
    [Serializable]
    public class ResolutionConfiguration
    {
        public bool isFullScreen = false;
        public ResolutionsInfo currentResolution;
    }
    [Serializable]
    public class ResolutionsInfo
    {
        public int width = 0;
        public int height = 0;
        public ResolutionsInfo(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }
    [Serializable]
    public class SystemDataInfo
    {
        public int currentGameDataIndex = 0;
        public ConfigurationsInfo configurationsInfo = new ConfigurationsInfo();
        public SerializedDictionary<string, InitialBGMSoundsConfigSO.BGMScenesData> bgmSceneData = new SerializedDictionary<string, InitialBGMSoundsConfigSO.BGMScenesData>();
    }
    public class DialogData
    {
        public string description;
        public string dialog;
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
        Items = 3,
        Skills = 4,
        Chars = 5
    }
    public enum TypeSaveData
    {
        None = 0,
        GameDataInfo = 1,
        SystemDataInfo = 2
    }
}