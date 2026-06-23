using System;
using UnityEngine;

public class WorkshopPlayerDetection : MonoBehaviour
{
    public static event Action<bool> OnWorkshopRangeChanged;

    [SerializeField] string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        Debug.Log("Player entered the workshop trigger");
        OnWorkshopRangeChanged?.Invoke(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        Debug.Log("Player left the workshop trigger");
        OnWorkshopRangeChanged?.Invoke(false);
    }
}