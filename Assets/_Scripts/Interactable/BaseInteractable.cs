using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour
{
    protected abstract void OnMouseEnter();
    protected abstract void OnMouseExit();
    protected abstract void OnMouseDown();
}