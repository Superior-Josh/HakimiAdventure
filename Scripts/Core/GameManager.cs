using Godot;

namespace HakimiAdventure;

/// <summary>
/// 游戏全局管理器 — 单例，负责游戏状态、场景切换、存档等。
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public override void _Ready()
    {
        GD.Print("哈基米大冒险 — GameManager Ready");
    }
}
