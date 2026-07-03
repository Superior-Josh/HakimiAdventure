using Godot;
using HakimiAdventure.Core;

namespace HakimiAdventure.Combat;

/// <summary>
/// 体力管理器 — 运行时追踪体力值，处理消耗和自动恢复。
/// </summary>
[GlobalClass]
public partial class StaminaManager : Node
{
    [Export] public StaminaData Config { get; set; } = new();

    public float CurrentStamina { get; private set; }
    public float MaxStamina     => Config.MaxStamina;

    private float _regenTimer;

    public override void _Ready()
    {
        CurrentStamina = Config.MaxStamina;
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        // 消耗延迟倒计时
        if (_regenTimer > 0)
        {
            _regenTimer -= dt;
            return;
        }

        // 自动恢复
        if (CurrentStamina < Config.MaxStamina)
        {
            CurrentStamina = Mathf.Min(CurrentStamina + Config.RegenRate * dt, Config.MaxStamina);
        }
    }

    /// <summary> 检查是否足够体力 </summary>
    public bool HasStamina(float amount) => CurrentStamina >= amount;

    /// <summary> 消耗体力，成功返回 true </summary>
    public bool Consume(float amount)
    {
        if (CurrentStamina < amount) return false;
        CurrentStamina -= amount;
        _regenTimer = Config.RegenDelay;
        return true;
    }

    /// <summary> 消耗攻击体力（自动取配置中的值） </summary>
    public bool ConsumeAttack(bool heavy = false) =>
        Consume(heavy ? Config.HeavyAttackCost : Config.LightAttackCost);

    /// <summary> 立即恢复全部 </summary>
    public void RestoreFull() => CurrentStamina = Config.MaxStamina;
}
