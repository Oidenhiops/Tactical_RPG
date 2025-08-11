using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManagementOpenCloseScene : MonoBehaviour
{
    public Animator openCloseSceneAnimator;
    public bool _finishLoad;
    public Action<bool>OnFinishLoadChange;
    public float speedFill;
    public GameObject[] characters;
    public bool finishLoad
    {
        get => _finishLoad;
        set
        {
            if (_finishLoad != value)
            {
                _finishLoad = value;
                OnFinishLoadChange?.Invoke(_finishLoad);
            }
        }
    }
    float _currentLoad = 0;
    public Image loaderImage;
    void Start()
    {
        ResetValues();
        GameManager.Instance.OnChangeScene += Charge;
        Charge();
    }
    void OnDestroy()
    {
        GameManager.Instance.OnChangeScene -= Charge;
    }
    public void Charge()
    {
        StartCoroutine(ValidateChargeIsComplete());
    }
    public IEnumerator ValidateChargeIsComplete()
    {
        while (true)
        {
            if (!finishLoad)
            {
                float value = _currentLoad / 100;
                loaderImage.fillAmount = Mathf.MoveTowards(loaderImage.fillAmount, value, speedFill * Time.unscaledDeltaTime);
                if (loaderImage.fillAmount == 1)
                {
                    finishLoad = true;
                    _ = FinishLoad();
                    break;
                }
            }
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
    public IEnumerator AutoCharge()
    {
        while (true)
        {
            if (_currentLoad >= 100)
            {
                break;
            }
            _currentLoad += 20;
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }
    public void AdjustLoading(float amount)
    {
        _currentLoad = amount;
    }
    public async Awaitable FinishLoad()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            while (openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.05));
            }
            Scene scene = SceneManager.GetSceneByName("HomeScene");
            if (scene.IsValid() && scene.isLoaded)
            {
                MenuHelper menuHelper = FindAnyObjectByType<MenuHelper>();
                if (menuHelper != null)
                {
                    menuHelper.SelectButton();
                }
            }
            openCloseSceneAnimator.SetBool("Out", false);
            await Task.Delay(TimeSpan.FromSeconds(1));
            loaderImage.fillAmount = 0;
            GameManager.Instance.startGame = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Task.Delay(TimeSpan.FromSeconds(0.05));
        }
    }
    public async Awaitable WaitFinishCloseAnimation()
    {
        try
        {
            while (openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.05));
            }
            ResetValues();
            await Task.Delay(TimeSpan.FromSeconds(0.05));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Task.Delay(TimeSpan.FromSeconds(0.05));
        }
    }
    public void ResetValues()
    {
        try
        {
            loaderImage.fillAmount = 0;
            _currentLoad = 0;
            finishLoad = false;
            if (GameManager.Instance.currentScene == "HomeScene" || GameManager.Instance.currentScene == "")
            {
                StartCoroutine(AutoCharge());
            }
        }
        catch (Exception e)
        {
            print(e);
        }
    }
}