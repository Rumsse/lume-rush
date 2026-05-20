using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ResourceCost
{
    public string resourceName;
    public int amount;
}

[CreateAssetMenu(fileName = "RoomLevelData", menuName = "Ship/Room Level Data")]
public class RoomLevelData : ScriptableObject
{
    public int level;
    public List<ResourceCost> costs = new List<ResourceCost>();
    public GameObject roomPrefab;
    public Sprite previewImage;
    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
}