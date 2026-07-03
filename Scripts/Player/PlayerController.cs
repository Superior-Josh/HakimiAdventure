using Godot;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;

namespace HakimiAdventure.Player;

/// <summary>
/// 第一人称玩家控制器 — WASD 移动 + 鼠标视角 + 重力/碰撞。
/// </summary>
[GlobalClass]
public partial class PlayerController : CharacterBody3D, IDamageable
{
    // ── 导出参数 ──

    [ExportCategory("Movement")]
    [Export] public float WalkSpeed     { get; set; } = 5.0f;
    [Export] public float SprintSpeed   { get; set; } = 8.0f;
    [Export] public float Acceleration  { get; set; } = 10.0f;
    [Export] public float AirControl    { get; set; } = 1.0f;

    [ExportCategory("Combat")]
    [Export] public float MaxHP  { get; set; } = 100f;
    public        float CurrentHP { get; private set; }

    [ExportCategory("Mouse")]
    [Export] public float MouseSensitivity { get; set; } = 0.002f;
    [Export] public bool  InvertY         { get; set; } = false;

    // ── 内部状态 ──

    private CameraController _camera = null!;
    private AnimationPlayer  _animPlayer = null!;
    private StaminaManager   _stamina = null!;
    private WeaponController _weapon = null!;
    private LockOnSystem     _lockOn = null!;
    private Vector3 _targetVelocity;
    private CharacterAnimState _currentAnim;

    // ── 生命周期 ──

    public override void _Ready()
    {
        _camera     = GetNode<CameraController>("Camera3D");
        _animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _stamina    = GetNode<StaminaManager>("StaminaManager");
        _weapon     = GetNode<WeaponController>("WeaponController");
        _lockOn     = GetNode<LockOnSystem>("LockOnSystem");

        AddToGroup("player");
        CurrentHP = MaxHP;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // 水平旋转
            RotateY(-motion.Relative.X * MouseSensitivity);

            // 垂直旋转（委托给 CameraController）
            var vertical = motion.Relative.Y * MouseSensitivity;
            if (InvertY) vertical = -vertical;
            _camera?.AddVerticalRotation(-vertical);
        }

        if (@event.IsActionPressed("attack") && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            if (_stamina.HasStamina(_stamina.Config.LightAttackCost))
            {
                if (!_weapon.TryCombo())
                {
                    _stamina.ConsumeAttack(false);
                    _weapon.StartAttack();
                }
            }
        }

        if (@event.IsActionPressed("lock_on"))
        {
            _lockOn?.ToggleLock();
        }

        if (@event.IsActionPressed("menu"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;

        // ── 输入方向 ──
        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        // ── 速度 ──
        var speed = Input.IsActionPressed("sprint") ? SprintSpeed : WalkSpeed;
        var targetX = direction.X * speed;
        var targetZ = direction.Z * speed;

        var accel = IsOnFloor() ? Acceleration : Acceleration * AirControl;
        _targetVelocity.X = Mathf.MoveToward(Velocity.X, targetX, accel * dt);
        _targetVelocity.Z = Mathf.MoveToward(Velocity.Z, targetZ, accel * dt);

        // ── 重力 ──
        if (!IsOnFloor())
            _targetVelocity.Y -= (float)ProjectSettings.GetSetting("physics/3d/default_gravity", 9.81) * dt;
        else if (_targetVelocity.Y < 0)
            _targetVelocity.Y = -0.01f;  // 紧贴地面

        // ── 应用 ──
        Velocity = _targetVelocity;
        MoveAndSlide();

        // ── 动画 ──
        UpdateAnimation();
    }

    // ── 动画 ──

    private void UpdateAnimation()
    {
        var vel2d = new Vector2(Velocity.X, Velocity.Z).Length();

        CharacterAnimState newState;
        if (vel2d < 0.1f)
            newState = CharacterAnimState.Idle;
        else if (Input.IsActionPressed("sprint"))
            newState = CharacterAnimState.Run;
        else
            newState = CharacterAnimState.Walk;

        if (newState != _currentAnim)
        {
            _currentAnim = newState;
            _animPlayer?.Play(CharacterAnimHelper.GetAnimName(newState));
        }
    }

    // ── 公开接口 ──

    /// <summary> 通用动画播放 </summary>
    public void PlayAnim(CharacterAnimState state)
    {
        _currentAnim = state;
        _animPlayer?.Play(CharacterAnimHelper.GetAnimName(state));
    }

    /// <summary> IDamageable: 受伤 </summary>
    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);
        _camera?.Shake(new Vector3(2f, 1f, 0.5f));

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            PlayAnim(CharacterAnimState.Death);
            // Sprint 2 实现死亡惩罚
        }
    }
}
