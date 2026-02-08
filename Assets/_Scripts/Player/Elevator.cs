using UnityEngine;

public class Elevator : MonoBehaviour
{
    #region Serialized Fields

    [Header("Elevator Settings")]
    [SerializeField] private Transform[] floors;
    [SerializeField] private string playerTag = "Player";

    [Header("UI (Optional)")]
    [SerializeField] private GameObject elevatorUI;

    [Header("Input")]
    [SerializeField] private KeyCode floor0Key = KeyCode.Alpha0;
    [SerializeField] private KeyCode floor1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode floor2Key = KeyCode.Alpha2;

    #endregion

    #region Private Fields

    private Transform playerTransform;
    private bool playerInElevator;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (elevatorUI != null)
            elevatorUI.SetActive(false);

        if (floors.Length < 3)
            Debug.LogWarning("Elevator needs at least 3 floor positions!");
    }

    private void Update()
    {
        if (!playerInElevator || playerTransform == null)
            return;

        HandleFloorInput();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerTransform = other.transform;
            playerInElevator = true;

            if (elevatorUI != null)
                elevatorUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInElevator = false;
            playerTransform = null;

            if (elevatorUI != null)
                elevatorUI.SetActive(false);
        }
    }

    #endregion

    #region Floor Selection

    private void HandleFloorInput()
    {
        if (Input.GetKeyDown(floor0Key))
            TeleportToFloor(0);
        else if (Input.GetKeyDown(floor1Key))
            TeleportToFloor(1);
        else if (Input.GetKeyDown(floor2Key))
            TeleportToFloor(2);
    }

    private void TeleportToFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floors.Length)
        {
            Debug.LogWarning($"Floor {floorIndex} doesn't exist!");
            return;
        }

        if (floors[floorIndex] == null)
        {
            Debug.LogWarning($"Floor {floorIndex} position is not assigned!");
            return;
        }

        if (playerTransform != null)
        {
            CharacterController controller = playerTransform.GetComponent<CharacterController>();
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();

            if (controller != null)
            {
                controller.enabled = false;
                playerTransform.position = floors[floorIndex].position;
                controller.enabled = true;
            }
            else if (rb != null)
            {
                rb.position = floors[floorIndex].position;
                rb.linearVelocity = Vector3.zero;
            }
            else
            {
                playerTransform.position = floors[floorIndex].position;
            }

            Debug.Log($"Teleported to floor {floorIndex}");
        }
    }

    #endregion
}