using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManagerHelper : MonoBehaviour
{
    [SerializeField] Animator _unloadAnimator;
    GameObject audioBoxInstance;
    public void ChangeScene(int typeScene)
    {
        GameManager.TypeScene scene = (GameManager.TypeScene)typeScene;
        GameManager.Instance.ChangeSceneSelector(scene);
    }
    public void SaveGame()
    {
        GameData.Instance.SaveGameData();
    }
    public void PlayASound(SoundsDBSO.TypeSound typeSound ,string soundId)
    {
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(typeSound, soundId));
    }
    public void PlayASound(SoundsDBSO.TypeSound typeSound ,string soundId, float initialRandomPitch)
    {
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(typeSound, soundId), initialRandomPitch, false);
    }
    public void PlayASoundButton(string soundId)
    {
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, soundId), 1, false);
    }
    public void PlayASoundButtonUniqueInstance(string soundId)
    {
        if (audioBoxInstance == null)
        {
            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, soundId), 1, false, out GameObject audioBox);
            audioBoxInstance = audioBox;
        }
    }
    public void VibrateGamePad()
    {
        if (GameManager.Instance.currentDevice == GameManager.TypeDevice.GAMEPAD)
        {
            var gamepad = Gamepad.current;
            Gamepad.current.SetMotorSpeeds(0.5f, 0.5f);
            StartCoroutine(StopVibration(gamepad));
        }
    }
    IEnumerator StopVibration(Gamepad gamepad)
    {
        if (gamepad != null)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
    public void SetAudioMixerData()
    {
        AudioManager.Instance.SetAudioMixerData();
    }
    public void UnloadScene()
    {
        string sceneForUnload = ValidateScene();
        _= UnloadSceneByName(sceneForUnload);
    }
    public string ValidateScene()
    {
        int sceneCount = SceneManager.sceneCount;
        List<string> scenes = new List<string>();
        for (int i = 0; i < sceneCount; i++)
        {
            scenes.Add(SceneManager.GetSceneAt(i).name);
        }
        if (scenes.Contains("CreditsScene")) return "CreditsScene";
        return "OptionsScene";
    }
    public async Awaitable UnloadSceneByName(string sceneForUnload)
    {
        try
        {
            _unloadAnimator.SetBool("exit", true);
            await Task.Delay(TimeSpan.FromSeconds(0.25f));
            while (GameManager.Instance.openCloseScene.openCloseSceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
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
            if (sceneForUnload == "OptionsScene")
            {
                Time.timeScale = 1;
                GameManager.Instance.isPause = false;
            }
            await Task.Delay(TimeSpan.FromSeconds(0.25f));
            _ = SceneManager.UnloadSceneAsync(sceneForUnload);
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
        }
    }
}
