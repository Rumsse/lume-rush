using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject emptyPumpIcon;
    [SerializeField] GameObject fullPumpIcon;

    void OnEnable()
    {
        UpdatePumpIcons(PlayerInteraction.CurrentPumpState);
        PlayerInteraction.PumpStateChanged += OnPumpStateChanged;
    }

    void OnDisable() => PlayerInteraction.PumpStateChanged -= OnPumpStateChanged;

    void OnPumpStateChanged(PumpState state) => UpdatePumpIcons(state);

    void UpdatePumpIcons(PumpState state)
    {
        if (emptyPumpIcon) emptyPumpIcon.SetActive(state == PumpState.Empty);
        if (fullPumpIcon) fullPumpIcon.SetActive(state == PumpState.Full);
    }
}