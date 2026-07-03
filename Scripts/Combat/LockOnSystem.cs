using Godot;

namespace HakimiAdventure.Combat;

/// <summary>
/// 锁定系统 — 按 Q 锁定最近敌人，相机跟随锁定目标。
/// </summary>
[GlobalClass]
public partial class LockOnSystem : Node
{
    [ExportCategory("Lock-On")]
    [Export] public float MaxLockRange   { get; set; } = 20f;
    [Export] public float LockAngle      { get; set; } = 60f;  // 屏幕中心锥体角度
    [Export] public float SwitchCooldown { get; set; } = 0.3f;

    // ── 公开状态 ──

    public Node3D? CurrentTarget { get; private set; }
    public bool    IsLocked      => CurrentTarget != null && IsInstanceValid(CurrentTarget);

    // ── 内部 ──

    private Player.PlayerController _player = null!;
    private Camera3D _camera = null!;
    private float _switchTimer;

    public override void _Ready()
    {
        _player = GetParent<Player.PlayerController>();
        _camera = _player.GetNode<Camera3D>("Camera3D");
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        if (_switchTimer > 0) _switchTimer -= dt;

        if (IsLocked)
        {
            UpdateLockedRotation(dt);

            // 检查目标是否死亡或超出范围
            if (CurrentTarget is IDamageable dmg && dmg.CurrentHP <= 0)
                Unlock();

            var dist = _player.GlobalPosition.DistanceTo(CurrentTarget!.GlobalPosition);
            if (dist > MaxLockRange * 1.5f)
                Unlock();
        }
    }

    // ── 公开 API ──

    /// <summary> 按需调用 （Q 键） </summary>
    public void ToggleLock()
    {
        if (IsLocked)
        {
            Unlock();
            return;
        }

        if (_switchTimer > 0) return;
        _switchTimer = SwitchCooldown;

        FindAndLockTarget();
    }

    /// <summary> 强制锁定指定目标 </summary>
    public void LockTarget(Node3D target)
    {
        if (target == null || !IsInstanceValid(target)) return;
        CurrentTarget = target;
    }

    public void Unlock()
    {
        CurrentTarget = null;
    }

    // ── 内部 ──

    private void FindAndLockTarget()
    {
        Node3D? bestTarget = null;
        var bestScore = float.MaxValue;

        var enemies = GetTree().GetNodesInGroup("enemies");
        var playerPos = _player.GlobalPosition;
        var cameraBasis = _camera.GlobalTransform.Basis;
        var cameraForward = -cameraBasis.Z;

        foreach (Node3D enemy in enemies)
        {
            if (enemy == null || !enemy.IsInsideTree()) continue;

            // 排除已死亡的
            if (enemy is IDamageable dmg && dmg.CurrentHP <= 0) continue;

            var toEnemy = enemy.GlobalPosition - playerPos;
            var dist = toEnemy.Length();
            if (dist > MaxLockRange) continue;

            // 屏幕中心锥体过滤
            var angle = cameraForward.AngleTo(toEnemy.Normalized());
            if (angle > Mathf.DegToRad(LockAngle)) continue;

            // 评分：距离 + 角度加权
            var score = dist + angle * 10f;
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        CurrentTarget = bestTarget;
    }

    /// <summary> 锁定状态下自动转向目标 </summary>
    private void UpdateLockedRotation(float delta)
    {
        if (CurrentTarget == null) return;

        var targetPos = CurrentTarget.GlobalPosition;
        targetPos.Y = _player.GlobalPosition.Y; // 只水平旋转

        var direction = _player.GlobalPosition.DirectionTo(targetPos);
        if (direction == Vector3.Zero) return;

        // 平滑旋转玩家面向目标
        var targetBasis = Basis.LookingAt(direction, Vector3.Up);
        _player.Transform = new Transform3D(
            _player.Transform.Basis.Slerp(targetBasis, delta * 8.0f),
            _player.GlobalPosition
        );

        // 相机额外绕 Y 轴微调
    }

    /// <summary> 获取锁定目标在屏幕上的位置（用于 HUD 指示器） </summary>
    public Vector2? GetTargetScreenPos()
    {
        if (!IsLocked) return null;
        return _camera.UnprojectPosition(CurrentTarget!.GlobalPosition);
    }
}
