using Godot;

namespace HakimiAdventure.Inventory;

/// <summary>
/// 背包 UI — 网格视图 + 物品详情 + 使用/丢弃按钮。
/// </summary>
[GlobalClass]
public partial class InventoryUI : CanvasLayer
{
    private InventorySystem _inv = null!;
    private Player.PlayerController _player = null!;
    private bool _isOpen;

    // ── UI ──
    private Panel _bg = null!;
    private GridContainer _grid = null!;
    private RichTextLabel _detailLabel = null!;
    private Button _useBtn = null!;
    private Button _discardBtn = null!;
    private int _selectedSlot = -1;

    public override void _Ready()
    {
        Layer = 20; // 在 HUD 之上
        CreateUI();
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_player == null)
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController;
            if (_player == null) return;
            _inv = _player.GetNodeOrNull<InventorySystem>("InventorySystem");
        }

        if (Input.IsActionJustPressed("inventory"))
        {
            _isOpen = !_isOpen;
            Visible = _isOpen;
            Input.MouseMode = _isOpen ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
            if (_isOpen) RefreshGrid();
        }

        if (_isOpen)
            RefreshGrid();
    }

    private void CreateUI()
    {
        _bg = new Panel
        {
            Position = new Vector2(200, 80),
            Size = new Vector2(900, 650),
            Modulate = new Color(0.08f, 0.08f, 0.12f, 0.92f)
        };
        AddChild(_bg);

        var title = new Label
        {
            Text = "背包 (I 关闭)",
            Position = new Vector2(220, 90),
            Size = new Vector2(400, 30)
        };
        title.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        title.AddThemeFontSizeOverride("font_size", 20);
        AddChild(title);

        _grid = new GridContainer
        {
            Position = new Vector2(220, 130),
            Size = new Vector2(520, 400),
            Columns = 5
        };
        AddChild(_grid);

        _detailLabel = new RichTextLabel
        {
            Position = new Vector2(760, 130),
            Size = new Vector2(300, 300),
            BbcodeEnabled = false
        };
        _detailLabel.AddThemeColorOverride("default_color", new Color(0.9f, 0.9f, 0.9f));
        _detailLabel.AddThemeFontSizeOverride("normal_font_size", 14);
        AddChild(_detailLabel);

        _useBtn = new Button
        {
            Text = "使用",
            Position = new Vector2(760, 440),
            Size = new Vector2(130, 36),
            Disabled = true
        };
        _useBtn.Pressed += () => { if (_selectedSlot >= 0) _inv?.UseItem(_selectedSlot, _player); };
        AddChild(_useBtn);

        _discardBtn = new Button
        {
            Text = "丢弃",
            Position = new Vector2(900, 440),
            Size = new Vector2(130, 36),
            Disabled = true
        };
        _discardBtn.Pressed += () => { if (_selectedSlot >= 0) _inv?.DiscardItem(_selectedSlot, 1); };
        AddChild(_discardBtn);
    }

    private void RefreshGrid()
    {
        if (_inv == null) return;

        // 清空旧按钮
        foreach (Node child in _grid.GetChildren())
            child.QueueFree();

        for (var i = 0; i < _inv.SlotCount; i++)
        {
            var idx = i;
            var slot = _inv.Slots[i];
            var btn = new Button
            {
                Text = slot.IsEmpty ? "" : $"{slot.Data!.Name}\n×{slot.Count}",
                Size = new Vector2(100, 72),
                Disabled = slot.IsEmpty,
                TooltipText = slot.IsEmpty ? "" : slot.Data!.Name
            };
            btn.Pressed += () => SelectSlot(idx);
            _grid.AddChild(btn);
        }
    }

    private void SelectSlot(int index)
    {
        _selectedSlot = index;
        var slot = _inv?.Slots[index];
        if (slot == null || slot.Value.IsEmpty)
        {
            _detailLabel.Text = "";
            _useBtn.Disabled = true;
            _discardBtn.Disabled = true;
            return;
        }

        var d = slot.Value.Data!;
        _detailLabel.Text = $"[b]{d.Name}[/b]\n{d.Description}\n\n类型: {d.Type}\n价格: {d.BuyPrice}G\n" +
                            (d.HealHP > 0 ? $"恢复 HP: {d.HealHP}\n" : "") +
                            (d.HealMP > 0 ? $"恢复 MP: {d.HealMP}\n" : "");

        _useBtn.Disabled = d.Type != ItemType.Consumable;
        _discardBtn.Disabled = false;
    }
}
