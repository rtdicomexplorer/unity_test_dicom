

using UnityEngine;

public class CameraControllerTriplanes : MonoBehaviour
{
    public Transform target; // center of rotation (TriPlanarView)
    public float distance = 10f;

    public float zoomSpeed = 5f;
    public float rotateSpeed = 5f;
    public float panSpeed = 0.5f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Start()
    {
        if (!target)
        {
            Debug.LogError("CameraController: target not assigned!");
            return;
        }

        // Initial camera position
        transform.position = target.position - transform.forward * distance;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Rotate with left mouse
        if (Input.GetMouseButton(0)) // 0 = left click
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }

        // Zoom with scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, 2f, 50f);

        // Pan with middle mouse
        if (Input.GetMouseButton(2)) // 2 = middle click
        {
            Vector3 right = transform.right * -Input.GetAxis("Mouse X") * panSpeed;
            Vector3 up = transform.up * -Input.GetAxis("Mouse Y") * panSpeed;
            target.position += right + up;
        }

        // Update camera position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.position = target.position - rotation * Vector3.forward * distance;
        transform.LookAt(target.position);
    }

}

