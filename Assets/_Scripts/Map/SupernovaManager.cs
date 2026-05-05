using UnityEngine;

public class SupernovaManager : MonoBehaviour
{
    #region Settings

    [SerializeField] float expansionSpeed = 2f;

    #endregion

    #region State

    public static SupernovaManager Instance { get; private set; }
    public float CurrentRadius { get; private set; }

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadRadius();
    }

    void Update() => CurrentRadius += expansionSpeed * Time.deltaTime;

    void OnDisable() => SaveRadius();

    void OnApplicationQuit() => SaveRadius();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, CurrentRadius);
    }

    #endregion

    #region Logic

    public void SaveRadius()
    {
        PlayerPrefs.SetFloat("SupernovaRadius", CurrentRadius);
        PlayerPrefs.Save();
    }

    void LoadRadius()
    {
        if (!PlayerPrefs.HasKey("SupernovaRadius"))
        {
            CurrentRadius = 0f;
            return;
        }

        CurrentRadius = PlayerPrefs.GetFloat("SupernovaRadius");
    }

    public static void ResetRadius()
    {
        if (Instance)
            Instance.CurrentRadius = 0f;

        PlayerPrefs.DeleteKey("SupernovaRadius");
    }

    #endregion
}