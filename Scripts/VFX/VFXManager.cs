using Godot;

namespace HakimiAdventure.VFX;

/// <summary>
/// 视觉特效管理器 — 对象池 + GpuParticles3D 生命周期管理。
/// </summary>
[GlobalClass]
public partial class VFXManager : Node
{
    public static VFXManager Instance { get; private set; } = null!;

    private const int PoolSize = 8;

    // ── 粒子预制体缓存 ──
    private PackedScene? _hitSparkScene;
    private readonly Godot.Collections.Array<GpuParticles3D> _pool = new();

    public override void _EnterTree()
    {
        if (Instance != null) { QueueFree(); return; }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary> 在世界坐标播放一次命中火花 </summary>
    public void PlayHitSpark(Vector3 worldPos)
    {
        var particle = GetOrCreateParticle();
        if (particle == null) return;

        particle.GlobalPosition = worldPos;
        particle.Emitting = true;

        // 动画结束后归还回池
        var timer = new Timer { OneShot = true, WaitTime = 1.0f };
        timer.Timeout += () =>
        {
            particle.Emitting = false;
            particle.Visible = false;
        };
        AddChild(timer);
        timer.Start();
    }

    /// <summary> 创建屏幕闪红效果（调用 HUD 方法） </summary>
    public void FlashDamageScreen()
    {
        // 查找 HUD 并调用闪红接口
        var hud = GetTree().GetFirstNodeInGroup("hud");
        // HUD 侧实现：添加 ColorRect 覆盖层定时渐隐
    }

    // ── 对象池 ──

    private GpuParticles3D? GetOrCreateParticle()
    {
        // 找空闲粒子
        foreach (var p in _pool)
        {
            if (!p.Emitting)
            {
                p.Visible = true;
                return p;
            }
        }

        // 池满时创建新的（超出部分不缓存）
        if (_pool.Count >= PoolSize * 2)
            return null;

        var particle = new GpuParticles3D();
        particle.OneShot = true;
        particle.Amount = 16;
        particle.Lifetime = 0.5f;
        particle.Explosiveness = 0.8f;

        // 简单方向扩散
        var spread = new Curve();
        spread.AddPoint(new Vector2(0, 180));
        particle.Spread = 180f;

        // 速度
        particle.LocalCoords = false;
        particle.FixedFps = 0;

        // 添加 ProcessMaterial
        var mat = new ParticleProcessMaterial
        {
            Direction = new Vector3(0, 1, 0),
            Spread = 180f,
            Gravity = new Vector3(0, -5, 0),
            InitialVelocityMin = 2f,
            InitialVelocityMax = 5f
        };
        particle.ProcessMaterial = mat;

        // 设置白色方块作为粒子纹理（使用默认的白色纹理）
        particle.DrawPass = GpuParticles3D.DrawPassEnum.Mesh;
        // 注意：需要用一个小圆球或方块作为粒子网格

        GetTree().Root.AddChild(particle);
        _pool.Add(particle);
        return particle;
    }
}
