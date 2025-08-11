using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "InitialBGMSoundsConfigSO", menuName = "ScriptableObjects/InitialConfig/InitialBGMSoundsConfig", order = 1)]
public class InitialBGMSoundsConfigSO : ScriptableObject
{
    public SerializedDictionary<string, BGMScenesData> BGMSceneData;
    [System.Serializable]
    public class BGMScenesData
    {
        public string defaultClip;
        public string otherClip;
    }
    public SerializedDictionary<string, BGMScenesData> Clone()
    {
        var clone = new SerializedDictionary<string, BGMScenesData>();

        foreach (var kvp in BGMSceneData)
        {
            clone[kvp.Key] = new BGMScenesData
            {
                defaultClip = kvp.Value.defaultClip,
                otherClip = kvp.Value.otherClip,
            };
        }

        return clone;
    }
}