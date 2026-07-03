using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;

namespace HakimiAdventure.Enemy;

/// <summary>
/// 骷髅兵 — 快速近战，高攻速低血量。
/// </summary>
[GlobalClass]
public partial class FastEnemyAI : CharacterBody3D, IDamageable
{
    [Export] public float MaxHPValue    { get; set; } = 20f;
    [Export] public float MoveSpeed     { get; set; } = 6f;
    [Export] public float Damage        { get; set; } = 8f;
    [Export] public float DetectionRange { get; set; } = 14f;
    [Export] public float AttackRange   { get; set; } = 2f;
    [Export] public float AttackCooldown { get; set; } = 0.8f;

    public float CurrentHP { get; private set; }
    public float MaxHP => MaxHPValue;

    private Player.PlayerController _player = null!;
    private AnimationPlayer _animPlayer = null!;
    private float _attackTimer;

    public override void _Ready()
    {
        AddToGroup("enemies");
        CurrentHP = MaxHPValue;
        _animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController!;
    }

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;
        _attackTimer -= dt;

        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > DetectionRange) return;

        FaceTarget(_player.GlobalPosition);

        if (dist <= AttackRange && _attackTimer <= 0)
        {
            PerformAttack();
            _attackTimer = AttackCooldown;
        }
        else if (dist > AttackRange)
        {
            MoveToward(_player.GlobalPosition, MoveSpeed);
        }
    }

    private void PerformAttack()
    {
        PlayAnim(CharacterAnimState.Attack);
        AudioManager.Instance?.PlaySfx(SfxGenerator.HitSfx());
        var dmg = DamageSystem.CalculateDamage(Damage);
        _player.TakeDamage(dmg);
    }

    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);
        if (CurrentHP <= 0)
        {
            _player?.AddExpReward(8);
            if (_player != null) _player.Gold += 3;
            QueueFree();
        }
    }

    private void MoveToward(Vector3 target, float speed)
    {
        var dir = GlobalPosition.DirectionTo(target); dir.Y = 0;
        if (dir.LengthSquared() < 0.01f) return;
        Velocity = dir * speed;
        MoveAndSlide();
    }

    private void FaceTarget(Vector3 target)
    {
        var dir = GlobalPosition.DirectionTo(target); dir.Y = 0;
        if (dir == Vector3.Zero) return;
        Transform = new Transform3D(
            Transform.Basis.Slerp(Basis.LookingAt(dir, Vector3.Up), 0.2f),
            GlobalPosition
        );
    }

    private void PlayAnim(CharacterAnimState state)
    {
        _animPlayer?.Play(CharacterAnimHelper.GetAnimName(state));
    }
}
