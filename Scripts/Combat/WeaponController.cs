using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Core;

namespace HakimiAdventure.Combat;

/// <summary>
/// 武器控制器 — 攻击命中判定 + 连击窗口管理。
/// 使用距离 + 角度检测攻击范围内的敌人。
/// </summary>
[GlobalClass]
public partial class WeaponController : Node3D
{
    [ExportCategory("Weapon Stats")]
    [Export] public float BaseDamage    { get; set; } = 20f;
    [Export] public float AttackRange   { get; set; } = 2.8f;
    [Export] public float AttackAngle   { get; set; } = 45f;   // 半角 °
    [Export] public float ComboWindow   { get; set; } = 0.6f;  // 连击输入窗口

    [ExportCategory("Timing")]
    [Export] public float LightWindup   { get; set; } = 0.15f; // 前摇 → 判定帧
    [Export] public float LightRecovery { get; set; } = 0.35f; // 后摇恢复

    // ── 内部状态 ──

    public bool IsAttacking { get; private set; }
    public bool IsInCombo   { get; private set; }

    private Player.PlayerController _player = null!;
    private Timer _hitWindowTimer;
    private bool _hitRegistered;
    private int _comboCount;
    private float _comboTimer;

    public override void _Ready()
    {
        _player = GetParent<Player.PlayerController>();
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        // 连击窗口倒计时
        if (_comboTimer > 0)
        {
            _comboTimer -= dt;
            if (_comboTimer <= 0) _comboCount = 0;
        }
    }

    /// <summary> 执行一次攻击，返回 true 表示攻击发起成功 </summary>
    public bool StartAttack()
    {
        if (IsAttacking) return false;

        IsAttacking = true;
        _hitRegistered = false;
        _player.PlayAnim(CharacterAnimState.Attack);
        AudioManager.Instance?.PlaySfx(SfxGenerator.AttackSfx());

        // 命中判定计时器
        var timer = new Timer { OneShot = true, WaitTime = LightWindup };
        timer.Timeout += OnHitWindow;
        AddChild(timer);
        timer.Start();

        // 记录这个 timer 以便清理
        _hitWindowTimer = timer;

        // 后摇恢复（用动画播放完后的 Timer）
        var recoverTimer = new Timer { OneShot = true, WaitTime = LightWindup + LightRecovery };
        recoverTimer.Timeout += OnAttackEnd;
        AddChild(recoverTimer);
        recoverTimer.Start();

        return true;
    }

    /// <summary> 命中判定窗口打开 </summary>
    private void OnHitWindow()
    {
        PerformHitDetection();
        _hitRegistered = true;
    }

    /// <summary> 执行命中检测 </summary>
    private void PerformHitDetection()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        var playerPos = _player.GlobalPosition;
        var forward = -_player.GlobalTransform.Basis.Z;
        var halfAngle = Mathf.DegToRad(AttackAngle);

        foreach (Node3D enemy in enemies)
        {
            if (enemy == null || !enemy.IsInsideTree()) continue;

            var toEnemy = (enemy.GlobalPosition - playerPos);
            var dist = toEnemy.Length();
            if (dist > AttackRange) continue;

            var angle = forward.AngleTo(toEnemy.Normalized());
            if (angle > halfAngle) continue;

            // 命中！
            HitEnemy(enemy);
            AudioManager.Instance?.PlaySfx(SfxGenerator.HitSfx());
        }
    }

    private void HitEnemy(Node3D enemy)
    {
        // 查找 IDamageable
        if (enemy is IDamageable dmg)
        {
            dmg.TakeDamage(BaseDamage);
        }
        else
        {
            // 也可能挂在子节点
            var dmgComponent = enemy.FindChild("DamageSystem", true, false) as IDamageable;
            dmgComponent?.TakeDamage(BaseDamage);
        }

        // 触发连击窗口
        _comboCount++;
        _comboTimer = ComboWindow;
        IsInCombo = true;
    }

    /// <summary> 尝试连击 </summary>
    public bool TryCombo()
    {
        if (!IsInCombo || _comboTimer <= 0 || IsAttacking) return false;
        _comboCount++;
        StartAttack();
        return true;
    }

    private void OnAttackEnd()
    {
        IsAttacking = false;
        _player.PlayAnim(CharacterAnimState.Idle);
    }
}
