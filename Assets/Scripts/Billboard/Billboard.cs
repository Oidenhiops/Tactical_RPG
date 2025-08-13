using UnityEngine;
public class Billboard : MonoBehaviour
{
    public Vector3 offsetRotation;
    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(offsetRotation);
        }
    }
}