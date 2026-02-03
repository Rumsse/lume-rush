using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour
{
    public abstract void OnInteract();
    public abstract void OnHoverStart();
    public abstract void OnHoverEnd();
}