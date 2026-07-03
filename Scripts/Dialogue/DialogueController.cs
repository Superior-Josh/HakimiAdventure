using Godot;
using System.Collections.Generic;

namespace HakimiAdventure.Dialogue;

/// <summary>
/// 对话控制器 — 管理对话流程、打字机效果、状态变量。
/// </summary>
[GlobalClass]
public partial class DialogueController : Node
{
    // ── 事件 ──
    [Signal] public delegate void DialogueStartedEventHandler(string speakerName, string text);
    [Signal] public delegate void OptionsReadyEventHandler(Godot.Collections.Array<string> options);
    [Signal] public delegate void DialogueEndedEventHandler();

    // ── 状态 ──

    public bool IsActive { get; private set; }
    private DialogueResource? _resource;
    private DialogueNode? _currentNode;
    private Dictionary<string, string> _variables = new();

    private DialogueUI _ui = null!;

    public override void _Ready()
    {
        _ui = GetNodeOrNull<DialogueUI>("../HUD/DialogueUI");
        if (_ui == null)
        {
            // 自动创建 UI
            var hud = GetTree().GetFirstNodeInGroup("hud") as Node;
            if (hud != null)
            {
                _ui = new DialogueUI();
                hud.AddChild(_ui);
            }
        }
    }

    /// <summary> 开始对话 </summary>
    public void StartDialogue(DialogueResource resource, string startNodeID = "start")
    {
        if (IsActive) return;

        _resource = resource;
        IsActive = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GoToNode(startNodeID);
    }

    /// <summary> 选择选项 </summary>
    public void SelectOption(int index)
    {
        if (!IsActive || _currentNode == null) return;
        if (index < 0 || index >= _currentNode.Options.Count) return;

        var opt = _currentNode.Options[index];

        // 记录变量
        if (!string.IsNullOrEmpty(opt.SetVar))
            _variables[opt.SetVar] = opt.SetVarValue;

        if (!string.IsNullOrEmpty(opt.NextNodeID))
            GoToNode(opt.NextNodeID);
        else
            EndDialogue();
    }

    /// <summary> 强制结束对话 </summary>
    public void EndDialogue()
    {
        IsActive = false;
        _currentNode = null;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        EmitSignal(SignalName.DialogueEnded);
    }

    /// <summary> 获取变量值（用于条件判断） </summary>
    public string GetVariable(string name) =>
        _variables.TryGetValue(name, out var val) ? val : "";

    /// <summary> 是否激活中 </summary>
    public bool IsDialogueActive() => IsActive;

    // ── 内部 ──

    private void GoToNode(string nodeID)
    {
        if (_resource == null) { EndDialogue(); return; }

        _currentNode = _resource.GetNode(nodeID);
        if (_currentNode == null) { EndDialogue(); return; }

        // 过滤有条件但不满意的选项
        var visibleOptions = new Godot.Collections.Array<string>();
        foreach (var opt in _currentNode.Options)
        {
            if (!string.IsNullOrEmpty(opt.ConditionVar) &&
                GetVariable(opt.ConditionVar) != "true")
                continue;
            visibleOptions.Add(opt.Text);
        }

        EmitSignal(SignalName.DialogueStarted, _currentNode.SpeakerName, _currentNode.Text);
        if (visibleOptions.Count > 0)
            EmitSignal(SignalName.OptionsReady, visibleOptions);
    }
}
