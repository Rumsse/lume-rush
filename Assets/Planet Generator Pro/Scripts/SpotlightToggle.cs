using UnityEngine;

namespace PlanetaryRendering.Utility
{
    /// <summary>
    /// Simple spotlight toggle on left mouse button.
    /// Attach this script to any GameObject and assign a Light (Spot recommended).
    /// </summary>
    [DisallowMultipleComponent]
    public class SpotlightToggleSimple : MonoBehaviour
    {
        [Tooltip("Light to toggle (Spotlight recommended).")]
        public Light spotlight;

        private void Reset()
        {
            // Auto-assign Light component if attached to the same GameObject
            if (spotlight == null)
                spotlight = GetComponent<Light>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                if (spotlight != null)
                    spotlight.enabled = !spotlight.enabled;
            }
        }
    }
}
