using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject pumpIcon;

    void OnEnable()
    {
        pumpIcon?.SetActive(PlayerInteraction.HasPumpGlobal);
        PlayerInteraction.PumpStateChanged += OnPumpStateChanged;
    }

    void OnDisable()
    {
        PlayerInteraction.PumpStateChanged -= OnPumpStateChanged;
    }

    void OnPumpStateChanged(bool hasPump) => pumpIcon?.SetActive(hasPump);
}