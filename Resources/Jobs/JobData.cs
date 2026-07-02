using Godot;

namespace HakimiAdventure;

/// <summary>
/// 职业/流派数据资源 — Godot Resource，可在编辑器中像填表一样配置。
/// </summary>
[GlobalClass]
public partial class JobData : Resource
{
    [Export] public string JobName { get; set; } = "新职业";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

    // 基础属性
    [ExportGroup("基础属性")]
    [Export] public int BaseHP { get; set; } = 100;
    [Export] public int BaseMP { get; set; } = 50;
    [Export] public int BaseAttack { get; set; } = 10;
    [Export] public int BaseDefense { get; set; } = 5;
    [Export] public int BaseSpeed { get; set; } = 10;

    // 战斗类型
    [ExportGroup("战斗类型")]
    [Export] public bool IsMelee { get; set; } = true;
    [Export] public float AttackRange { get; set; } = 2.0f;
}
