using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out IHealth health))
            return;

        health.TakeDamage(damageAmount);
    }
}