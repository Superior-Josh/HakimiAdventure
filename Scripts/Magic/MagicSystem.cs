using Godot;
using System.Collections.Generic;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;

namespace HakimiAdventure.Magic;

/// <summary>
/// 法术系统 — 施法流程 + MP 管理 + 独立冷却管理器。
/// </summary>
[GlobalClass]
public partial class MagicSystem : Node
{
    [Export] public Godot.Collections.Array<SpellData> LearnedSpells { get; set; } = new();

    public Dictionary<string, float> Cooldowns { get; } = new();

    private Player.PlayerController _player = null!;

    public override void _Ready()
    {
        _player = GetParent<Player.PlayerController>();
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        // 冷却递减
        var expired = new List<string>();
        foreach (var kv in Cooldowns)
        {
            Cooldowns[kv.Key] = kv.Value - dt;
            if (Cooldowns[kv.Key] <= 0) expired.Add(kv.Key);
        }
        foreach (var key in expired) Cooldowns.Remove(key);
    }

    /// <summary> 施放法术（按编号 0-3） </summary>
    public bool CastSpell(int spellIndex)
    {
        if (spellIndex < 0 || spellIndex >= LearnedSpells.Count) return false;
        var spell = LearnedSpells[spellIndex];
        if (spell == null) return false;

        // 检查冷却
        if (Cooldowns.TryGetValue(spell.ID, out var cd) && cd > 0) return false;

        // 检查 MP
        if (_player.MP < spell.MpCost) return false;

        // 消耗
        _player.MP -= spell.MpCost;

        // 开始冷却
        Cooldowns[spell.ID] = spell.Cooldown;

        // 播放施法动画
        _player.PlayAnim(CharacterAnimState.Cast);

        // 延迟执行
        var timer = new Timer { OneShot = true, WaitTime = spell.CastTime };
        timer.Timeout += () => ExecuteSpell(spell);
        AddChild(timer);
        timer.Start();

        return true;
    }

    private void ExecuteSpell(SpellData spell)
    {
        _player.PlayAnim(CharacterAnimState.Idle);

        if (spell.Heals)
        {
            _player.CurrentHP = Mathf.Min(_player.CurrentHP + spell.HealAmount, _player.MaxHP);
            return;
        }

        // 弹道或直接伤害
        if (spell.IsProjectile)
        {
            SpawnProjectile(spell);
        }
        else
        {
            // 直接伤害前方敌人
            var enemies = GetTree().GetNodesInGroup("enemies");
            foreach (Node3D enemy in enemies)
            {
                var dist = _player.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist <= spell.Range && enemy is IDamageable dmg)
                {
                    dmg.TakeDamage(spell.Damage);
                }
            }
        }
    }

    private void SpawnProjectile(SpellData spell)
    {
        // 简单弹道：使用 Area3D 碰撞检测
        var projectile = new Area3D();
        var mesh = new MeshInstance3D { Mesh = new SphereMesh { Radius = 0.15f, Height = 0.3f } };
        mesh.MaterialOverride = new StandardMaterial3D
        {
            AlbedoColor = spell.Heals ? new Color(0, 1, 0) : new Color(1, 0.5f, 0),
            EmissionEnabled = true,
            Emission = spell.Heals ? new Color(0, 1, 0) : new Color(1, 0.5f, 0)
        };
        projectile.AddChild(mesh);

        var shape = new CollisionShape3D { Shape = new SphereShape3D { Radius = 0.2f } };
        projectile.AddChild(shape);

        projectile.BodyEntered += (body) =>
        {
            if (body is IDamageable dmg && body != _player)
            {
                dmg.TakeDamage(spell.Damage);
            }
            projectile.QueueFree();
        };

        // 位置和方向
        var cam = _player.GetNode<Camera3D>("Camera3D");
        projectile.GlobalPosition = cam.GlobalPosition - cam.GlobalTransform.Basis.Z * 0.5f;
        GetTree().Root.AddChild(projectile);

        // 飞行动画
        var tween = GetTree().CreateTween();
        var target = cam.GlobalPosition - cam.GlobalTransform.Basis.Z * spell.Range;
        tween.TweenProperty(projectile, "global_position", target, 0.5f);
        tween.Finished += () =>
        {
            if (IsInstanceValid(projectile)) projectile.QueueFree();
        };
    }

    /// <summary> 学习新法术 </summary>
    public void LearnSpell(SpellData spell)
    {
        if (!LearnedSpells.Contains(spell))
            LearnedSpells.Add(spell);
    }

    /// <summary> 获取法术冷却剩余 </summary>
    public float GetCooldownRemaining(SpellData spell)
    {
        return Cooldowns.TryGetValue(spell.ID, out var cd) ? Mathf.Max(0, cd) : 0;
    }
}
