using UnityEngine;

[CreateAssetMenu(fileName = "MapBlocksInfo", menuName = "ScriptableObjects/Map/MapBlocksInfo", order = 1)]
public class MapBlocksInfoSO : ScriptableObject
{
    public Texture2D blocksTexture;
    public Texture2D stairsTexture;
}
