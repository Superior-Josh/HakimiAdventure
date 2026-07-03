using Godot;
using HakimiAdventure.Dialogue;

namespace HakimiAdventure.Core;

/// <summary>
/// NPC — 可交互角色。子节点需包含 Area3D 碰撞体作为触发区。
/// </summary>
[GlobalClass]
public partial class Npc : StaticBody3D
{
    [Export] public string DisplayName { get; set; } = "NPC";
    [Export] public DialogueResource? Dialogue { get; set; }
    [Export] public string StartNodeID { get; set; } = "start";

    /// <summary> 玩家靠近时显示的提示 </summary>
    public string InteractionHint => $"按 [F] 与 {DisplayName} 对话";

    /// <summary> 尝试与 NPC 对话 </summary>
    public void Interact()
    {
        if (Dialogue == null) return;
        var controller = GetTree().GetFirstNodeInGroup("dialogue_controller") as DialogueController;
        controller?.StartDialogue(Dialogue, StartNodeID);
    }
}
