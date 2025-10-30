using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ManagementOpenCloseScene : MonoBehaviour
{
    public static ManagementOpenCloseScene Instance { get; private set; }
    public Animator openCloseSceneAnimator;
    public bool _finishLoad;
    public Action OnFinishOpenAnimation;
    public float speedFill;
    public float currentLoad;
    public Image loaderImage;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Charge()
    {
        _ = ValidateChargeIsComplete();
    }
    public async Awaitable ValidateChargeIsComplete()
    {
        loaderImage.fillAmount = 0;
        try
        {
            while (loaderImage.fillAmount < 1)
            {
                float value = currentLoad / 100;
                loaderImage.fillAmount = Mathf.MoveTowards(loaderImage.fillAmount, value, speedFill * Time.deltaTime);
                await Awaitable.NextFrameAsync();
            }
            _ = FinishLoad();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void AdjustLoading(float amount)
    {
        currentLoad = amount;
    }
    public async Awaitable FinishLoad()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            while (openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                await Awaitable.NextFrameAsync();
            }
            openCloseSceneAnimator.SetBool("Out", false);
            while (true)
            {
                if (openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).IsName("OpenCloseSceneOpen") && openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                {
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05));
            }
            loaderImage.fillAmount = 0;
            OnFinishOpenAnimation?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable WaitFinishCloseAnimation()
    {
        try
        {
            while (openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                await Awaitable.NextFrameAsync();
            }
            await Awaitable.NextFrameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}