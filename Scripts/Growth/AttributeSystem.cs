using Godot;
using HakimiAdventure.Core;

namespace HakimiAdventure.Growth;

/// <summary>
/// 属性系统 — 力量/敏捷 → 派生属性。
/// </summary>
[GlobalClass]
public partial class AttributeSystem : Node
{
    [Export] public int Strength       { get; set; } = 5;
    [Export] public int Agility        { get; set; } = 5;

    // ── 派生值 ──
    public float AttackPower  => 10f + Strength * 2f;
    public float Defense      => Agility * 1.5f;
    public float MaxHPBonus   => Strength * 10f;
    public float MaxMPBonus   => Agility * 5f;

    private Player.PlayerController _player = null!;

    public override void _Ready()
    {
        _player = GetParent<Player.PlayerController>();
    }

    /// <summary> 将加点效果应用到玩家 </summary>
    public void ApplyStats()
    {
        // max HP/MP 由 AttributeSystem 和基础值共同决定
        // Player.MaxHP/MaxMP 可改为计算属性
    }

    /// <summary> 计算实际伤害 = 攻击力 + 武器基础伤害 </summary>
    public float GetModifiedDamage(float baseDamage) => baseDamage + AttackPower;
}
