using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    #region Serialized Fields

    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float height = 1.8f;
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.6f, 0, 0);

    [Header("Input")]
    [SerializeField] private string mouseXAxis = "Mouse X";
    [SerializeField] private string mouseYAxis = "Mouse Y";
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private bool invertY = false;

    [Header("Rotation")]
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 70f;

    [Header("Smoothing")]
    [SerializeField] private float positionDamping = 5f;
    [SerializeField] private float rotationDamping = 8f;

    [Header("Collision")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float collisionPadding = 0.2f;
    [SerializeField] private LayerMask collisionMask = -1;

    #endregion

    #region Private Fields

    private float rotationX;
    private float rotationY;
    private float currentDistance;
    private Vector3 currentPosition;

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
        currentPosition = transform.position;

        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleInput();

        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleCursor();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdatePosition();
    }

    #endregion

    #region Input

    private void HandleInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis(mouseXAxis);
        float mouseY = Input.GetAxis(mouseYAxis);

        rotationX += mouseX * mouseSensitivity;

        float yInput = invertY ? mouseY : -mouseY;
        rotationY += yInput * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
    }

    private void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    #endregion

    #region Camera Logic

    private void UpdatePosition()
    {
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 rotatedOffset = rotation * shoulderOffset;
        targetPosition += rotatedOffset;

        Vector3 direction = rotation * -Vector3.forward;
        float desiredDistance = CheckCollision(targetPosition, direction);

        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * positionDamping);

        Vector3 finalPosition = targetPosition + direction * currentDistance;
        currentPosition = Vector3.Lerp(currentPosition, finalPosition, Time.deltaTime * positionDamping);

        transform.position = currentPosition;
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
    }

    private float CheckCollision(Vector3 targetPos, Vector3 direction)
    {
        if (Physics.Raycast(targetPos, direction, out RaycastHit hit, distance, collisionMask))
            return Mathf.Max(hit.distance - collisionPadding, minDistance);

        return distance;
    }

    #endregion
}