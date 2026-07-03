using Godot;
using HakimiAdventure.Audio;
using HakimiAdventure.Core;

namespace HakimiAdventure.UI;

/// <summary>
/// 主菜单 — 程序化创建 UI。提供 New Game / Continue / Settings / Quit。
/// </summary>
[GlobalClass]
public partial class MainMenu : Node
{
    private const string TestScene = "res://Scenes/Levels/MazeLevel1.tscn";

    private Control _root = null!;
    private Panel _settingsPanel = null!;

    public override void _Ready()
    {
        _root = new Control { Name = "MenuRoot" };
        AddChild(_root);

        CreateBackground();
        CreateTitle();
        CreateButtons();
        CreateSettingsPanel();
    }

    private void CreateBackground()
    {
        // 简单纯色背景
        var bg = new ColorRect
        {
            Color = new Color(0.08f, 0.08f, 0.12f),
            Size = new Vector2(1920, 1080)
        };
        _root.AddChild(bg);

        // 标题装饰线
        var line = new ColorRect
        {
            Color = new Color(0.3f, 0.5f, 0.8f, 0.5f),
            Position = new Vector2(300, 220),
            Size = new Vector2(400, 2)
        };
        _root.AddChild(line);
    }

    private void CreateTitle()
    {
        var title = new Label
        {
            Text = "哈基米大冒险",
            Position = new Vector2(300, 120),
            Size = new Vector2(400, 100),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
        title.AddThemeFontSizeOverride("font_size", 48);
        _root.AddChild(title);

        var subtitle = new Label
        {
            Text = "Hakimi Adventure",
            Position = new Vector2(300, 170),
            Size = new Vector2(400, 40),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        subtitle.AddThemeColorOverride("font_color", new Color(0.5f, 0.7f, 1f));
        subtitle.AddThemeFontSizeOverride("font_size", 18);
        _root.AddChild(subtitle);
    }

    private void CreateButtons()
    {
        var btnData = new (string Text, string Action)[]
        {
            ("开始新游戏", "new_game"),
            ("继续游戏",   "continue"),
            ("设置",       "settings"),
            ("退出游戏",   "quit")
        };

        for (var i = 0; i < btnData.Length; i++)
        {
            var btn = new Button
            {
                Text = btnData[i].Text,
                Position = new Vector2(350, 280 + i * 60),
                Size = new Vector2(300, 44),
                Flat = false
            };
            var action = btnData[i].Action;
            btn.Pressed += () => OnButtonPressed(action);
            _root.AddChild(btn);
        }
    }

    private void CreateSettingsPanel()
    {
        _settingsPanel = new Panel
        {
            Position = new Vector2(200, 100),
            Size = new Vector2(500, 500),
            Visible = false,
            Modulate = new Color(0.1f, 0.1f, 0.15f, 0.95f)
        };
        _root.AddChild(_settingsPanel);

        var title = new Label
        {
            Text = "设置",
            Position = new Vector2(20, 20),
            Size = new Vector2(460, 40)
        };
        title.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        title.AddThemeFontSizeOverride("font_size", 28);
        _settingsPanel.AddChild(title);

        // 音量滑块
        CreateVolumeSlider(_settingsPanel, "主音量", 60, AudioManager.Instance?.MasterVolume ?? 1f,
            v => { if (AudioManager.Instance != null) AudioManager.Instance.MasterVolume = v; });
        CreateVolumeSlider(_settingsPanel, "BGM", 120, AudioManager.Instance?.BgmVolume ?? 0.7f,
            v => { if (AudioManager.Instance != null) AudioManager.Instance.BgmVolume = v; });
        CreateVolumeSlider(_settingsPanel, "SFX", 180, AudioManager.Instance?.SfxVolume ?? 0.8f,
            v => { if (AudioManager.Instance != null) AudioManager.Instance.SfxVolume = v; });
        CreateVolumeSlider(_settingsPanel, "语音", 240, AudioManager.Instance?.VoiceVolume ?? 0.7f,
            v => { if (AudioManager.Instance != null) AudioManager.Instance.VoiceVolume = v; });

        // 返回按钮
        var backBtn = new Button
        {
            Text = "返回",
            Position = new Vector2(20, 400),
            Size = new Vector2(200, 40)
        };
        backBtn.Pressed += () => _settingsPanel.Visible = false;
        _settingsPanel.AddChild(backBtn);
    }

    private static void CreateVolumeSlider(Panel parent, string label, int y, float initial, System.Action<float> onChanged)
    {
        var lbl = new Label
        {
            Text = label,
            Position = new Vector2(20, y),
            Size = new Vector2(100, 30)
        };
        lbl.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        parent.AddChild(lbl);

        var slider = new HSlider
        {
            Position = new Vector2(130, y),
            Size = new Vector2(300, 30),
            MinValue = 0f,
            MaxValue = 1f,
            Step = 0.05f,
            Value = initial
        };
        slider.ValueChanged += value => onChanged?.Invoke((float)value);
        parent.AddChild(slider);

        var valueLabel = new Label
        {
            Text = $"{initial * 100:F0}%",
            Position = new Vector2(440, y),
            Size = new Vector2(50, 30)
        };
        valueLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        parent.AddChild(valueLabel);
        slider.ValueChanged += v => valueLabel.Text = $"{v:F0}%";
    }

    private void OnButtonPressed(string action)
    {
        switch (action)
        {
            case "new_game":
                SaveManager.Instance?.DeleteSave();
                GetTree().ChangeSceneToFile(TestScene);
                break;

            case "continue":
                var save = SaveManager.Instance;
                if (save != null && save.HasSaveData())
                {
                    save.LoadGame();
                    var scene = string.IsNullOrEmpty(save.LastScene) ? TestScene : save.LastScene;
                    GetTree().ChangeSceneToFile(scene);
                }
                break;

            case "settings":
                _settingsPanel.Visible = !_settingsPanel.Visible;
                break;

            case "quit":
                GameManager.QuitGame();
                break;
        }
    }
}
