using Godot;

namespace HakimiAdventure.Core;

/// <summary>
/// 加载画面 — 场景切换时显示的黑色加载界面。
/// </summary>
[GlobalClass]
public partial class LoadingScreen : CanvasLayer
{
    private Label _tipLabel = null!;

    public override void _Ready()
    {
        Layer = 100;
        var bg = new ColorRect
        {
            Color = new Color(0, 0, 0),
            Size = new Vector2(1920, 1080)
        };
        AddChild(bg);

        _tipLabel = new Label
        {
            Text = "加载中…",
            Position = new Vector2(600, 500),
            Size = new Vector2(400, 40),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _tipLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        _tipLabel.AddThemeFontSizeOverride("font_size", 24);
        AddChild(_tipLabel);

        Visible = false;
    }

    /// <summary> 显示加载画面并切换场景 </summary>
    public async void LoadScene(string scenePath)
    {
        Visible = true;
        _tipLabel.Text = "加载中…";

        // 使用 ResourceLoader 异步加载
        var loader = ResourceLoader.LoadThreadedRequest(scenePath);
        if (loader != Error.Ok) { GetTree().ChangeSceneToFile(scenePath); return; }

        // 轮询进度
        while (true)
        {
            var progress = new Godot.Collections.Array();
            var status = ResourceLoader.LoadThreadedGetStatus(scenePath, progress);
            if (status == ResourceLoader.ThreadLoadStatus.InProgress)
            {
                _tipLabel.Text = $"加载中… {(float)progress[0] * 100:F0}%";
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            }
            else if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                _tipLabel.Text = "完成！";
                await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
                GetTree().ChangeSceneToFile(scenePath);
                break;
            }
            else
            {
                _tipLabel.Text = "加载失败。";
                await ToSignal(GetTree().CreateTimer(1f), "timeout");
                GetTree().ChangeSceneToFile(scenePath);
                break;
            }
        }
    }
}
