using Godot;
using HakimiAdventure.Core;

namespace HakimiAdventure.Combat;

/// <summary>
/// 伤害计算系统 — 静态工具方法，处理伤害公式。
/// </summary>
public static class DamageSystem
{
    /// <summary> 基础伤害公式 </summary>
    public static float CalculateDamage(float baseDmg, float attackerStr = 1f, float defenderDef = 0f)
    {
        // 简单公式：基础伤害 × 攻击倍率 - 防御
        var raw = baseDmg * (1f + attackerStr * 0.1f);
        var reduction = defenderDef * 0.5f;
        return Mathf.Max(1f, raw - reduction);
    }

    /// <summary> 对 IDamageable 造成伤害，返回实际伤害值 </summary>
    public static float ApplyDamage(IDamageable target, float damage)
    {
        var actualDmg = CalculateDamage(damage);
        target.TakeDamage(actualDmg);
        return actualDmg;
    }
}
