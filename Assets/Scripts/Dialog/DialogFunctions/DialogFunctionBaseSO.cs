using UnityEngine;
public class DialogFunctionBaseSO : ScriptableObject
{
    public bool closeDialog;
    public virtual async Awaitable MakeBannerFunction(CharacterBase characterWorld) { }
}
