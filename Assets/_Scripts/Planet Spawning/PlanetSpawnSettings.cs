using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(menuName = "Planet/SpawnSettings", fileName = "PlanetSpawnSettings")]
public class PlanetSpawnSettings : ScriptableObject
{
    public GameObject planetPrefab;
    public bool spawnOnStart = true;
    public Vector3 spawnPosition = Vector3.zero;
    public Vector3 spawnEuler = Vector3.zero;
    public Transform parent;
    public int seedMin = 0;
    public int seedMax = 1000000;
}