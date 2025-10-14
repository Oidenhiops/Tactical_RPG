using Febucci.UI;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public TMP_Text label;
    public TypewriterByCharacter typewriterByCharacter;
    public TextAnimator_TMP textAnimator_TMP;
    void Awake()
    {
        Material clonedMaterial = Instantiate(label.fontMaterial);
        label.fontMaterial = clonedMaterial;
    }
    public async Task SendText(string value, Color color, bool isCritic)
    {
        await Awaitable.WaitForSecondsAsync(0.1f);
        label.text = value;
        label.color = color;
        if (isCritic)
        {
            label.fontMaterial.SetColor("_FaceColor", color * 2);
            label.fontMaterial.EnableKeyword("_EMISSION");
            label.fontMaterial.SetColor("_EmissionColor", color * 2);
            label.fontStyle = FontStyles.Bold;
        }
        typewriterByCharacter.StartShowingText();
        await Awaitable.WaitForSecondsAsync(1);
        typewriterByCharacter.StartDisappearingText();
        await Awaitable.WaitForSecondsAsync(1);
        Destroy(gameObject);
    }
}