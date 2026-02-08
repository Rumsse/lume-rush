using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private bool invertY = false;

    [Header("Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Distance Limits")]
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float scrollSpeed = 2f;

    private float _currentX;
    private float _currentY;

    private void Start()
    {
        if (!target)
        {
            target = new GameObject("Camera Target").transform;
            target.position = Vector3.zero;
        }

        Vector3 angles = transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;

        UpdateCameraPosition();
    }

    private void LateUpdate()
    {
        if (!target)
            return;

        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    private void HandleRotation()
    {
        if (!Input.GetMouseButton(1))
            return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        _currentX += mouseX;
        _currentY -= invertY ? -mouseY : mouseY;
        _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f))
            return;

        distance -= scroll * scrollSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        Vector3 direction = rotation * Vector3.back;
        transform.position = target.position + direction * distance;
        transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;

    public void SetDistance(float newDistance) => distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
}