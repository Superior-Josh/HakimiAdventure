using Godot;
using HakimiAdventure.Core;
using HakimiAdventure.Player;

namespace HakimiAdventure.Growth;

/// <summary>
/// 升级界面 — 分配属性点（HP/MP/力量/敏捷）。
/// </summary>
[GlobalClass]
public partial class LevelUpController : CanvasLayer
{
    private ExperienceManager _exp = null!;
    private AttributeSystem _attr = null!;
    private PlayerController _player = null!;
    private bool _isOpen;

    // ── UI ──
    private Panel _bg = null!;
    private Label _title = null!;
    private Label _pointsLabel = null!;
    private Button _strBtn = null!;
    private Button _agiBtn = null!;
    private Button _closeBtn = null!;

    public override void _Ready()
    {
        Layer = 30;
        CreateUI();
        Visible = false;

        // 监听升级事件
        var timer = new Timer { OneShot = true, WaitTime = 0.5f };
        timer.Timeout += () =>
        {
            if (_exp != null)
                _exp.LeveledUp += OnLeveledUp;
        };
        AddChild(timer);
        timer.Start();
    }

    public override void _Process(double delta)
    {
        if (_player == null)
        {
            _player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
            if (_player == null) return;
            _exp  = _player.GetNodeOrNull<ExperienceManager>("ExperienceManager");
            _attr = _player.GetNodeOrNull<AttributeSystem>("AttributeSystem");
        }

        if (_isOpen) RefreshInfo();
    }

    private void CreateUI()
    {
        _bg = new Panel
        {
            Position = new Vector2(500, 200),
            Size = new Vector2(400, 350),
            Modulate = new Color(0.08f, 0.08f, 0.12f, 0.95f)
        };
        AddChild(_bg);

        _title = new Label
        {
            Text = "升级！",
            Position = new Vector2(20, 20),
            Size = new Vector2(360, 40)
        };
        _title.AddThemeColorOverride("font_color", new Color(1, 1, 0.6f));
        _title.AddThemeFontSizeOverride("font_size", 26);
        _bg.AddChild(_title);

        _pointsLabel = new Label
        {
            Position = new Vector2(20, 70),
            Size = new Vector2(360, 30)
        };
        _pointsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 1f));
        _pointsLabel.AddThemeFontSizeOverride("font_size", 18);
        _bg.AddChild(_pointsLabel);

        var y = 120;
        _strBtn = CreateStatButton("力量", y, IncreaseStr);
        _agiBtn = CreateStatButton("敏捷", y + 60, IncreaseAgi);

        _closeBtn = new Button
        {
            Text = "关闭",
            Position = new Vector2(20, 260),
            Size = new Vector2(200, 40)
        };
        _closeBtn.Pressed += () => { _isOpen = false; Visible = false; };
        _bg.AddChild(_closeBtn);
    }

    private Button CreateStatButton(string name, int y, System.Action onClick)
    {
        var panel = new Panel
        {
            Position = new Vector2(20, y),
            Size = new Vector2(360, 50),
            Modulate = new Color(0.2f, 0.2f, 0.3f, 0.8f)
        };
        _bg.AddChild(panel);

        var label = new Label
        {
            Text = name,
            Position = new Vector2(10, 10),
            Size = new Vector2(200, 30)
        };
        label.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        panel.AddChild(label);

        var btn = new Button
        {
            Text = "+",
            Position = new Vector2(300, 5),
            Size = new Vector2(40, 40)
        };
        btn.Pressed += onClick;
        panel.AddChild(btn);

        return btn;
    }

    private void IncreaseStr()
    {
        if (_exp == null || _attr == null) return;
        if (!_exp.SpendPoint()) return;
        _attr.Strength++;
        _attr.ApplyStats();
    }

    private void IncreaseAgi()
    {
        if (_exp == null || _attr == null) return;
        if (!_exp.SpendPoint()) return;
        _attr.Agility++;
        _attr.ApplyStats();
    }

    private void OnLeveledUp(int newLevel)
    {
        _isOpen = true;
        Visible = true;
        _title.Text = $"升级！等级 {newLevel}";
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void RefreshInfo()
    {
        if (_exp == null || _attr == null) return;
        _pointsLabel.Text = $"可用属性点: {_exp.StatPoints}  |  等级 {_exp.Level}";
        _strBtn.Text = $"+ (力量: {_attr.Strength})";
        _agiBtn.Text = $"+ (敏捷: {_attr.Agility})";
        _closeBtn.Disabled = _exp.StatPoints > 0; // 必须消耗完点数才能关闭
    }
}
