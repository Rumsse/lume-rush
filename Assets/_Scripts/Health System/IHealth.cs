public interface IHealth
{
    int CurrentHealth { get; }
    int MaxHealth { get; }

    void TakeDamage(int damage);
    void Heal(int healAmount);
    void UpdateHearts();
    void Die();
}