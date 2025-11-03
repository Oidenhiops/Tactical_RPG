using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public List<DialogText> texts;
    public GameObject charPrefab;
    public Transform charsContainer;
    [NaughtyAttributes.Button]
    public void TestText()
    {
        _ = ShowText();
    }
    public async Awaitable ShowText()
    {
        foreach (Transform child in charsContainer)
        {
            Destroy(child.gameObject);
        }
        await Awaitable.NextFrameAsync();
        foreach (var dialogText in texts)
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
                        await Awaitable.WaitForSecondsAsync(dialogText.timeBetweenChars);
                    }
                }
                var space = Instantiate(charPrefab, charsContainer).GetComponent<DialogChar>();
                space.SetText(" ");
            }
        }
    }
    bool WordNeedJumpLine(string word, out int amountChars)
    {
        int lineNumber = charsContainer.childCount / 48;
        if (charsContainer.childCount - (lineNumber * 48) + word.Length > 48) // Assuming 40 characters fit in one line
        {
            amountChars = Math.Abs(charsContainer.childCount - (lineNumber * 48) - 48);
            return true;
        }
        amountChars = 0;
        return false;
    }
    [Serializable]
    public class DialogText
    {
        public List<TypeAnimate> typeAnimates;
        public string textId;
        public float timeBetweenChars = 0.05f;
    }
    public enum TypeAnimate
    {
        None,
        Bounce,
        Dangle,
        Rainb,
        Rot,
        Shake,
        Incr,
        Slide,
        Swing,
        Wave,
        Wiggle
    }
}
