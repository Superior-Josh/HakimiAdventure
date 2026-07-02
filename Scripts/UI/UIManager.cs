using Godot;

namespace HakimiAdventure;

/// <summary>
/// UI 管理器 — HUD（血量/法力/小地图提示）、战斗菜单、对话框容器。
/// </summary>
public partial class UIManager : Control
{
    [Export] public Color DangerColor { get; set; } = new(1, 0.2f, 0.2f);

    // HUD 元素引用（在编辑器中拖拽绑定）
    private Label? _hpLabel;
    private Label? _mpLabel;
    private Label? _checkpointLabel;
    private ProgressBar? _hpBar;
    private ProgressBar? _mpBar;

    // 战斗菜单
    private VBoxContainer? _combatMenu;

    public override void _Ready()
    {
        // 尝试获取子节点（如果场景中已配置好）
        _hpLabel = GetNodeOrNull<Label>("HUD/HP/Value");
        _mpLabel = GetNodeOrNull<Label>("HUD/MP/Value");
        _hpBar = GetNodeOrNull<ProgressBar>("HUD/HP/Bar");
        _mpBar = GetNodeOrNull<ProgressBar>("HUD/MP/Bar");
        _checkpointLabel = GetNodeOrNull<Label>("HUD/Checkpoint");
        _combatMenu = GetNodeOrNull<VBoxContainer>("CombatMenu");

        if (_combatMenu != null)
            _combatMenu.Visible = false;
    }

    /// <summary>更新玩家 HUD</summary>
    public void UpdateHUD(CombatStats stats, string checkpoint = "")
    {
        if (_hpLabel != null) _hpLabel.Text = $"{stats.CurrentHP} / {stats.MaxHP}";
        if (_mpLabel != null) _mpLabel.Text = $"{stats.CurrentMP} / {stats.MaxMP}";

        if (_hpBar != null)
        {
            _hpBar.MaxValue = stats.MaxHP;
            _hpBar.Value = stats.CurrentHP;
            if ((float)stats.CurrentHP / stats.MaxHP < 0.3f)
                _hpBar.Modulate = DangerColor;
            else
                _hpBar.Modulate = Colors.White;
        }

        if (_mpBar != null)
        {
            _mpBar.MaxValue = stats.MaxMP;
            _mpBar.Value = stats.CurrentMP;
        }

        if (_checkpointLabel != null && !string.IsNullOrEmpty(checkpoint))
            _checkpointLabel.Text = $"检查点: {checkpoint}";
    }

    /// <summary>显示战斗菜单</summary>
    public void ShowCombatMenu()
    {
        if (_combatMenu != null)
            _combatMenu.Visible = true;
    }

    /// <summary>隐藏战斗菜单</summary>
    public void HideCombatMenu()
    {
        if (_combatMenu != null)
            _combatMenu.Visible = false;
    }

    /// <summary>显示提示信息（如"存档成功"）</summary>
    public async void ShowMessage(string text, float duration = 2.0f)
    {
        var msgLabel = new Label();
        msgLabel.Text = text;
        msgLabel.HorizontalAlignment = HorizontalAlignment.Center;
        msgLabel.AnchorLeft = 0.3f;
        msgLabel.AnchorRight = 0.7f;
        msgLabel.AnchorTop = 0.1f;
        AddChild(msgLabel);

        await ToSignal(GetTree().CreateTimer(duration), "timeout");
        msgLabel.QueueFree();
    }
}
