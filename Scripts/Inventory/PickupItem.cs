using Godot;
using HakimiAdventure.Audio;

namespace HakimiAdventure.Inventory;

/// <summary>
/// 可拾取物品 — Area3D，玩家靠近后自动拾取或按 F 拾取。
/// </summary>
[GlobalClass]
public partial class PickupItem : Area3D
{
    [Export] public ItemData? Item       { get; set; }
    [Export] public int       Count      { get; set; } = 1;
    [Export] public bool      AutoPickup { get; set; } = true;

    private Vector3 _floatOffset;

    public override void _Ready()
    {
        _floatOffset = Position;
        BodyEntered += OnBodyEntered;

        // 如果没有自定义 Mesh，创建默认的
        SetupVisual();
    }

    public override void _Process(double delta)
    {
        // 浮动动画
        var pos = _floatOffset;
        pos.Y += Mathf.Sin(Time.GetTicksUsec() / 1000000f * 2f) * 0.1f;
        Position = pos;
        RotateY((float)delta * 1.5f);
    }

    private void SetupVisual()
    {
        if (GetChildCount() > 0) return; // 已有子节点

        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(0.2f, 0.2f, 0.2f) },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(1, 0.85f, 0.2f),
                EmissionEnabled = true,
                Emission = new Color(1, 0.85f, 0.2f),
                EmissionEnergyMultiplier = 0.3f
            }
        };
        AddChild(mesh);

        var light = new OmniLight3D
        {
            LightEnergy = 0.5f,
            OmniRange = 2f,
            LightColor = new Color(1, 0.85f, 0.2f)
        };
        AddChild(light);
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player") || Item == null) return;
        if (!AutoPickup) return;

        Pickup(body);
    }

    /// <summary> 拾取逻辑 </summary>
    public void Pickup(Node3D player)
    {
        var inv = player.GetNodeOrNull<InventorySystem>("InventorySystem");
        if (inv == null) return;

        var remaining = inv.PickupItem(Item, Count);
        if (remaining < Count) // 至少拾取了部分
        {
            AudioManager.Instance?.PlaySfx(SfxGenerator.PickupSfx());
            QueueFree();
        }
    }
}
