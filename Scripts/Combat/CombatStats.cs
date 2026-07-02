using Godot;

namespace HakimiAdventure;

/// <summary>
/// 战斗数值 — 运行时角色（玩家/敌人）的战斗状态。
/// </summary>
public class CombatStats
{
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }
    public int MaxMP { get; set; }
    public int CurrentMP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public bool IsMelee { get; set; }
    public float AttackRange { get; set; }

    public bool IsDead => CurrentHP <= 0;

    public CombatStats(JobData job)
    {
        MaxHP = job.BaseHP;
        CurrentHP = MaxHP;
        MaxMP = job.BaseMP;
        CurrentMP = MaxMP;
        Attack = job.BaseAttack;
        Defense = job.BaseDefense;
        Speed = job.BaseSpeed;
        IsMelee = job.IsMelee;
        AttackRange = job.AttackRange;
    }

    public CombatStats(EnemyData enemy)
    {
        MaxHP = enemy.HP;
        CurrentHP = MaxHP;
        MaxMP = 0;
        CurrentMP = 0;
        Attack = enemy.Attack;
        Defense = enemy.Defense;
        Speed = enemy.Speed;
        IsMelee = true;
        AttackRange = 2.0f;
    }

    /// <summary>计算实际伤害</summary>
    public int CalculateDamage(CombatStats attacker)
    {
        int rawDamage = Mathf.Max(1, attacker.Attack - Defense);
        return rawDamage;
    }

    /// <summary>受到伤害，返回实际扣血量</summary>
    public int TakeDamage(int amount)
    {
        int actual = Mathf.Min(amount, CurrentHP);
        CurrentHP -= actual;
        return actual;
    }
}
