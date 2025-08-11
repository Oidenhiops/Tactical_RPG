using System.Collections;
using UnityEngine;

public class AutoLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(AutoCharge());
    }
    public IEnumerator AutoCharge()
    {
        int i = 0;
        while (true)
        {
            GameManager.Instance.openCloseScene.AdjustLoading(10 * i);
            yield return new WaitForSeconds(0.1f);
            i++;
            if (i > 10) break;
        }
    }
}
