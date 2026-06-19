using System;
using UnityEngine;
using static CharacterStats;

public abstract class Character : MonoBehaviour // IHealth
{
    [SerializeField] CharacterStats stats;
    [SerializeField] int baseMaxHealth = 100;
    [SerializeField] int strengthToHealthMultiplier = 10;
    [SerializeField] bool fullHealOnInit = true;

    int currentHealth;
    int maxHealth;
    bool isDead;

    public NPCSpecialization Class { get; private set; }
    public CharacterStats Stats => stats;
    public StatType SpecializationStat { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    #region Initialization

    public void Initialize(CharacterStats s)
    {
        if (!s)
        {
            Debug.LogError("Stats missing");
            return;
        }

        stats = s;
        SpecializationStat = s.GetHighestStat();
        Class = MapClass(SpecializationStat);
        InitializeHealth();
        Debug.Log($"{name} initialized as {Class} with spec {SpecializationStat}");
    }

    protected virtual NPCSpecialization MapClass(StatType spec)
    {
        if (spec == StatType.Creativity) return NPCSpecialization.Worker;
        if (spec == StatType.Strength) return NPCSpecialization.Fighter;
        if (spec == StatType.Resourcefulness) return NPCSpecialization.Cook;
        if (spec == StatType.Fieldcraft) return NPCSpecialization.Explorer;
        return NPCSpecialization.Worker;
    }

    void InitializeHealth()
    {
        maxHealth = CalculateMaxHealth();
        if (fullHealOnInit || currentHealth <= 0) currentHealth = maxHealth;
        else if (currentHealth > maxHealth) currentHealth = maxHealth;
        isDead = currentHealth <= 0;
        UpdateHearts();
    }

    protected virtual int CalculateMaxHealth()
    {
        var str = Stats ? Stats.Strength : 1;
        var result = baseMaxHealth + str * strengthToHealthMultiplier;
        if (result < 1) result = 1;
        return result;
    }

    #endregion

    #region Stats 

    public int GetEffectiveStat(StatType type, bool inSpecializationContext)
    {
        var baseVal = stats.Get(type);
        if (inSpecializationContext && type == SpecializationStat) return baseVal + 1;
        return baseVal;
    }

    #endregion

    #region Actions


    public virtual int ProduceFoodUnits()
    {
        var res = GetEffectiveStat(StatType.Resourcefulness, Class == NPCSpecialization.Cook);
        var units = Mathf.CeilToInt(res / 4f);
        if (units < 1) units = 1;
        return units;
    }

    public virtual int CraftItemQuality()
    {
        var cr = GetEffectiveStat(StatType.Creativity, Class == NPCSpecialization.Worker);
        if (cr < 1) return 1;
        if (cr > 10) return 10;
        return cr;
    }

    public virtual int AttackDamage()
    {
        var str = GetEffectiveStat(StatType.Strength, Class == NPCSpecialization.Fighter);
        if (str < 1) return 1;
        return str;
    }

    public virtual int CarryCapacity()
    {
        var fc = GetEffectiveStat(StatType.Fieldcraft, Class == NPCSpecialization.Explorer);
        if (fc < 1) return 1;
        return fc;
    }

    public virtual float GetDurationWithEfficiency(float baseSeconds)
    {
        if (baseSeconds <= 0f) return 0f;
        var eff = GetEffectiveStat(StatType.Efficiency, SpecializationStat == StatType.Efficiency);
        var mult = 1f - (eff - 1) * 0.05f;
        if (mult < 0.5f) mult = 0.5f;
        if (mult > 1f) mult = 1f;
        return baseSeconds * mult;
    }

    #endregion

    #region Health


    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (damage <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHearts();

        if (currentHealth > 0) return;
        Die();
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        if (healAmount <= 0) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHearts();
    }

    public virtual void UpdateHearts()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        currentHealth = 0;
        UpdateHearts();
        Died?.Invoke();
        Debug.Log($"{name} died");
    }

    #endregion
}