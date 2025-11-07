using UnityEngine;

namespace PlanetaryRendering.Celestial
{
    /// <summary>
    /// Rotates an object around a planet on a stable orbital path.
    /// Position is calculated analytically: P = planetPos + R_y(angle) * (forward * orbitRadius).
    /// </summary>
    [DisallowMultipleComponent]
    public class SunOrbit : MonoBehaviour
    {
        [Tooltip("Center of the orbit (planet core).")]
        public Transform planet;

        [Tooltip("Orbit radius in world units.")]
        public float orbitRadius = 20000f;

        [Tooltip("Orbital speed in degrees per second (positive = clockwise when viewed from above).")]
        public float orbitSpeedDeg = 5f;

        [Tooltip("Initial angle in degrees (0 = world forward from planet).")]
        public float initialAngleDeg = 0f;

        [Tooltip("Inclination angle relative to world Y in degrees. 0 = flat orbit, positive = tilted forward.")]
        public float inclinationDeg = 0f;

        [Tooltip("Whether the object should look at the planet.")]
        public bool lookAtPlanet = true;

        private float angleDeg;

        private void Start()
        {
            angleDeg = initialAngleDeg;

            if (planet == null)
            {
                var found = GameObject.FindWithTag("Planet");
                if (found != null) planet = found.transform;
            }

            UpdatePosition(0f);
        }

        private void Update()
        {
            if (planet == null) return;

            angleDeg += orbitSpeedDeg * Time.deltaTime;

            if (angleDeg >= 360f) angleDeg -= 360f;
            if (angleDeg < 0f) angleDeg += 360f;

            UpdatePosition(angleDeg);
        }

        private void UpdatePosition(float angle)
        {
            Vector3 baseVec = Vector3.forward * orbitRadius;

            Quaternion rotY = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 posOnCircle = rotY * baseVec;

            Quaternion incl = Quaternion.AngleAxis(inclinationDeg, Vector3.right);
            posOnCircle = incl * posOnCircle;

            Vector3 worldPos = planet.position + posOnCircle;
            transform.position = worldPos;

            if (lookAtPlanet)
                transform.LookAt(planet.position, Vector3.up);
        }

        private void OnDrawGizmosSelected()
        {
            if (planet == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(planet.position, orbitRadius);
            Gizmos.DrawLine(planet.position, transform.position);
        }
    }
}
