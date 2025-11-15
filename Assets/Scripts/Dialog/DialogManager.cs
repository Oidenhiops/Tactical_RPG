using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour, GameManagerHelper.IScene
{
    public static DialogManager Instance { get; private set; }
    public DialogBaseSO dialogBaseSO;
    public GameObject charPrefab;
    public Transform charsContainer;
    public Transform bannerContainer;
    public GameObject bannerPrefab;
    public Animator menuAnimator;
    public Animator bannerAnimator;
    public CharacterActions inputActions;
    public bool nextDialog;
    public bool isResettingDialog;
    public CharacterData leftCharacter;
    public CharacterData rightCharacter;
    public GameObject nextDialogButton;
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
        inputActions.CharacterInputs.NextDialog.performed += OnHandleNextDialog;
        inputActions.Enable();
    }
    public void OnDisable()
    {
        inputActions.CharacterInputs.NextDialog.performed -= OnHandleNextDialog;
        inputActions.Disable();
    }
    public void OnHandleNextDialog(InputAction.CallbackContext context)
    {
        _ = NextDialogAction();
    }
    async Awaitable NextDialogAction()
    {
        nextDialog = true;
    }
    public async Awaitable InitializeCharacter(CharacterBase characterPlayer, CharacterBase characterNpc)
    {
        if (characterPlayer.positionInGrid.x < characterNpc.positionInGrid.x)
        {
            leftCharacter.character.initialDataSO = characterPlayer.initialDataSO;
            leftCharacter.isCharacterPlayer = true;
            rightCharacter.character.initialDataSO = characterNpc.initialDataSO;
        }
        else
        {
            leftCharacter.character.initialDataSO = characterNpc.initialDataSO;
            rightCharacter.character.initialDataSO = characterPlayer.initialDataSO;
            rightCharacter.isCharacterPlayer = true;
        }
        await leftCharacter.character.InitializeCharacter();
        await rightCharacter.character.InitializeCharacter();
        leftCharacter.characterImage.SetActive(true);
        rightCharacter.characterImage.SetActive(true);
    }
    public async Awaitable ShowText()
    {
        foreach (Transform child in charsContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in bannerContainer)
        {
            Destroy(child.gameObject);
        }
        await Awaitable.NextFrameAsync();
        nextDialogButton.SetActive(true);
        for (int d = 0; d < dialogBaseSO.dialogLines.Count; d++)
        {
            List<string> words = new List<string>(GameData.Instance.GetDialog(dialogBaseSO.dialogLines[d].textId, GameData.TypeLOCS.Dialogs).dialog.Split(' '));
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
                        dialogChar.typeAnimates = dialogBaseSO.dialogLines[d].typeAnimates;
                        dialogChar.SetText(words[w][c].ToString());
                        if (!nextDialog)
                        {
                            await Awaitable.WaitForSecondsAsync(dialogBaseSO.dialogLines[d].timeBetweenChars);
                            AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "TypeSound"), 1, false);
                        }
                    }
                }
                var space = Instantiate(charPrefab, charsContainer).GetComponent<DialogChar>();
                space.SetText(" ");
                if (w == words.Count - 1 && dialogBaseSO.dialogLines[d].banners.Count > 0)
                {
                    bannerContainer.gameObject.SetActive(true);
                    nextDialogButton.SetActive(false);
                    bannerAnimator.Play("Enter");
                    await Awaitable.NextFrameAsync();
                    while (true)
                    {
                        if (bannerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                        {
                            break;
                        }
                        await Awaitable.NextFrameAsync();
                    }
                    foreach (var banner in dialogBaseSO.dialogLines.Last().banners)
                    {
                        DialogBanner bannerML = Instantiate(bannerPrefab, bannerContainer).GetComponent<DialogBanner>();
                        bannerML.managementLanguage.id = banner.bannerTextId;
                        bannerML.dialogManager = this;
                        bannerML.newDialog = banner.newDialog;
                        bannerML.managementLanguage.RefreshDialog();
                    }
                    SetExplicitNavigation();
                    while (true)
                    {
                        if (bannerContainer.GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                        {
                            break;
                        }
                        await Awaitable.NextFrameAsync();
                    }
                    await Awaitable.NextFrameAsync();
                    EventSystem.current.SetSelectedGameObject(bannerContainer.GetChild(0).gameObject);
                    while (true)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                }
            }
        }
        nextDialog = false;
        nextDialogButton.SetActive(true);
        if (dialogBaseSO.dialogLines.Last().finishDialog)
        {
            AddNextDialogIndicator();
            while (true)
            {
                if (nextDialog)
                {
                    _ = CloseDialog();
                    break;
                }
                await Awaitable.NextFrameAsync();
            }
        }
        else if (dialogBaseSO.dialogLines.Last().banners.Count == 0 && dialogBaseSO.dialogLines.Last().otherDialog)
        {
            AddNextDialogIndicator();
            while (true)
            {
                if (nextDialog)
                {
                    _ = ResetTextByLoop(dialogBaseSO.dialogLines.Last().otherDialog);
                    break;
                }
                await Awaitable.NextFrameAsync();
            }
        }
        else if (dialogBaseSO.dialogLines.Last().dialogFunction)
        {
            AddNextDialogIndicator();
            while (true)
            {
                if (nextDialog)
                {
                    _ = dialogBaseSO.dialogLines.Last().dialogFunction.MakeBannerFunction(WorldManager.Instance.characterWorld);
                    break;
                }
                await Awaitable.NextFrameAsync();
            }
        }
    }
    void SetExplicitNavigation()
    {
        for (int i = 0; i < bannerContainer.childCount; i++)
        {
            var banner = bannerContainer.GetChild(i).GetComponent<Button>();
            if (i == 0)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = bannerContainer.GetChild(i + 1).GetComponent<Button>(),
                    selectOnDown = bannerContainer.GetChild(bannerContainer.childCount - 1).GetComponent<Button>()
                };
                banner.navigation = nav;
            }
            else if (i == bannerContainer.childCount - 1)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = bannerContainer.GetChild(0).GetComponent<Button>(),
                    selectOnDown = bannerContainer.GetChild(i - 1).GetComponent<Button>()
                };
                banner.navigation = nav;
            }
            else
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = bannerContainer.GetChild(i + 1).GetComponent<Button>(),
                    selectOnDown = bannerContainer.GetChild(i - 1).GetComponent<Button>()
                };
                banner.navigation = nav;
            }
        }
    }
    void AddNextDialogIndicator()
    {
        var space = Instantiate(charPrefab, charsContainer).GetComponent<DialogChar>();
        space.charText.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
        space.charText.alignment = TextAlignmentOptions.Midline;
        space.typeAnimates = new List<DialogBaseSO.TypeAnimate> { DialogBaseSO.TypeAnimate.Bounce };
        space.SetText("^");
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
    public async Awaitable OnHandleSelectBanner(DialogBanner banner)
    {
        nextDialog = false;
        isResettingDialog = true;
        bannerAnimator.Play("Exit");
        await Awaitable.NextFrameAsync();
        while (true)
        {
            if (bannerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }
            await Awaitable.NextFrameAsync();
        }
        dialogBaseSO = banner.newDialog;
        isResettingDialog = false;
        _ = ShowText();
    }
    public async Awaitable ResetTextByLoop(DialogBaseSO newDialog)
    {
        nextDialog = false;
        isResettingDialog = true;
        dialogBaseSO = newDialog;
        isResettingDialog = false;
        _ = ShowText();
    }
    public async Awaitable CloseDialog()
    {
        nextDialog = false;
        dialogBaseSO = null;
        menuAnimator.Play("Exit");
        await Awaitable.NextFrameAsync();
        await GameManager.Instance.UnloadAdditiveScene(GameManager.TypeScene.DialogScene, GameManager.TypeLoader.None, this, null);
    }
    public bool CanShowDialogs()
    {
        return dialogBaseSO != null && menuAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    }
    public void PlayEndAnimation() { }

    public bool AnimationEnded()
    {
        return menuAnimator.GetCurrentAnimatorStateInfo(0).IsName("Exit") && menuAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
    [Serializable]
    public class CharacterData
    {
        public CharacterBase character;
        public bool isCharacterPlayer;
        public GameObject characterImage;
    }
}
