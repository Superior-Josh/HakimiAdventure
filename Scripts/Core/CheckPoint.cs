using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Core;

namespace HakimiAdventure.Core;

/// <summary>
/// 检查点 — Area3D 触发器，玩家进入时记录存档位置并自动保存。
/// </summary>
[GlobalClass]
public partial class CheckPoint : Area3D
{
    [Export] public int    CheckpointID  { get; set; }
    [Export] public string DisplayName   { get; set; } = "检查点";

    // ── 视觉指示（可选子节点） ──
    private MeshInstance3D? _visual;
    private bool _activated;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;

        // 查找可选的可视化子节点
        _visual = GetNodeOrNull<MeshInstance3D>("Visual");

        // 添加检查点专用碰撞层
        CollisionLayer = 0;
        CollisionMask = 1; // 只检测 layer 1（默认）
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        if (_activated) return; // 不重复激活

        _activated = true;
        var save = SaveManager.Instance;
        if (save == null) return;

        // 记录位置 + 朝向
        save.LastCheckpointPos = body.GlobalPosition;
        if (body is Player.PlayerController pc)
        {
            save.LastCheckpointYaw = pc.RotationDegrees.Y;
            save.HP   = pc.CurrentHP;
            save.MP   = pc.MP;
            save.Gold = pc.Gold;
            save.LastScene = GetTree().CurrentScene.SceneFilePath;

            // 触发保存
            save.SaveGame();
            AudioManager.Instance?.PlaySfx(SfxGenerator.CheckpointSfx());
            GD.Print($"✅ 已存档: {DisplayName} (ID={CheckpointID})");
        }

        // 视觉反馈：改变颜色闪烁
        if (_visual != null)
        {
            _visual.SetInstanceShaderParameter("emission", new Color(0, 1, 0));
        }
    }

    /// <summary> 重置检查点（用于测试） </summary>
    public void ResetCheckpoint()
    {
        _activated = false;
    }
}
