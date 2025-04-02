using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float offsetZ = -10;
    public float smoothSpeed = 0.125f;

    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + (transform.forward * offsetZ);

        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothPosition;
    }
}
