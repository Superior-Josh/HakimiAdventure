using Godot;
using HakimiAdventure.Audio;
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
    [Export] public float MaxMP  { get; set; } = 50f;
    public        float CurrentHP { get; private set; }
    public        float MP        { get; set; }
    public        int   Gold      { get; set; }

    [ExportCategory("Mouse")]
    [Export] public float MouseSensitivity { get; set; } = 0.002f;
    [Export] public bool  InvertY         { get; set; } = false;

    // ── 内部状态 ──

    private CameraController _camera = null!;
    private AnimationPlayer  _animPlayer = null!;
    private StaminaManager   _stamina = null!;
    private WeaponController _weapon = null!;
    private LockOnSystem     _lockOn = null!;
    private SaveManager      _save = null!;
    private float _footstepTimer;
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
        MP = MaxMP;
        Gold = 0;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // 尝试读取存档恢复数据
        _save = SaveManager.Instance;
        if (_save != null && _save.HasSaveData())
        {
            _save.LoadGame();
            CurrentHP = _save.HP;
            MP = _save.MP;
            Gold = _save.Gold;
            GlobalPosition = _save.LastCheckpointPos;
            RotationDegrees = new Vector3(0, _save.LastCheckpointYaw, 0);
        }
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

        // ── 脚步声 ──
        UpdateFootstep(dt);

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

    /// <summary> 脚步声 </summary>
    private void UpdateFootstep(float dt)
    {
        if (!IsOnFloor()) return;
        var vel2d = new Vector2(Velocity.X, Velocity.Z).Length();
        if (vel2d < 0.1f) { _footstepTimer = 0; return; }

        var interval = Input.IsActionPressed("sprint") ? 0.35f : 0.5f;
        _footstepTimer -= dt;
        if (_footstepTimer <= 0)
        {
            _footstepTimer = interval;
            AudioManager.Instance?.PlaySfx(SfxGenerator.FootstepSfx());
        }
    }

    /// <summary> IDamageable: 受伤 </summary>
    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        PlayAnim(CharacterAnimState.Hit);
        _camera?.Shake(new Vector3(2f, 1f, 0.5f));
        AudioManager.Instance?.PlaySfx(SfxGenerator.HitSfx());

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            PlayAnim(CharacterAnimState.Death);
            HandleDeath();
        }
    }

    /// <summary> 死亡惩罚：扣 10% 金币 → 重生到检查点 </summary>
    private void HandleDeath()
    {
        Gold = Mathf.FloorToInt(Gold * 0.9f);
        AudioManager.Instance?.PlaySfx(SfxGenerator.DeathSfx());
        // 扣 10% 金币
        Gold = Mathf.FloorToInt(Gold * 0.9f);

        // 延迟重生
        var timer = new Timer { OneShot = true, WaitTime = 1.5f };
        timer.Timeout += RespawnAtCheckpoint;
        AddChild(timer);
        timer.Start();
    }

    private void RespawnAtCheckpoint()
    {
        if (_save == null) { GetTree().ReloadCurrentScene(); return; }

        _save.LoadGame();
        CurrentHP = MaxHP;
        MP = MaxMP;
        GlobalPosition = _save.LastCheckpointPos;
        RotationDegrees = new Vector3(0, _save.LastCheckpointYaw, 0);
        PlayAnim(CharacterAnimState.Idle);
    }
}
