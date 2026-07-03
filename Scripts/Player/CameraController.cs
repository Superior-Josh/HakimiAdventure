using Godot;

namespace HakimiAdventure.Player;

/// <summary>
/// 相机控制器 — FOV 动态调节、头部晃动 (head bob)、受击屏幕抖动。
/// 必须为 PlayerController 的子节点。
/// </summary>
[GlobalClass]
public partial class CameraController : Camera3D
{
    // ── 导出参数 ──

    [ExportCategory("FOV")]
    [Export] public float DefaultFov  { get; set; } = 75.0f;
    [Export] public float SprintFov   { get; set; } = 80.0f;
    [Export] public float FovLerpSpeed { get; set; } = 8.0f;

    [ExportCategory("Head Bob")]
    [Export] public float BobFrequency  { get; set; } = 10.0f;
    [Export] public float BobAmplitude  { get; set; } = 0.005f;
    [Export] public float BobSprintMult { get; set; } = 1.5f;

    [ExportCategory("Shake")]
    [Export] public float ShakeDecay    { get; set; } = 5.0f;

    // ── 内部状态 ──

    private PlayerController _player = null!;
    private float _verticalRotation;        // 俯仰角度累加
    private float _bobTimer;
    private Vector3 _bobOffset;
    private Vector3 _shakeOffset;
    private Vector3 _shakeStrength;

    // ── 生命周期 ──

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();
        Fov = DefaultFov;
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        // 1. FOV 动态调整
        var targetFov = Input.IsActionPressed("sprint") ? SprintFov : DefaultFov;
        Fov = Mathf.Lerp(Fov, targetFov, FovLerpSpeed * dt);

        // 2. Head Bob
        UpdateHeadBob(dt);

        // 3. 抖动衰减
        _shakeStrength = _shakeStrength.MoveToward(Vector3.Zero, ShakeDecay * dt);
        _shakeOffset = new Vector3(
            _shakeStrength.X * (float)GD.RandRange(-1, 1),
            _shakeStrength.Y * (float)GD.RandRange(-1, 1),
            _shakeStrength.Z * (float)GD.RandRange(-1, 1)
        );

        // 4. 合成偏移
        Transform3D t = Transform;
        t.Origin = _bobOffset + _shakeOffset;
        Transform = t;
    }

    // ── Head Bob ──

    private void UpdateHeadBob(float delta)
    {
        var velocity2d = new Vector2(_player.Velocity.X, _player.Velocity.Z).Length();

        if (velocity2d > 0.1f && _player.IsOnFloor())
        {
            var mult = Input.IsActionPressed("sprint") ? BobSprintMult : 1.0f;
            _bobTimer += velocity2d * BobFrequency * mult * delta;

            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer) * BobAmplitude * mult,
                Mathf.Sin(_bobTimer * 2.0f) * BobAmplitude * 0.5f * mult,
                0
            );
        }
        else
        {
            _bobTimer = 0;
            _bobOffset = _bobOffset.MoveToward(Vector3.Zero, 10f * delta);
        }
    }

    // ── 公开 API ──

    /// <summary> 俯仰旋转（由 PlayerController 调用） </summary>
    public void AddVerticalRotation(float amount)
    {
        _verticalRotation = Mathf.Clamp(_verticalRotation + amount, -90f, 90f);
        RotationDegrees = new Vector3(_verticalRotation, RotationDegrees.Y, RotationDegrees.Z);
    }

    /// <summary> 触发受击抖动 </summary>
    public void Shake(Vector3 intensity)
    {
        _shakeStrength = intensity;
    }

    /// <summary> 触发受击闪红（需要在场景中添加 ColorRect 覆盖层） </summary>
    public void FlashDamage()
    {
        // 委托给 UIManager 或直接操作覆盖层
        // 这里预留接口，Sprint 2 实现 VFX 时完善
    }
}
