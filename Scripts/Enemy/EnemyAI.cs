using Godot;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;

namespace HakimiAdventure.Enemy;

/// <summary>
/// 敌人 AI 状态机 — 巡逻 → 发现 → 追踪 → 攻击 → 硬直 → 死亡
/// </summary>
[GlobalClass]
public partial class EnemyAI : CharacterBody3D, IDamageable
{
    public enum AIState { Patrol, Alert, Chase, Attack, Hit, Death }

    // ── 导出 ──

    [Export] public EnemyData Config { get; set; } = new();

    // ── 公开状态 ──

    public AIState    CurrentState    { get; private set; } = AIState.Patrol;
    public float      CurrentHP       { get; private set; }
    public float      MaxHP           => Config.MaxHP;

    // ── 内部 ──

    private Player.PlayerController _player = null!;
    private AnimationPlayer _animPlayer = null!;
    private float _stateTimer;
    private float _attackCooldown;
    private Vector3 _patrolOrigin;
    private Vector3 _patrolTarget;

    public override void _Ready()
    {
        AddToGroup("enemies");
        CurrentHP = Config.MaxHP;
        _animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

        // 自动定位玩家
        _player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController
                  ?? throw new System.Exception("EnemyAI: 场景中没有 Player (group=player)");

        _patrolOrigin = GlobalPosition;
        PickNewPatrolTarget();
    }

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;
        _stateTimer -= dt;
        _attackCooldown -= dt;

        switch (CurrentState)
        {
            case AIState.Patrol:    UpdatePatrol(dt);    break;
            case AIState.Alert:     UpdateAlert(dt);     break;
            case AIState.Chase:     UpdateChase(dt);     break;
            case AIState.Attack:    UpdateAttack(dt);    break;
            case AIState.Hit:       UpdateHit(dt);       break;
            case AIState.Death:     /* 等待动画完成 */   break;
        }
    }

    // ── 状态更新 ──

    private void UpdatePatrol(float dt)
    {
        MoveToward(_patrolTarget, Config.MoveSpeed);

        var distToTarget = GlobalPosition.DistanceTo(_patrolTarget);
        if (distToTarget < 0.5f || _stateTimer <= 0)
        {
            PickNewPatrolTarget();
            _stateTimer = GD.RandRange(2f, 5f);
        }

        // 检测玩家
        if (DetectPlayer())
        {
            EnterState(AIState.Alert);
        }
    }

    private void UpdateAlert(float dt)
    {
        // 短暂停顿 → 转向玩家
        FaceTarget(_player.GlobalPosition);

        if (_stateTimer <= 0)
        {
            EnterState(AIState.Chase);
        }
    }

    private void UpdateChase(float dt)
    {
        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);

        if (dist <= Config.AttackRange)
        {
            EnterState(AIState.Attack);
            return;
        }

        if (dist > Config.DetectionRange * 1.5f)
        {
            EnterState(AIState.Patrol);
            return;
        }

        MoveToward(_player.GlobalPosition, Config.ChaseSpeed);
    }

    private void UpdateAttack(float dt)
    {
        FaceTarget(_player.GlobalPosition);
        Velocity = Vector3.Zero;

        if (_stateTimer <= 0 && _attackCooldown <= 0)
        {
            // 执行攻击
            PerformAttack();
            _attackCooldown = Config.AttackCooldown;
            _stateTimer = Config.AttackWindup + 0.2f;
            PlayAnim(CharacterAnimState.Attack);

            // 攻击结束后回到追踪
            var attackEnd = new Timer { OneShot = true, WaitTime = Config.AttackWindup + 0.3f };
            attackEnd.Timeout += () =>
            {
                if (CurrentState == AIState.Attack)
                    EnterState(AIState.Chase);
            };
            AddChild(attackEnd);
            attackEnd.Start();
        }

        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > Config.AttackRange * 1.5f)
        {
            EnterState(AIState.Chase);
        }
    }

    private void UpdateHit(float dt)
    {
        Velocity = Vector3.Zero;
        if (_stateTimer <= 0)
        {
            EnterState(AIState.Chase);
        }
    }

    // ── 内部方法 ──

    private void MoveToward(Vector3 target, float speed)
    {
        var dir = GlobalPosition.DirectionTo(target);
        dir.Y = 0;
        if (dir.LengthSquared() < 0.01f) return;

        Velocity = dir * speed;
        MoveAndSlide();

        // 面朝移动方向
        if (dir != Vector3.Zero)
        {
            var targetBasis = Basis.LookingAt(dir, Vector3.Up);
            Transform = new Transform3D(
                Transform.Basis.Slerp(targetBasis, 0.15f),
                GlobalPosition
            );
        }

        PlayAnim(CharacterAnimState.Walk);
    }

    private bool DetectPlayer()
    {
        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > Config.DetectionRange) return false;

        // 视线检测（可选射线阻挡）
        var space = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            GlobalPosition + Vector3.Up,
            _player.GlobalPosition + Vector3.Up * 0.8f
        );
        query.CollideWithAreas = false;
        var result = space.IntersectRay(query);

        return result.Count == 0 || ((Node3D?)result["collider"])?.IsInGroup("player") == true;
    }

    private void FaceTarget(Vector3 target)
    {
        var dir = GlobalPosition.DirectionTo(target);
        dir.Y = 0;
        if (dir == Vector3.Zero) return;
        var targetBasis = Basis.LookingAt(dir, Vector3.Up);
        Transform = new Transform3D(
            Transform.Basis.Slerp(targetBasis, 0.2f),
            GlobalPosition
        );
    }

    private void PerformAttack()
    {
        var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist <= Config.AttackRange + 1.0f)
        {
            var dmg = DamageSystem.CalculateDamage(Config.Damage);
            _player.TakeDamage(dmg);
            _player.PlayAnim(CharacterAnimState.Hit);
        }
    }

    private void PickNewPatrolTarget()
    {
        var offset = new Vector3(
            GD.RandRange(-5f, 5f),
            0,
            GD.RandRange(-5f, 5f)
        );
        _patrolTarget = _patrolOrigin + offset;
    }

    private void EnterState(AIState newState)
    {
        CurrentState = newState;
        _stateTimer = 0f;

        switch (newState)
        {
            case AIState.Alert:  _stateTimer = 0.5f; break;
            case AIState.Attack: _stateTimer = 0.5f; break;
            case AIState.Hit:    _stateTimer = Config.HitStunDuration; break;
            case AIState.Death:  HandleDeath(); break;
        }
    }

    private void PlayAnim(CharacterAnimState state)
    {
        _animPlayer?.Play(CharacterAnimHelper.GetAnimName(state));
    }

    // ── IDamageable ──

    public void TakeDamage(float damage)
    {
        if (CurrentState == AIState.Death) return;

        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);

        if (CurrentHP <= 0)
        {
            EnterState(AIState.Death);
        }
        else
        {
            EnterState(AIState.Hit);
        }
    }

    private void HandleDeath()
    {
        PlayAnim(CharacterAnimState.Death);
        Velocity = Vector3.Zero;
        SetCollisionLayerValue(1, false);

        // 延迟移除
        var deathTimer = new Timer { OneShot = true, WaitTime = 2.0f };
        deathTimer.Timeout += () =>
        {
            RemoveFromGroup("enemies");
            QueueFree();
        };
        AddChild(deathTimer);
        deathTimer.Start();
    }
}
