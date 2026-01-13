using UnityEngine;

public class LongerInteractable : Interactable
{
    [SerializeField] float collectTime = 1.5f;

    public float CollectTime => collectTime;
}