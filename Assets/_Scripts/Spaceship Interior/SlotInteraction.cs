using UnityEngine;
using UnityEngine.InputSystem;

public class SlotInteraction : MonoBehaviour
{
    public Ray ray;
    public Camera mainCam;

    private GameObject cachedBuildSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void CastARay()
    {
        ray = new Ray(mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue()), transform.forward);
        CheckForColliders();
    }

    void CheckForColliders()
    {
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.tag == "BuildSlot")
        {
            cachedBuildSlot = hit.collider.gameObject;
            cachedBuildSlot.GetComponent<SlotController>()._selected = true;
            cachedBuildSlot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
        else if (cachedBuildSlot != null)
        {
            cachedBuildSlot.GetComponent<SlotController>()._selected = false;
            cachedBuildSlot.transform.localScale = new Vector3(1f, 1f, 1f);
            cachedBuildSlot = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CastARay();
    }
}

