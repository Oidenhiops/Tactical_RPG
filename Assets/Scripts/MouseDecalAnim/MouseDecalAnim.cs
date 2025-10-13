using UnityEngine;

public class MouseDecalAnim : MonoBehaviour
{
    public MeshRenderer decal;
    public float speed = 1.5f;
    public float minAlpha = 0f;
    public float maxAlpha = 1f;
    public Transform subGridContainer;
    private Material decalMaterial;
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
}
