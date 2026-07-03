using Godot;

namespace HakimiAdventure.Combat;

/// <summary>
/// 体力配置数据 — 可保存为 .tres 资源文件。
/// </summary>
[GlobalClass]
public partial class StaminaData : Resource
{
    [Export] public float MaxStamina    { get; set; } = 100f;
    [Export] public float RegenRate     { get; set; } = 15f;     // 每秒恢复
    [Export] public float RegenDelay    { get; set; } = 1.0f;    // 消耗后等待时间
    [Export] public float LightAttackCost { get; set; } = 15f;
    [Export] public float HeavyAttackCost { get; set; } = 30f;
    [Export] public float SprintCost    { get; set; } = 10f;     // 每秒
}
