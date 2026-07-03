namespace HakimiAdventure.Combat;

/// <summary>
/// 可受击接口 — 任何可以受伤的对象实现此接口。
/// </summary>
public interface IDamageable
{
    float CurrentHP { get; }
    float MaxHP     { get; }
    void   TakeDamage(float damage);
}
