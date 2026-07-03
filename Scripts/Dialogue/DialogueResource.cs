using Godot;

namespace HakimiAdventure.Dialogue;

/// <summary> 对话选项 </summary>
[GlobalClass]
public partial class DialogueOption : Resource
{
    [Export] public string Text          { get; set; } = "";
    [Export] public string NextNodeID    { get; set; } = "";
    [Export] public string ConditionVar  { get; set; } = "";   // 条件变量名，空=无条件
    [Export] public string SetVar        { get; set; } = "";   // 选择后设置的变量
    [Export] public string SetVarValue   { get; set; } = "";
}

/// <summary> 对话节点 </summary>
[GlobalClass]
public partial class DialogueNode : Resource
{
    [Export] public string NodeID       { get; set; } = "";
    [Export] public string SpeakerName  { get; set; } = "";
    [Export] public string Text         { get; set; } = "";
    [Export] public Godot.Collections.Array<DialogueOption> Options { get; set; } = new();
}

/// <summary> 对话树资源 — 包含一组节点，从 "start" 节点开始 </summary>
[GlobalClass]
public partial class DialogueResource : Resource
{
    [Export] public Godot.Collections.Array<DialogueNode> Nodes { get; set; } = new();

    /// <summary> 按 NodeID 查找节点 </summary>
    public DialogueNode? GetNode(string id)
    {
        foreach (var n in Nodes)
            if (n.NodeID == id) return n;
        return null;
    }
}
