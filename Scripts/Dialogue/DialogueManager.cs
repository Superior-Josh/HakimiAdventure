using Godot;
using Godot.Collections;

namespace HakimiAdventure;

/// <summary>
/// 对话管理器 — 管理 NPC 对话树，分支选项影响流程。
/// 对话数据使用 JSON 格式，便于策划配置。
/// </summary>
public partial class DialogueManager : Control
{
    [Signal] public delegate void DialogueStartedEventHandler(string npcName);
    [Signal] public delegate void DialogueEndedEventHandler();
    [Signal] public delegate void ChoiceMadeEventHandler(string choiceId);

    [Export] public float TextSpeed { get; set; } = 0.05f; // 逐字显示速度

    private RichTextLabel? _dialogueText;
    private VBoxContainer? _choicesContainer;
    private string _currentNpc = "";

    public override void _Ready()
    {
        Visible = false;
    }

    /// <summary>显示一段对话（无选项）</summary>
    public async void ShowDialogue(string npcName, string text)
    {
        _currentNpc = npcName;
        Visible = true;
        EmitSignal(SignalName.DialogueStarted, npcName);

        if (_dialogueText != null)
        {
            _dialogueText.Text = "";
            foreach (char c in text)
            {
                _dialogueText.Text += c;
                await ToSignal(GetTree().CreateTimer(TextSpeed), "timeout");
            }
        }
    }

    /// <summary>显示带选项的对话</summary>
    public void ShowChoices(string npcName, string text, Array<Dictionary> choices)
    {
        ShowDialogue(npcName, text);

        if (_choicesContainer == null) return;
        // 清除旧选项
        foreach (Node child in _choicesContainer.GetChildren())
            child.QueueFree();

        foreach (var choice in choices)
        {
            var btn = new Button();
            btn.Text = choice["text"].AsString();
            string choiceId = choice["id"].AsString();
            btn.Pressed += () =>
            {
                EmitSignal(SignalName.ChoiceMade, choiceId);
                HideDialogue();
            };
            _choicesContainer.AddChild(btn);
        }
    }

    public void HideDialogue()
    {
        Visible = false;
        EmitSignal(SignalName.DialogueEnded);
    }
}
