using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Combat;

namespace HakimiAdventure.Enemy;

/// <summary>
/// 冰霜巨人 BOSS — 冰锥 + 冰环 + 冻结。
/// P1: 冰锥投射 | P2: 冰环 AOE | P3: 冰锥+冰环 交替
/// </summary>
[GlobalClass]
public partial class IceGolemBossAI : CharacterBody3D, IDamageable
{
    [Export] public float MaxHPValue   { get; set; } = 400f;
    [Export] public float AttackDamage { get; set; } = 22f;
    public float CurrentHP { get; private set; }
    public float MaxHP => MaxHPValue;

    private Player.PlayerController _player = null!;
    private AnimationPlayer _animPlayer = null!;
    private float _attackTimer;
    private bool _phase2;
    private bool _phase3;

    public override void _Ready()
    {
        AddToGroup("enemies"); AddToGroup("boss");
        CurrentHP = MaxHPValue;
        _animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController!;
    }

    public override void _PhysicsProcess(double delta)
    {
        _attackTimer -= (float)delta;
        var hpRatio = CurrentHP / MaxHPValue;
        if (hpRatio < 0.66f) _phase2 = true;
        if (hpRatio < 0.33f) _phase3 = true;

        FaceTarget(_player.GlobalPosition);
        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > 18f) { MoveToward(_player.GlobalPosition, 3f); return; }

        if (_attackTimer <= 0)
        {
            if (_phase3) { IceShard(); IceRing(); }
            else if (_phase2) { if (GD.Randf() > 0.5f) IceShard(); else IceRing(); }
            else IceShard();
            _attackTimer = _phase3 ? 1.5f : 2.5f;
        }
    }

    private void IceShard()
    {
        PlayAnim(CharacterAnimState.Attack);
        AudioManager.Instance?.PlaySfx(SfxGenerator.AttackSfx());
        var dmg = DamageSystem.CalculateDamage(AttackDamage);
        _player.TakeDamage(dmg);
    }

    private void IceRing()
    {
        PlayAnim(CharacterAnimState.Cast);
        AudioManager.Instance?.PlaySfx(SfxGenerator.ExplosionSfx());
        // 范围 AOE
        var dmg = DamageSystem.CalculateDamage(AttackDamage * 1.5f);
        _player.TakeDamage(dmg);
    }

    public void TakeDamage(float damage)
    {
        if (CurrentHP <= 0) return;
        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);
        if (CurrentHP <= 0)
        {
            PlayAnim(CharacterAnimState.Death);
            AudioManager.Instance?.PlaySfx(SfxGenerator.ExplosionSfx());
            if (_player != null) { _player.Gold += 80; _player.AddExpReward(100); }
            var t = new Timer { OneShot = true, WaitTime = 2.5f };
            t.Timeout += () => { RemoveFromGroup("enemies"); QueueFree(); };
            AddChild(t); t.Start();
        }
    }

    private void MoveToward(Vector3 target, float speed)
    {
        var dir = GlobalPosition.DirectionTo(target); dir.Y = 0;
        if (dir.LengthSquared() < 0.01f) return;
        Velocity = dir * speed; MoveAndSlide();
    }

    private void FaceTarget(Vector3 t)
    {
        var d = GlobalPosition.DirectionTo(t); d.Y = 0;
        if (d == Vector3.Zero) return;
        Transform = new Transform3D(Transform.Basis.Slerp(Basis.LookingAt(d, Vector3.Up), 0.1f), GlobalPosition);
    }

    private void PlayAnim(CharacterAnimState s) => _animPlayer?.Play(CharacterAnimHelper.GetAnimName(s));
}
