using Godot;

namespace HakimiAdventure.Enemy;

/// <summary>
/// 敌人配置数据 — 可保存为 .tres 资源文件。
/// </summary>
[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public float MaxHP          { get; set; } = 50f;
    [Export] public float MoveSpeed      { get; set; } = 3.0f;
    [Export] public float ChaseSpeed     { get; set; } = 4.5f;
    [Export] public float Damage         { get; set; } = 10f;
    [Export] public float DetectionRange { get; set; } = 12f;
    [Export] public float AttackRange    { get; set; } = 2.5f;
    [Export] public float AttackCooldown { get; set; } = 1.5f;
    [Export] public float AttackWindup   { get; set; } = 0.4f;   // 前摇时间
    [Export] public float HitStunDuration { get; set; } = 0.3f;
    [Export] public int   ExpReward      { get; set; } = 10;
    [Export] public int   GoldReward     { get; set; } = 5;
    [Export] public string DisplayName   { get; set; } = "哥布林";
}
