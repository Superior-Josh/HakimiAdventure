using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;

namespace HakimiAdventure.Enemy;

/// <summary>
/// BOSS AI — 多阶段战斗。
/// P1 (HP 100-66%): 近战突进
/// P2 (HP 66-33%): 近战突进 + 范围震地
/// P3 (HP <33%): 全部招式 + 狂暴加速
/// </summary>
[GlobalClass]
public partial class BossAI : CharacterBody3D, IDamageable
{
    public enum BossPhase { P1_Melee, P2_Range, P3_Berserk }
    public enum AIState { Idle, Intro, Chase, Attack, Hit, Death }

    [Export] public float MaxHPValue    { get; set; } = 300f;
    [Export] public float MoveSpeed     { get; set; } = 3.5f;
    [Export] public float ChargeSpeed   { get; set; } = 8f;
    [Export] public float AttackDamage  { get; set; } = 20f;
    [Export] public float SlamDamage    { get; set; } = 30f;
    [Export] public float DetectionRange { get; set; } = 20f;
    [Export] public float AttackRange   { get; set; } = 3f;
    [Export] public string DisplayName  { get; set; } = "BOSS";

    public float   CurrentHP       { get; private set; }
    public float   MaxHP           => MaxHPValue;
    public BossPhase CurrentPhase  { get; private set; } = BossPhase.P1_Melee;

    private Player.PlayerController _player = null!;
    private AnimationPlayer _animPlayer = null!;
    private AIState _state;
    private float _stateTimer;
    private float _attackCooldown;

    public override void _Ready()
    {
        AddToGroup("enemies");
        AddToGroup("boss");
        CurrentHP = MaxHPValue;
        _animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController
                  ?? throw new System.Exception("BossAI: 没有 Player");
        _state = AIState.Idle;
    }

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;
        _stateTimer -= dt;
        _attackCooldown -= dt;

        // 阶段转换
        var hpRatio = CurrentHP / MaxHPValue;
        CurrentPhase = hpRatio > 0.66f ? BossPhase.P1_Melee :
                       hpRatio > 0.33f ? BossPhase.P2_Range :
                       BossPhase.P3_Berserk;

        switch (_state)
        {
            case AIState.Idle:  if (DetectPlayer()) _state = AIState.Chase; break;
            case AIState.Chase: UpdateChase(dt); break;
            case AIState.Attack: UpdateAttack(dt); break;
            case AIState.Hit:   UpdateHit(dt); break;
        }
    }

    private void UpdateChase(float dt)
    {
        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        var speed = CurrentPhase == BossPhase.P3_Berserk ? MoveSpeed * 1.5f : MoveSpeed;
        if (dist > AttackRange)
        {
            MoveToward(_player.GlobalPosition, speed);
        }
        else
        {
            _state = AIState.Attack;
            _stateTimer = 0.5f;
        }
    }

    private void UpdateAttack(float dt)
    {
        Velocity = Vector3.Zero;
        FaceTarget(_player.GlobalPosition);

        if (_attackCooldown <= 0)
        {
            if (CurrentPhase == BossPhase.P1_Melee)
                PerformCharge();
            else if (CurrentPhase == BossPhase.P2_Range)
                PerformSlamOrCharge();
            else
                PerformBerserkAttack();

            _attackCooldown = CurrentPhase == BossPhase.P3_Berserk ? 1.0f : 2.0f;
            _stateTimer = 0.8f;
        }

        if (_stateTimer <= 0)
            _state = AIState.Chase;
    }

    private void PerformCharge()
    {
        PlayAnim(CharacterAnimState.Attack);
        AudioManager.Instance?.PlaySfx(SfxGenerator.BossRoarSfx());

        var dir = GlobalPosition.DirectionTo(_player.GlobalPosition);
        Velocity = dir * ChargeSpeed;

        var timer = new Timer { OneShot = true, WaitTime = 0.4f };
        timer.Timeout += () =>
        {
            Velocity = Vector3.Zero;
            var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (dist <= AttackRange + 1.5f)
            {
                var dmg = DamageSystem.CalculateDamage(AttackDamage);
                _player.TakeDamage(dmg);
            }
        };
        AddChild(timer);
        timer.Start();
    }

    private void PerformSlamOrCharge()
    {
        if (GD.RandRange(0, 1) > 0.5f)
        {
            PerformCharge();
        }
        else
        {
            // 震地（AOE）
            PlayAnim(CharacterAnimState.Cast);
            AudioManager.Instance?.PlaySfx(SfxGenerator.ExplosionSfx());
            var dmg = DamageSystem.CalculateDamage(SlamDamage);
            _player.TakeDamage(dmg);
        }
    }

    private void PerformBerserkAttack()
    {
        PerformCharge();
        // 额外攻击
        var t = new Timer { OneShot = true, WaitTime = 0.6f };
        t.Timeout += () =>
        {
            if (GlobalPosition.DistanceTo(_player.GlobalPosition) <= AttackRange + 2f)
            {
                var dmg = DamageSystem.CalculateDamage(AttackDamage * 0.7f);
                _player.TakeDamage(dmg);
            }
        };
        AddChild(t);
        t.Start();
    }

    private void UpdateHit(float dt)
    {
        Velocity = Vector3.Zero;
        if (_stateTimer <= 0) _state = AIState.Chase;
    }

    private void MoveToward(Vector3 target, float speed)
    {
        var dir = GlobalPosition.DirectionTo(target); dir.Y = 0;
        if (dir.LengthSquared() < 0.01f) return;
        Velocity = dir * speed;
        MoveAndSlide();
        FaceTarget(target);
        PlayAnim(CharacterAnimState.Walk);
    }

    public void TakeDamage(float damage)
    {
        if (CurrentHP <= 0) return;
        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);
        if (CurrentHP <= 0) { HandleDeath(); return; }
        _state = AIState.Hit;
        _stateTimer = 0.3f;
    }

    private void HandleDeath()
    {
        PlayAnim(CharacterAnimState.Death);
        AudioManager.Instance?.PlaySfx(SfxGenerator.ExplosionSfx());
        Velocity = Vector3.Zero;
        SetCollisionLayerValue(1, false);
        GD.Print($"🏆 击败 {DisplayName}！");

        // 金币奖励
        if (_player != null) _player.Gold += 50;

        var t = new Timer { OneShot = true, WaitTime = 3f };
        t.Timeout += () => { RemoveFromGroup("enemies"); QueueFree(); };
        AddChild(t); t.Start();
    }

    private bool DetectPlayer()
    {
        return GlobalPosition.DistanceTo(_player.GlobalPosition) <= DetectionRange;
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
