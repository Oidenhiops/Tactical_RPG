using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }
    public DialogBaseSO dialogBaseSO;
    public GameObject charPrefab;
    public Transform charsContainer;
    public Animator menuAnimator;
    public CharacterActions inputActions;
    public bool skipDialog;
    [NaughtyAttributes.Button]
    public void TestText()
    {
        _ = ShowText();
    }
    void Awake()
    {
        Instance = this;
    }
    public void OnEnable()
    {
        inputActions = new CharacterActions();
        inputActions.Enable();
        inputActions.CharacterInputs.SkipDialog.performed += ctx => skipDialog = true;
    }
    public void OnDisable()
    {
        
    }
    public async Awaitable ShowText()
    {
        foreach (Transform child in charsContainer)
        {
            Destroy(child.gameObject);
        }
        await Awaitable.NextFrameAsync();
        foreach (var dialogText in dialogBaseSO.dialogLines)
        {
            List<string> words = new List<string>(GameData.Instance.GetDialog(dialogText.textId, GameData.TypeLOCS.Dialogs).dialog.Split(' '));
            for (int w = 0; w < words.Count; w++)
            {
                if (WordNeedJumpLine(words[w], out int amountChars))
                {
                    for (int c = 0; c < amountChars; c++)
                    {
                        var jumpChar = Instantiate(charPrefab, charsContainer).GetComponent<DialogChar>();
                        jumpChar.SetText(" ");
                    }
                }
                for (int c = 0; c < words[w].Length; c++)
                {
                    if (words[w][c] != ' ' && words[w][c].ToString() != "")
                    {
                        var dialogChar = Instantiate(charPrefab, charsContainer).GetComponent<DialogChar>();
                        dialogChar.typeAnimates = dialogText.typeAnimates;
                        dialogChar.SetText(words[w][c].ToString());
                        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TypeSound"), 1, false);
                        if (!skipDialog)
                        {
                            await Awaitable.WaitForSecondsAsync(dialogText.timeBetweenChars);
                        }
                    }
                }
                var space = Instantiate(charPrefab, charsContainer).GetComponent<DialogChar>();
                space.SetText(" ");
                if (w == words.Count - 1 && dialogText.banners.Count > 0)
                {
                    foreach (var banner in dialogBaseSO.dialogLines.ElementAt(dialogBaseSO.dialogLines.Count - 1).banners)
                    {
                        dialogBaseSO.MakeBannerFunction(banner.bannerFunction);
                    }
                    skipDialog = false;
                    while (true)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                }
            }
            skipDialog = false;
        }
    }
    bool WordNeedJumpLine(string word, out int amountChars)
    {
        int lineNumber = charsContainer.childCount / 48;
        if (charsContainer.childCount - (lineNumber * 48) + word.Length > 48)
        {
            amountChars = Math.Abs(charsContainer.childCount - (lineNumber * 48) - 48);
            return true;
        }
        amountChars = 0;
        return false;
    }
    public bool CanShowDialogs()
    {
        return dialogBaseSO != null && menuAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    }
}
