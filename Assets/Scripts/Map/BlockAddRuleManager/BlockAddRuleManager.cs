using UnityEngine;

public class BlockAddRuleManager : MonoBehaviour
{
    public static BlockAddRuleManager Instance { get; private set; }
    public Block blockToAddRule;
    void Awake()
    {
        if (Instance == null) Instance = this;
    }
}
