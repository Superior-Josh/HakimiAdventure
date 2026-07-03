using Godot;
using HakimiAdventure.Combat;
using HakimiAdventure.Core;
using HakimiAdventure.Magic;
using HakimiAdventure.Growth;

namespace HakimiAdventure.UI;

/// <summary>
/// HUD 控制器 — HP 条 + 体力条 + 锁定指示器。
/// 所有 UI 元素在 _Ready 中程序化创建。
/// </summary>
[GlobalClass]
public partial class HUD : CanvasLayer
{
    private Player.PlayerController _player = null!;
    private StaminaManager _stamina = null!;
    private LockOnSystem _lockOn = null!;
    private MagicSystem _magic = null!;
    private ExperienceManager _exp = null!;

    // ── UI 元素 ──
    private TextureProgressBar _hpBar = null!;
    private TextureProgressBar _staminaBar = null!;
    private TextureProgressBar _mpBar = null!;
    private TextureProgressBar _expBar = null!;
    private Label _hpLabel = null!;
    private TextureRect _lockOnIndicator = null!;
    private Label _goldLabel = null!;
    private int _lastGold;
    private float _goldAnimTimer;
    private Control _hudRoot = null!;

    public override void _Ready()
    {
        _hudRoot = new Control { Name = "HUDRoot" };
        AddChild(_hudRoot);

        CreateHpBar();
        CreateStaminaBar();
        CreateMpBar();
        CreateExpBar();
        CreateLockOnIndicator();
        CreateGoldDisplay();
        CreateSkillSlots();
    }

    public override void _Process(double delta)
    {
        if (_player == null)
        {
            // 首次运行时定位玩家
            _player = GetTree().GetFirstNodeInGroup("player") as Player.PlayerController;
            if (_player == null) return;
            _stamina = _player.GetNodeOrNull<StaminaManager>("StaminaManager");
            _lockOn  = _player.GetNodeOrNull<LockOnSystem>("LockOnSystem");
        }

        if (_player == null) return;

        UpdateHpBar();
        UpdateStaminaBar();
        UpdateMpBar();
        UpdateExpBar();
        UpdateLockOnIndicator();
        UpdateGoldDisplay((float)delta);
        UpdateSkillSlots();
    }

    // ── HP 条 ──

    private void CreateHpBar()
    {
        var bg = new NinePatchRect
        {
            Name = "HPBarBg",
            Position = new Vector2(20, 20),
            Size = new Vector2(300, 28),
            Modulate = new Color(0.2f, 0.2f, 0.2f, 0.8f)
        };
        _hudRoot.AddChild(bg);

        _hpBar = new TextureProgressBar
        {
            Name = "HPBar",
            Position = new Vector2(22, 22),
            Size = new Vector2(296, 24),
            MaxValue = 100f,
            Value = 100f,
            FillMode = TextureProgressBar.FillModeEnum.LeftToRight,
            Modulate = new Color(0.9f, 0.2f, 0.2f)
        };
        _hudRoot.AddChild(_hpBar);

        _hpLabel = new Label
        {
            Name = "HPLabel",
            Position = new Vector2(22, 22),
            Size = new Vector2(296, 24),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Theme = new Theme()
        };
        // 简单的字体样式
        _hpLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));
        _hpLabel.AddThemeFontSizeOverride("font_size", 14);
        _hudRoot.AddChild(_hpLabel);
    }

    private void UpdateHpBar()
    {
        _hpBar.MaxValue = _player.MaxHP;
        _hpBar.Value = _player.CurrentHP;
        _hpLabel.Text = $"HP: {_player.CurrentHP:F0}/{_player.MaxHP:F0}";

        // 低血量变色
        var ratio = _player.CurrentHP / _player.MaxHP;
        if (ratio < 0.3f)
            _hpBar.Modulate = new Color(1f, 0.5f, 0.5f);
        else if (ratio < 0.6f)
            _hpBar.Modulate = new Color(0.95f, 0.7f, 0.3f);
        else
            _hpBar.Modulate = new Color(0.9f, 0.2f, 0.2f);
    }

    // ── 体力条 ──

    private void CreateStaminaBar()
    {
        var bg = new NinePatchRect
        {
            Name = "StaminaBarBg",
            Position = new Vector2(20, 52),
            Size = new Vector2(300, 18),
            Modulate = new Color(0.2f, 0.2f, 0.2f, 0.7f)
        };
        _hudRoot.AddChild(bg);

        _staminaBar = new TextureProgressBar
        {
            Name = "StaminaBar",
            Position = new Vector2(22, 53),
            Size = new Vector2(296, 16),
            MaxValue = 100f,
            Value = 100f,
            FillMode = TextureProgressBar.FillModeEnum.LeftToRight,
            Modulate = new Color(0.2f, 0.8f, 0.3f)
        };
        _hudRoot.AddChild(_staminaBar);
    }

    private void UpdateStaminaBar()
    {
        if (_stamina == null) return;
        _staminaBar.MaxValue = _stamina.MaxStamina;
        _staminaBar.Value = _stamina.CurrentStamina;

        // 低体力变色
        var ratio = _stamina.CurrentStamina / _stamina.MaxStamina;
        if (ratio < 0.25f)
            _staminaBar.Modulate = new Color(0.8f, 0.8f, 0.2f);
        else
            _staminaBar.Modulate = new Color(0.2f, 0.8f, 0.3f);
    }

    // ── 锁定指示器 ──

    private void CreateLockOnIndicator()
    {
        _lockOnIndicator = new TextureRect
        {
            Name = "LockOnIndicator",
            Size = new Vector2(24, 24),
            // 使用白色方块临时替代，后期替换为环形图标
            Modulate = new Color(1f, 0.8f, 0f, 0.9f),
            Visible = false
        };

        // 绘制一个简单的环形（使用 Panel 代替）
        var panel = new Panel
        {
            Name = "LockOnPanel",
            Size = new Vector2(24, 24),
            Modulate = new Color(1f, 0.8f, 0f, 0.9f),
            SelfModulate = new Color(0, 0, 0, 0) // 透明填充
        };
        // 实际上用 NinePatchRect 代替显示边框

        _lockOnIndicator.AddChild(panel);
        _hudRoot.AddChild(_lockOnIndicator);
    }

    // ── 金币 ──

    private void CreateGoldDisplay()
    {
        _goldLabel = new Label
        {
            Name = "GoldLabel",
            Position = new Vector2(1600, 20),
            Size = new Vector2(200, 36),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        _goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f));
        _goldLabel.AddThemeFontSizeOverride("font_size", 22);
        _hudRoot.AddChild(_goldLabel);

        _lastGold = -1;
    }

    private void UpdateGoldDisplay(float delta)
    {
        if (_player == null) return;

        if (_player.Gold != _lastGold)
        {
            _lastGold = _player.Gold;
            _goldAnimTimer = 0.5f;
        }

        if (_goldAnimTimer > 0)
        {
            _goldAnimTimer -= delta;
            // 缩放动画
            var scale = 1f + (_goldAnimTimer / 0.5f) * 0.3f;
            _goldLabel.Scale = new Vector2(scale, scale);
        }
        else
        {
            _goldLabel.Scale = Vector2.One;
        }

        _goldLabel.Text = $"💰 {_player.Gold}";
    }

    private void UpdateLockOnIndicator()
    {
        if (_lockOn == null) return;

        var screenPos = _lockOn.GetTargetScreenPos();
        _lockOnIndicator.Visible = screenPos.HasValue;

        if (screenPos.HasValue)
        {
            // 将指示器放在目标屏幕位置
            var pos = screenPos.Value;
            _lockOnIndicator.Position = pos - _lockOnIndicator.Size * 0.5f;
        }
    }

    // ── MP 条 ──

    private void CreateMpBar()
    {
        var bg = new NinePatchRect
        {
            Name = "MpBarBg",
            Position = new Vector2(20, 74),
            Size = new Vector2(300, 16),
            Modulate = new Color(0.2f, 0.2f, 0.2f, 0.7f)
        };
        _hudRoot.AddChild(bg);

        _mpBar = new TextureProgressBar
        {
            Name = "MpBar",
            Position = new Vector2(22, 75),
            Size = new Vector2(296, 14),
            MaxValue = 50f,
            Value = 50f,
            FillMode = TextureProgressBar.FillModeEnum.LeftToRight,
            Modulate = new Color(0.3f, 0.4f, 0.9f)
        };
        _hudRoot.AddChild(_mpBar);
    }

    private void UpdateMpBar()
    {
        if (_player == null) return;
        _mpBar.MaxValue = _player.MaxMP;
        _mpBar.Value = _player.MP;
    }

    // ── 经验条 ──

    private void CreateExpBar()
    {
        var bg = new NinePatchRect
        {
            Name = "ExpBarBg",
            Position = new Vector2(20, 94),
            Size = new Vector2(300, 12),
            Modulate = new Color(0.2f, 0.2f, 0.2f, 0.6f)
        };
        _hudRoot.AddChild(bg);

        _expBar = new TextureProgressBar
        {
            Name = "ExpBar",
            Position = new Vector2(22, 95),
            Size = new Vector2(296, 10),
            MaxValue = 1f,
            Value = 0f,
            FillMode = TextureProgressBar.FillModeEnum.LeftToRight,
            Modulate = new Color(0.6f, 0.3f, 0.9f)
        };
        _hudRoot.AddChild(_expBar);
    }

    private void UpdateExpBar()
    {
        if (_exp == null)
        {
            _exp = _player?.GetNodeOrNull<ExperienceManager>("ExperienceManager");
            return;
        }
        _expBar.MaxValue = 1f;
        _expBar.Value = _exp.ExpProgress;
    }

    // ── 技能槽 ──

    private Button[] _skillButtons = new Button[4];

    private void CreateSkillSlots()
    {
        for (var i = 0; i < 4; i++)
        {
            var btn = new Button
            {
                Position = new Vector2(1300 + i * 70, 850),
                Size = new Vector2(60, 60),
                Text = $"{i + 1}",
                Flat = false,
                Disabled = true
            };
            _hudRoot.AddChild(btn);
            _skillButtons[i] = btn;
        }
    }

    private void UpdateSkillSlots()
    {
        if (_magic == null)
        {
            _magic = _player?.GetNodeOrNull<MagicSystem>("MagicSystem");
            return;
        }

        for (var i = 0; i < 4; i++)
        {
            if (i < _magic.LearnedSpells.Count)
            {
                var spell = _magic.LearnedSpells[i];
                var cd = _magic.GetCooldownRemaining(spell);
                _skillButtons[i].Text = cd > 0
                    ? $"{i + 1}\n{cd:F1}s"
                    : $"{i + 1}\n{spell.Name}";
                _skillButtons[i].Disabled = cd > 0;
            }
        }
    }
}
