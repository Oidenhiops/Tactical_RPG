using UnityEngine;

public class Block : MonoBehaviour
{
    public TypeBlock typeBlock;
    public GameObject blockGrid;
    public enum TypeBlock
    {
        None = 0,
        Normal = 1,
        Spawn = 2
    }
}
