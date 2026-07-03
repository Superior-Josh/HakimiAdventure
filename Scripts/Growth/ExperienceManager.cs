using Godot;
using System.Collections.Generic;
using HakimiAdventure.Combat;

namespace HakimiAdventure.Growth;

/// <summary>
/// 经验管理器 — 击杀获得经验 + 升级。
/// </summary>
[GlobalClass]
public partial class ExperienceManager : Node
{
    [Export] public int   Level       { get; set; } = 1;
    [Export] public int   CurrentExp  { get; set; }
    [Export] public int   ExpToNext   { get; set; } = 50;
    [Export] public int   StatPoints  { get; set; }

    // ── 事件 ──
    [Signal] public delegate void LeveledUpEventHandler(int newLevel);

    private Player.PlayerController _player = null!;

    public override void _Ready()
    {
        _player = GetParent<Player.PlayerController>();
    }

    /// <summary> 增加经验 </summary>
    public void AddExp(int amount)
    {
        CurrentExp += amount;
        while (CurrentExp >= ExpToNext)
        {
            CurrentExp -= ExpToNext;
            Level++;
            StatPoints += 3;
            ExpToNext = Mathf.FloorToInt(ExpToNext * 1.25f);
            EmitSignal(SignalName.LeveledUp, Level);
        }
    }

    /// <summary> 消耗属性点 </summary>
    public bool SpendPoint()
    {
        if (StatPoints <= 0) return false;
        StatPoints--;
        return true;
    }

    public float ExpProgress => (float)CurrentExp / ExpToNext;
}
