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
    public void CamDirection(Vector3 direction, out Vector3 directionFromCamera)
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Quaternion rotationOffset = Quaternion.Euler(0, -45f, 0);
        Vector3 camForward = rotationOffset * forward;
        Vector3 camRight = rotationOffset * right;

        Vector3 camRelativeDir = (direction.x * camRight + direction.z * camForward).normalized;
        directionFromCamera = new Vector3Int(Mathf.RoundToInt(camRelativeDir.x), 0, Mathf.RoundToInt(camRelativeDir.z));
    }
}
