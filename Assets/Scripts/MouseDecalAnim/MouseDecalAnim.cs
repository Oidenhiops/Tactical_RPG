using System;
using Unity.Cinemachine;
using UnityEngine;

public class MouseDecalAnim : MonoBehaviour
{
    public MeshRenderer decal;
    public float speed = 1.5f;
    public float minAlpha = 0f;
    public float maxAlpha = 1f;
    public Transform subGridContainer;
    private Material decalMaterial;
    public CinemachineBrain cinemachineBrain;
    public Transform enemyCameraParent;
    public Transform enemyCameraTransform;
    public CinemachineCamera enemyCamera;
    bool activeBlendFinded;
    bool initializeBlendEvent;
    bool characterFinded;
    void Start()
    {
        decalMaterial = decal.material;
    }
    void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * speed, 1f));

        Color color = decalMaterial.GetColor("_BaseColor");
        color.a = alpha;
        decalMaterial.SetColor("_BaseColor", color);
    }
    public void SendSubCamera(GameObject target)
    {
        enemyCameraTransform.parent = target.transform;
        enemyCameraTransform.localPosition = Vector3.zero;
        enemyCamera.Priority = 10;
    }
    public async Awaitable ReturnSubCamera()
    {
        try
        {
            if (!initializeBlendEvent)
            {
                initializeBlendEvent = true;
            }
            else
            {
                characterFinded = false;
                foreach (var character in BattlePlayerManager.Instance.characters)
                {
                    if (character.gameObject.activeSelf)
                    {
                        characterFinded = true;
                        transform.position = character.transform.position;
                        await Awaitable.NextFrameAsync();
                        break;
                    }
                }
                if (!characterFinded)
                {
                    transform.position = Vector3.zero;
                }
                enemyCamera.Priority = 0;
                while (true)
                {
                    if (cinemachineBrain.ActiveBlend == null && activeBlendFinded)
                    {
                        break;
                    }
                    else if (cinemachineBrain.ActiveBlend != null)
                    {
                        activeBlendFinded = true;
                    }
                    await Awaitable.NextFrameAsync();
                }
                activeBlendFinded = false;
                enemyCameraTransform.parent = enemyCameraParent;
                enemyCameraTransform.localPosition = Vector3.zero;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void Test()
    {
        
    }
}
