using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomSlotData
{
    public string slotID;
    public RoomType roomType;
    public int level;
}

[Serializable]
public class ShipSaveData
{
    public List<RoomSlotData> rooms = new List<RoomSlotData>();
    public List<string> boughtWorkshopItems = new List<string>();
}

public static class ShipSaveManager
{
    private const string SaveKey = "ShipSaveData";

    public static void SaveRoom(string slotID, RoomType type, int level)
    {
        ShipSaveData data = LoadData();
        RoomSlotData room = data.rooms.Find(r => r.slotID == slotID);

        if (room != null)
        {
            room.roomType = type;
            room.level = level;
        }
        else
            data.rooms.Add(new RoomSlotData { slotID = slotID, roomType = type, level = level });

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static void SaveWorkshopItem(string itemID)
    {
        ShipSaveData data = LoadData();

        if (!data.boughtWorkshopItems.Contains(itemID))
        {
            data.boughtWorkshopItems.Add(itemID);

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }

    public static bool IsWorkshopItemBought(string itemID)
    {
        ShipSaveData data = LoadData();
        return data.boughtWorkshopItems.Contains(itemID);
    }

    public static ShipSaveData LoadData()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return new ShipSaveData();

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<ShipSaveData>(json);
    }

    public static void ResetShipData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}