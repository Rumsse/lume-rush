using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSystem : MonoBehaviour, IHealth
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Sprite fullHeart, emptyHeart;

    private Image[] hearts;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        FindHeartImages();
        UpdateHearts();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            TakeDamage(1);
    }

    private void FindHeartImages()
    {
        string heartsContainerName = "HeartsContainer";
        GameObject heartsContainer = GameObject.Find(heartsContainerName);

        if (heartsContainer != null)
        {
            hearts = heartsContainer.GetComponentsInChildren<Image>();

            if (hearts.Length != maxHealth)
                Debug.LogError($"The number of hearts in UI ({hearts.Length}) does not match maxHealth ({maxHealth}).");

        }
        else
        {
            Debug.LogError($"Object '{heartsContainerName}' not found on the scene! Cannot update hearts.");
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        UpdateHearts();

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(int healAmount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + healAmount, maxHealth);
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        if (hearts == null || hearts.Length == 0) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < CurrentHealth ? fullHeart : emptyHeart;
            hearts[i].enabled = i < maxHealth;
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
    }
}