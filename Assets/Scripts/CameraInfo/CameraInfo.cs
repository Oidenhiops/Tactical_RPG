using UnityEngine;

public class CameraInfo : MonoBehaviour
{
    public static CameraInfo Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void CamDirection(out Vector3 camForward, out Vector3 camRight)
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Quaternion rotationOffset = Quaternion.Euler(0, -45f, 0);
        camForward = rotationOffset * forward;
        camRight = rotationOffset * right;
    }
}
