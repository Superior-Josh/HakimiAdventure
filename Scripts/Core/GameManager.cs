namespace HakimiAdventure.Core;

/// <summary>
/// 全局游戏管理器 — 单例，负责场景切换和应用生命周期。
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    /// <summary> 当前场景路径，用于 Continue 功能 </summary>
    public string CurrentScenePath { get; private set; } = string.Empty;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary> 切换到目标场景 </summary>
    public void ChangeScene(string scenePath)
    {
        CurrentScenePath = scenePath;
        GetTree().ChangeSceneToFile(scenePath);
    }

    /// <summary> 重新加载当前场景 </summary>
    public void ReloadCurrentScene()
    {
        if (!string.IsNullOrEmpty(CurrentScenePath))
            GetTree().ChangeSceneToFile(CurrentScenePath);
    }

    /// <summary> 退出游戏 </summary>
    public static void QuitGame()
    {
        GetTree().Quit();
    }
}
