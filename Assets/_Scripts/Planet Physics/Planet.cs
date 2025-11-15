using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Planet : MonoBehaviour
{
    #region Inspector
    [SerializeField] float triggerMargin = 0.02f;
    [SerializeField] float gravityRadiusMultiplier = 1.6f;
    #endregion

    #region Static
    static readonly List<Planet> all = new List<Planet>();
    public static IReadOnlyList<Planet> All => all;
    #endregion

    #region API
    public Vector3 Center => transform.position;
    public float SurfaceRadius { get; private set; }
    public float GravityRadius => SurfaceRadius * gravityRadiusMultiplier;
    #endregion

    SphereCollider surfaceCollider;
    SphereCollider gravityTrigger;

    void Awake()
    {
        surfaceCollider = GetComponent<SphereCollider>();
        var scale = transform.lossyScale;
        var maxScale = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        SurfaceRadius = surfaceCollider.radius * maxScale;
        gravityTrigger = null;
        foreach (var c in GetComponents<SphereCollider>()) if (c.isTrigger) { gravityTrigger = c; break; }
        if (gravityTrigger == null)
        {
            gravityTrigger = gameObject.AddComponent<SphereCollider>();
            gravityTrigger.isTrigger = true;
        }
        gravityTrigger.center = surfaceCollider.center;
        gravityTrigger.radius = surfaceCollider.radius * gravityRadiusMultiplier;
        if (!all.Contains(this)) all.Add(this);
    }

    void OnDestroy() { if (all.Contains(this)) all.Remove(this); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gravityRadiusMultiplier);
    }
}