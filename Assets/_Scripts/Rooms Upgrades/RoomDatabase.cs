using System.Collections.Generic;
using UnityEngine;

public class RoomDatabase : MonoBehaviour
{
    #region Properties

    public static RoomDatabase Instance { get; private set; }

    #endregion

    #region Variables

    [SerializeField] private List<RoomDefinition> allRooms = new List<RoomDefinition>();

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    #region Database Logic

    public RoomDefinition GetDefinition(RoomType type) => allRooms.Find(r => r.roomType == type);

    #endregion
}