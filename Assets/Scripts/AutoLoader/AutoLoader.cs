using UnityEngine;

public class AutoLoader : MonoBehaviour
{
    void Start()
    {
        _ = AutoCharge();
    }
    public async Awaitable AutoCharge()
    {
        ManagementOpenCloseScene.Instance.Charge();
        int i = 0;
        while (true)
        {
            ManagementOpenCloseScene.Instance.AdjustLoading(10 * i);
            await Awaitable.NextFrameAsync();
            i++;
            if (i > 10) break;
        }
    }
}
