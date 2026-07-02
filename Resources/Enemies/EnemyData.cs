using Godot;

namespace HakimiAdventure;

/// <summary>
/// 敌人数据资源 — 编辑器中配置每种敌人。
/// </summary>
[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string EnemyName { get; set; } = "新敌人";
    [Export] public int HP { get; set; } = 50;
    [Export] public int Attack { get; set; } = 8;
    [Export] public int Defense { get; set; } = 3;
    [Export] public int Speed { get; set; } = 5;
    [Export] public int ExpReward { get; set; } = 20;

    /// <summary>是否为 Boss</summary>
    [Export] public bool IsBoss { get; set; } = false;
}
