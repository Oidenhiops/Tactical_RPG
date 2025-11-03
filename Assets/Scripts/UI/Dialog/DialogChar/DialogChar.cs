using System.Collections.Generic;
using Febucci.UI;
using TMPro;
using UnityEngine;

public class DialogChar : MonoBehaviour
{
    public TypewriterByWord typewriter;
    public TextAnimator_TMP textAnimator;
    public TMP_Text charText;
    public List<DialogManager.TypeAnimate> typeAnimates;
    public string testText;
    [NaughtyAttributes.Button]
    public void Test()
    {
        SetText(testText);
    }
    public void SetText(string text)
    {
        typewriter.StopShowingText();
        charText.text = "";
        string opens = "";
        string ends = "";
        foreach (var typeAnimate in typeAnimates)
        {
            opens += $"<{typeAnimate.ToString().ToLower()}>";
            ends += $"</{typeAnimate.ToString().ToLower()}>";
        }
        charText.text += opens + text + ends;
        typewriter.StartShowingText(true);
    }
}
