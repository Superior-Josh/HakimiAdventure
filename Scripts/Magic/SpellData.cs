using Godot;

namespace HakimiAdventure.Magic;

/// <summary> 法术数据配置 </summary>
[GlobalClass]
public partial class SpellData : Resource
{
    [Export] public string ID          { get; set; } = "";
    [Export] public string Name        { get; set; } = "法术";
    [Export] public string Description { get; set; } = "";
    [Export] public float  MpCost      { get; set; } = 10f;
    [Export] public float  StaminaCost { get; set; } = 5f;
    [Export] public float  Damage      { get; set; } = 25f;
    [Export] public float  Cooldown    { get; set; } = 3f;
    [Export] public float  CastTime    { get; set; } = 0.3f;
    [Export] public float  Range       { get; set; } = 10f;
    [Export] public bool   IsProjectile { get; set; } = true;
    [Export] public bool   Heals       { get; set; }
    [Export] public float  HealAmount  { get; set; }
}
