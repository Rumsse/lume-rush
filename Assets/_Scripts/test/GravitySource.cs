using System.Collections.Generic;
using UnityEngine;

public class GravitySource : MonoBehaviour
{
    #region Static registry

    static readonly List<GravitySource> registry = new List<GravitySource>();

    public static IReadOnlyList<GravitySource> All => registry;

    void OnEnable() => registry.Add(this);
    void OnDisable() => registry.Remove(this);

    #endregion

    #region Inspector

    public float gravity = 9.81f;

    #endregion

    public Vector3 Center => transform.position;
    public float Gravity => gravity;
}