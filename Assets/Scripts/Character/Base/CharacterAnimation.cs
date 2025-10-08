using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public Character character;
    public InitialDataSO.AnimationsInfo currentAnimation = new InitialDataSO.AnimationsInfo();
    public int currentSpriteIndex;
    public float currentSpritePerTime = 0.1f;
    public string animationAfterEnd;
    bool isUp = false;

    public void SetInitialData(ref InitialDataSO animationsData)
    {
        StopAllCoroutines();
        character.characterModel.characterMeshRenderer.transform.parent.transform.localScale = Vector3.one * GetScaleFactor(animationsData.animations["Idle"].spritesInfoDown[0].characterSprite.rect.width);
        character.characterModel.characterMeshRenderer.material.SetTexture("_BaseTexture", animationsData.atlas);
        if (animationsData.atlasHands)
        {
            character.characterModel.characterMeshRendererHand.gameObject.SetActive(true);
            character.characterModel.characterMeshRendererHand.material.SetTexture("_BaseTexture", animationsData.atlasHands);
        }
        currentAnimation = GetAnimation("Idle");
        StartCoroutine(AnimateSprite());
    }

    float GetScaleFactor(float size)
    {
        float baseScale = 64f;
        return size / baseScale;
    }
    public void MakeAnimation(string animationName)
    {
        StopAllCoroutines();
        currentAnimation = GetAnimation(animationName);
        currentSpriteIndex = 0;
        StartCoroutine(AnimateSprite());
    }
    public string GetAnimationAttack()
    {
        character.characterData.GetCurrentWeapon(out CharacterData.CharacterItem weapon);
        if (weapon != null)
        {
            return weapon.itemBaseSO.animationName;
        }
        return "FistAttack";
    }
    private InitialDataSO.AnimationsInfo GetAnimation(string animationName)
    {
        return character.initialDataSO.animations[animationName];
    }

    IEnumerator AnimateSprite()
    {
        while (true)
        {
            isUp = character.direction.z > 0;
            SetTextureFromAtlas(
                isUp ?
                    currentAnimation.spritesInfoUp[currentSpriteIndex].characterSprite :
                    currentAnimation.spritesInfoDown[currentSpriteIndex].characterSprite,
                character.characterModel.characterMeshRenderer
            );
            if (character.initialDataSO.atlasHands)
            {
                SetTextureFromAtlas(
                    isUp ?
                        currentAnimation.spritesInfoUp[currentSpriteIndex].characterSprite :
                        currentAnimation.spritesInfoDown[currentSpriteIndex].characterSprite,
                    character.characterModel.characterMeshRendererHand
                );
                SetHandsPos();
            }
            yield return new WaitForSeconds(currentSpritePerTime);
            currentSpriteIndex++;
            if (currentSpriteIndex > currentAnimation.spritesInfoUp.Length - 1)
            {
                if (currentAnimation.loop)
                {
                    currentSpriteIndex = 0;
                }
                else
                {
                    if (currentAnimation.linkAnimation != "")
                    {
                        MakeAnimation(currentAnimation.linkAnimation);
                    }
                    else
                    {
                        if (animationAfterEnd != "")
                        {
                            MakeAnimation(animationAfterEnd);
                            animationAfterEnd = "";
                        }
                        else
                        {
                            MakeAnimation("Idle");
                        }
                    }
                }
            }
        }
    }
    void SetTextureFromAtlas(Sprite spriteFromAtlas, MeshRenderer meshRenderer)
    {
        Vector2[] uvs = character.characterModel.originalMesh.uv;
        Texture2D texture = spriteFromAtlas.texture;
        meshRenderer.material.mainTexture = texture;
        Rect spriteRect = spriteFromAtlas.rect;
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i].x = Mathf.Lerp(spriteRect.x / texture.width, (spriteRect.x + spriteRect.width) / texture.width, uvs[i].x);
            uvs[i].y = Mathf.Lerp(spriteRect.y / texture.height, (spriteRect.y + spriteRect.height) / texture.height, uvs[i].y);
        }
        meshRenderer.GetComponent<MeshFilter>().mesh.uv = uvs;
    }
    void SetHandsPos()
    {
        if (currentSpriteIndex < currentAnimation.spritesInfoUp.Length && currentAnimation.spritesInfoUp.Length > 0)
        {
            switch (isUp)
            {
                case true:
                    Vector3 spriteLeftUpPos = character.direction.x > 0 ?
                                                currentAnimation.spritesInfoUp[currentSpriteIndex].leftHandPosDR :
                                                currentAnimation.spritesInfoUp[currentSpriteIndex].leftHandPosDL;
                    Vector3 spriteRightUpPos = character.direction.x > 0 ?
                                                currentAnimation.spritesInfoUp[currentSpriteIndex].rightHandPosDR :
                                                currentAnimation.spritesInfoUp[currentSpriteIndex].rightHandPosDL;
                    character.characterModel.leftHand.transform.localPosition = spriteLeftUpPos;
                    character.characterModel.leftHand.transform.localRotation = currentAnimation.spritesInfoUp[currentSpriteIndex].leftHandRotation;
                    character.characterModel.rightHand.transform.localPosition = spriteRightUpPos;
                    character.characterModel.rightHand.transform.localRotation = currentAnimation.spritesInfoUp[currentSpriteIndex].rightHandRotation;
                    break;
                case false:
                    Vector3 spriteLeftDownPos = character.direction.x > 0 ?
                                                currentAnimation.spritesInfoDown[currentSpriteIndex].leftHandPosDR :
                                                currentAnimation.spritesInfoDown[currentSpriteIndex].leftHandPosDL;
                    Vector3 spriteRightDownPos = character.direction.x > 0 ?
                                                currentAnimation.spritesInfoDown[currentSpriteIndex].rightHandPosDR :
                                                currentAnimation.spritesInfoDown[currentSpriteIndex].rightHandPosDL;
                    character.characterModel.leftHand.transform.localPosition = spriteLeftDownPos;
                    character.characterModel.leftHand.transform.localRotation = currentAnimation.spritesInfoDown[currentSpriteIndex].leftHandRotation;
                    character.characterModel.rightHand.transform.localPosition = spriteRightDownPos;
                    character.characterModel.rightHand.transform.localRotation = currentAnimation.spritesInfoDown[currentSpriteIndex].rightHandRotation;
                    break;
            }
        }
    }

    public void MakeEffect(TypeAnimationsEffects typeEffect)
    {
        switch (typeEffect)
        {
            case TypeAnimationsEffects.Blink:
                _ = Blink();
                break;
            case TypeAnimationsEffects.Shake:
                _ = Shake();
                break;
        }
    }
    #region AnimationsEffects
    async Awaitable Shake()
    {
        float tiempoTranscurrido = 0f;
        Vector3 initialPos = character.characterModel.characterMeshRenderer.transform.localPosition;

        while (tiempoTranscurrido < currentSpritePerTime * currentAnimation.spritesInfoUp.Length)
        {
            float desplazamientoX = Mathf.Sin(Time.time * currentAnimation.animationsEffects[TypeAnimationsEffects.Shake].frequency) * currentAnimation.animationsEffects[TypeAnimationsEffects.Shake].amplitude;
            character.characterModel.characterMeshRenderer.transform.localPosition = initialPos + new Vector3(desplazamientoX, 0, 0);
            tiempoTranscurrido += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
        initialPos.x = 0f;
        character.characterModel.characterMeshRenderer.transform.localPosition = initialPos;
    }
    async Awaitable Blink()
    {
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < currentSpritePerTime * currentAnimation.spritesInfoUp.Length)
        {
            if (character.characterModel.characterMeshRenderer.material.color == Color.white)
            {
                character.characterModel.characterMeshRenderer.material.SetColor("_Color", currentAnimation.animationsEffects[TypeAnimationsEffects.Blink].colorBlink);
            }
            else
            {
                character.characterModel.characterMeshRenderer.material.SetColor("_Color", Color.white);
            }
            tiempoTranscurrido += currentSpritePerTime;
            await Task.Delay(TimeSpan.FromSeconds(currentSpritePerTime));
        }
        character.characterModel.characterMeshRenderer.material.SetColor("_Color", Color.white);
    }
    #endregion
    [Serializable] public class AnimationEffectInfo
    {
        public float amplitude = 0;
        public float frequency = 0;
        public Color colorBlink = Color.white;
    }
    public enum TypeAnimationsEffects
    {
        None = 0,
        Shake = 1,
        Blink = 2
    }
}
