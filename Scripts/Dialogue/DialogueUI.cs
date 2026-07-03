using Godot;

namespace HakimiAdventure.Dialogue;

/// <summary>
/// 对话 UI — 打字机效果 + 选项按钮。程序化创建。
/// </summary>
[GlobalClass]
public partial class DialogueUI : CanvasLayer
{
    private DialogueController _controller = null!;

    // ── UI 元素 ──
    private Panel _bg = null!;
    private Label _speakerLabel = null!;
    private RichTextLabel _textLabel = null!;
    private VBoxContainer _optionsContainer = null!;
    private Timer _typeTimer;
    private string _fullText = "";
    private int _charIndex;

    public override void _Ready()
    {
        Layer = 10;
        _controller = GetTree().GetFirstNodeInGroup("dialogue_controller") as DialogueController
                      ?? throw new System.Exception("DialogueUI: 未找到 DialogueController");

        _controller.DialogueStarted += OnDialogueStarted;
        _controller.OptionsReady += OnOptionsReady;
        _controller.DialogueEnded += OnDialogueEnded;

        CreateUI();
        Visible = false;
    }

    private void CreateUI()
    {
        // 半透明背景（底部 1/3 区域）
        _bg = new Panel
        {
            Name = "DialogueBg",
            Position = new Vector2(0, 540),
            Size = new Vector2(1920, 180),
            Modulate = new Color(0.05f, 0.05f, 0.1f, 0.85f)
        };
        AddChild(_bg);

        // 说话人
        _speakerLabel = new Label
        {
            Name = "SpeakerLabel",
            Position = new Vector2(40, 550),
            Size = new Vector2(500, 28)
        };
        _speakerLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
        _speakerLabel.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_speakerLabel);

        // 对话文本（RichTextLabel 支持 BBCode）
        _textLabel = new RichTextLabel
        {
            Name = "DialogueText",
            Position = new Vector2(40, 580),
            Size = new Vector2(1500, 80),
            BbcodeEnabled = false
        };
        _textLabel.AddThemeColorOverride("default_color", new Color(1, 1, 1));
        _textLabel.AddThemeFontSizeOverride("normal_font_size", 16);
        AddChild(_textLabel);

        // 选项容器
        _optionsContainer = new VBoxContainer
        {
            Name = "OptionsContainer",
            Position = new Vector2(40, 660),
            Size = new Vector2(1500, 0),
            Visible = false
        };
        AddChild(_optionsContainer);

        // 打字机计时器
        _typeTimer = new Timer { OneShot = false, WaitTime = 0.03f };
        _typeTimer.Timeout += OnTypeTick;
        AddChild(_typeTimer);
    }

    private void OnDialogueStarted(string speaker, string text)
    {
        Visible = true;
        _speakerLabel.Text = speaker;
        _fullText = text;
        _charIndex = 0;
        _textLabel.Text = "";
        _typeTimer.Start();
        _optionsContainer.Visible = false;
    }

    private void OnTypeTick()
    {
        if (_charIndex < _fullText.Length)
        {
            _textLabel.Text += _fullText[_charIndex];
            _charIndex++;
        }
        else
        {
            _typeTimer.Stop();
        }
    }

    private void OnOptionsReady(Godot.Collections.Array<string> options)
    {
        _typeTimer.Stop();
        _textLabel.Text = _fullText;

        // 清空旧选项
        foreach (Node child in _optionsContainer.GetChildren())
            child.QueueFree();

        for (var i = 0; i < options.Count; i++)
        {
            var idx = i;
            var btn = new Button
            {
                Text = $"[{i + 1}] {options[i]}",
                Size = new Vector2(1500, 30),
                Flat = false
            };
            btn.Pressed += () => _controller.SelectOption(idx);
            _optionsContainer.AddChild(btn);
        }

        _optionsContainer.Visible = options.Count > 0;
    }

    private void OnDialogueEnded()
    {
        Visible = false;
        _typeTimer.Stop();
        _optionsContainer.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        // 打字中点击左键或按 F 加速完成
        if (_typeTimer.IsStopped()) return;

        if (@event.IsActionPressed("interact") || @event.IsActionPressed("attack"))
        {
            _typeTimer.Stop();
            _textLabel.Text = _fullText;
        }
    }
}
