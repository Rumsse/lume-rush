using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    #region Serialized Fields

    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [SerializeField] private float distance = 8f;
    [SerializeField] private float heightFar = 2f;
    [SerializeField] private float heightClose = 1.2f;

    [Header("Camera Angle")]
    [SerializeField] private float angleDownClose = 20f;
    [SerializeField] private float angleDownFar = 0f;

    [Header("Camera Side")]
    [SerializeField] private bool rightSide = true;

    [Header("Zoom")]
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 12f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float zoomSmoothing = 5f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 8f;
    [SerializeField] private float angleSmoothing = 5f;
    [SerializeField] private float heightSmoothing = 5f;

    #endregion

    #region Private Fields

    private float currentDistance;
    private float targetDistance;
    private float currentAngle;
    private float currentHeight;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentDistance = distance;
        targetDistance = distance;
        currentAngle = CalculateAngleForDistance(distance);
        currentHeight = CalculateHeightForDistance(distance);
    }

    private void Update()
    {
        HandleZoomInput();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateCameraPosition();
    }

    #endregion

    #region Input

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
            targetDistance -= scroll * zoomSpeed;

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmoothing);
    }

    #endregion

    #region Camera Logic

    private void UpdateCameraPosition()
    {
        float sideMultiplier = rightSide ? -1f : 1f;

        float targetHeight = CalculateHeightForDistance(currentDistance);
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * heightSmoothing);

        Vector3 targetPosition = new Vector3(
            target.position.x + (currentDistance * sideMultiplier),
            target.position.y + currentHeight,
            target.position.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmoothing);

        float targetAngle = CalculateAngleForDistance(currentDistance);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * angleSmoothing);

        float rotationY = rightSide ? 90f : -90f;
        transform.rotation = Quaternion.Euler(currentAngle, rotationY, 0);
    }

    private float CalculateAngleForDistance(float dist)
    {
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);
        return Mathf.Lerp(angleDownClose, angleDownFar, t);
    }

    private float CalculateHeightForDistance(float dist)
    {
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);
        return Mathf.Lerp(heightClose, heightFar, t);
    }

    #endregion
}