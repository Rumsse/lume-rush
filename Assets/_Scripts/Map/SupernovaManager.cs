using UnityEngine;
using UnityEngine.SceneManagement;

public class SupernovaManager : MonoBehaviour
{
    #region Settings

    [SerializeField] float expansionSpeed = 2f;
    [SerializeField] ParticleSystem waveParticleRoot;
    [SerializeField] string mapSceneName = "Map";

    #endregion

    #region State

    public static SupernovaManager Instance { get; private set; }
    public float CurrentRadius { get; private set; }
    ParticleSystem[] waveParticles;

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

        if (waveParticleRoot)
            waveParticles = waveParticleRoot.GetComponentsInChildren<ParticleSystem>();

        LoadRadius();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            SaveRadius();
    }

    void Update()
    {
        CurrentRadius += expansionSpeed * Time.deltaTime;
        UpdateParticleRadius();
    }

    void OnApplicationQuit()
    {
        if (Instance == this)
            SaveRadius();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, CurrentRadius);
    }

    #endregion

    #region Logic

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ToggleParticlesVisibility(scene.name == mapSceneName);

    void ToggleParticlesVisibility(bool visible)
    {
        if (waveParticles == null)
            return;

        foreach (var ps in waveParticles)
        {
            if (ps == null)
                continue;

            if (ps.TryGetComponent<ParticleSystemRenderer>(out var particleRenderer))
                particleRenderer.enabled = visible;
        }
    }

    void UpdateParticleRadius()
    {
        if (waveParticles == null)
            return;

        foreach (var ps in waveParticles)
        {
            if (ps == null)
                continue;

            var shape = ps.shape;
            shape.radius = CurrentRadius;
        }
    }

    public void SaveRadius()
    {
        PlayerPrefs.SetFloat("SupernovaRadius", CurrentRadius);
        PlayerPrefs.Save();
    }

    void LoadRadius() => CurrentRadius = PlayerPrefs.GetFloat("SupernovaRadius", 0f);

    public static void ResetRadius()
    {
        PlayerPrefs.DeleteKey("SupernovaRadius");

        if (Instance == null)
            return;

        Instance.CurrentRadius = 0f;

        if (Instance.waveParticles != null)
        {
            foreach (var ps in Instance.waveParticles)
            {
                if (ps == null)
                    continue;

                ps.Clear();

                var shape = ps.shape;
                shape.radius = 0f;
            }
        }
    }

    #endregion
}