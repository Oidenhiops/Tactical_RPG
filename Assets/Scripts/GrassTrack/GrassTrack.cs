using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class GrassTrack : MonoBehaviour
{
    public Vector3 trackerPos;
    Material grassMat;
    [SerializeField] Renderer grassRenderer;
    float time = 0;
    public float speed = 0.1f;
    public float value;
    public int length;
    void Start()
    {
        grassMat = grassRenderer.material;
    }
    void FixedUpdate()
    {
        time += Time.deltaTime * speed;
        trackerPos = Vector3.Lerp(grassMat.GetVector("_TrakerPosition"), Vector3.zero, time * Time.deltaTime);
        value = Mathf.PingPong(time, length);
        trackerPos = Vector3.one * value;
        trackerPos.y = 0;
        grassMat.SetVector("_TrakerPosition", trackerPos);
    }
}
