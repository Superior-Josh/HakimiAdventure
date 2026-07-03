using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Combat;

namespace HakimiAdventure.Enemy;

/// <summary>
/// 吸血鬼 BOSS — 传送 + 吸血。
/// P1: 瞬移突袭 | P2: 范围血爆 | P3: 狂暴连击+吸血
/// </summary>
[GlobalClass]
public partial class VampireBossAI : CharacterBody3D, IDamageable
{
    [Export] public float MaxHPValue    { get; set; } = 250f;
    [Export] public float AttackDamage  { get; set; } = 18f;
    public float CurrentHP { get; private set; }
    public float MaxHP => MaxHPValue;

    private Player.PlayerController _player = null!;
    private AnimationPlayer _animPlayer = null!;
    private float _attackTimer;
    private bool _enraged;

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
        if (!_enraged && hpRatio < 0.33f) _enraged = true;

        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > 15f) { MoveToward(_player.GlobalPosition, 5f); return; }

        if (_attackTimer <= 0)
        {
            if (hpRatio > 0.66f) TeleportStrike();
            else if (hpRatio > 0.33f) BloodExplosion();
            else { TeleportStrike(); LifeDrain(); }
            _attackTimer = _enraged ? 1.2f : 2f;
        }
    }

    private void TeleportStrike()
    {
        // 瞬移到玩家附近
        var offset = new Vector3(GD.RandRange(-2f, 2f), 0, GD.RandRange(-2f, 2f));
        GlobalPosition = _player.GlobalPosition + offset;
        PlayAnim(CharacterAnimState.Attack);
        AudioManager.Instance?.PlaySfx(SfxGenerator.BossRoarSfx());

        if (GlobalPosition.DistanceTo(_player.GlobalPosition) <= 3f)
        {
            _player.TakeDamage(DamageSystem.CalculateDamage(AttackDamage));
            CurrentHP = Mathf.Min(CurrentHP + 5f, MaxHPValue);
        }
    }

    private void BloodExplosion()
    {
        AudioManager.Instance?.PlaySfx(SfxGenerator.ExplosionSfx());
        PlayAnim(CharacterAnimState.Cast);
        var dmg = DamageSystem.CalculateDamage(AttackDamage * 1.3f);
        _player.TakeDamage(dmg);
    }

    private void LifeDrain()
    {
        AudioManager.Instance?.PlaySfx(SfxGenerator.HitSfx());
        var dmg = DamageSystem.CalculateDamage(AttackDamage * 0.6f);
        _player.TakeDamage(dmg);
        CurrentHP = Mathf.Min(CurrentHP + 10f, MaxHPValue);
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
            if (_player != null) { _player.Gold += 60; _player.AddExpReward(80); }
            var t = new Timer { OneShot = true, WaitTime = 2f };
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

    private void PlayAnim(CharacterAnimState s) => _animPlayer?.Play(CharacterAnimHelper.GetAnimName(s));
}
