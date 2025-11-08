using UnityEngine;

public class AutoLoader : MonoBehaviour
{
    void Start()
    {
        _ = ManagementLoaderScene.Instance.AutoCharge();
    }
}
