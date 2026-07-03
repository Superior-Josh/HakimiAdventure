using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;

namespace HakimiAdventure.Enemy;

/// <summary>
/// 弓箭手敌人 — 远程追踪，保持距离射击。
/// </summary>
[GlobalClass]
public partial class RangedEnemyAI : CharacterBody3D, IDamageable
{
    [Export] public float MaxHPValue    { get; set; } = 30f;
    [Export] public float MoveSpeed     { get; set; } = 2.5f;
    [Export] public float Damage        { get; set; } = 12f;
    [Export] public float DetectionRange { get; set; } = 18f;
    [Export] public float PreferredDist  { get; set; } = 8f;
    [Export] public float AttackCooldown { get; set; } = 2.0f;

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
        if (dist > DetectionRange) { MoveToward(_player.GlobalPosition, MoveSpeed); return; }

        FaceTarget(_player.GlobalPosition);

        if (dist < PreferredDist - 2f)
        {
            // 太近了，后退
            var away = GlobalPosition - _player.GlobalPosition; away.Y = 0;
            MoveToward(GlobalPosition + away.Normalized() * 3f, MoveSpeed);
        }
        else if (dist > PreferredDist + 1f)
        {
            MoveToward(_player.GlobalPosition, MoveSpeed);
        }
        else
        {
            Velocity = Vector3.Zero;
            if (_attackTimer <= 0)
            {
                Shoot();
                _attackTimer = AttackCooldown;
            }
        }
        MoveAndSlide();
    }

    private void Shoot()
    {
        PlayAnim(CharacterAnimState.Attack);
        AudioManager.Instance?.PlaySfx(SfxGenerator.AttackSfx());
        var dmg = DamageSystem.CalculateDamage(Damage);
        _player.TakeDamage(dmg);
    }

    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);
        if (CurrentHP <= 0)
        {
            _player?.AddExpReward(15);
            if (_player != null) _player.Gold += 8;
            QueueFree();
        }
    }

    private void MoveToward(Vector3 target, float speed)
    {
        var dir = GlobalPosition.DirectionTo(target); dir.Y = 0;
        if (dir.LengthSquared() < 0.01f) return;
        Velocity = dir * speed;
    }

    private void FaceTarget(Vector3 target)
    {
        var dir = GlobalPosition.DirectionTo(target); dir.Y = 0;
        if (dir == Vector3.Zero) return;
        Transform = new Transform3D(
            Transform.Basis.Slerp(Basis.LookingAt(dir, Vector3.Up), 0.15f),
            GlobalPosition
        );
    }

    private void PlayAnim(CharacterAnimState state)
    {
        _animPlayer?.Play(CharacterAnimHelper.GetAnimName(state));
    }
}
