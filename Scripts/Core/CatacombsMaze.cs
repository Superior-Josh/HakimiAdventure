using Godot;
using HakimiAdventure.Enemy;
using HakimiAdventure.Inventory;
using HakimiAdventure.Dialogue;

namespace HakimiAdventure.Core;

/// <summary> 墓穴迷宫 — 紧凑黑暗主题，窄通道 + 埋伏 </summary>
[GlobalClass]
public partial class CatacombsMaze : Node3D
{
    private const float RoomW = 10f, RoomD = 10f, WallH = 3f, DoorW = 2.5f;

    public override void _Ready() => Generate();

    private void Generate()
    {
        var l = new int[5, 5]
        {
            { 1, 1, 0, 2, 1 },
            { 0, 1, 1, 1, 1 },
            { 1, 1, 0, 1, 1 },
            { 1, 0, 1, 0, 1 },
            { 1, 1, 3, 1, 1 },
        };
        BuildMaze(l, new Color(0.2f, 0.15f, 0.1f), 0.3f); // 暗棕色系
    }

    private void BuildMaze(int[,] l, Color floorColor, float lightEnergy)
    {
        for (var y = 0; y < 5; y++)
            for (var x = 0; x < 5; x++)
            {
                if (l[y, x] == 0) continue;
                var cx = (x - 2) * RoomW; var cz = (y - 2) * RoomD;
                foreach (var (dx, dz, ax) in new[] { (0f, -RoomD / 2, 0), (0f, RoomD / 2, 0), (-RoomW / 2, 0f, 1), (RoomW / 2, 0f, 1) })
                    Wall(cx + dx, cz + dz, ax == 0 ? RoomW : RoomD, ax, l, y, x);
                Content(l[y, x], cx, cz);
            }

        var m = new StandardMaterial3D { AlbedoColor = floorColor, Roughness = 0.9f };
        AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(RoomW * 5, 0.3f, RoomD * 5), Material = m }, Position = new Vector3(0, -0.15f, 0) });
        var sun = new DirectionalLight3D { LightEnergy = lightEnergy, ShadowEnabled = true };
        sun.RotationDegrees = new Vector3(60, -20, 0); AddChild(sun);
        AddChild(new WorldEnvironment { Environment = new Godot.Environment { AmbientLightColor = new Color(0.1f, 0.08f, 0.05f), AmbientLightSource = Godot.Environment.AmbientSource.Color } });
    }

    private void Wall(float x, float z, float len, int ax, int[,] l, int gy, int gx)
    {
        int ny = ax == 0 ? gy + (z < (gy - 2) * RoomD ? -1 : 1) : gy;
        int nx = ax == 1 ? gx + (x < (gx - 2) * RoomW ? -1 : 1) : gx;
        bool adj = ny >= 0 && ny < 5 && nx >= 0 && nx < 5 && l[ny, nx] != 0;
        if (!adj) { Build(x, z, len, ax); return; }
        var seg = (len - DoorW) / 2; if (seg <= 0) return;
        var off = DoorW / 2 + seg / 2;
        if (ax == 0) { Build(x - off, z, seg, 0); Build(x + off, z, seg, 0); }
        else { Build(x, z - off, seg, 1); Build(x, z + off, seg, 1); }
    }

    private void Build(float x, float z, float len, int ax)
    {
        var s = ax == 0 ? new Vector3(len, WallH, 0.3f) : new Vector3(0.3f, WallH, len);
        var w = new StaticBody3D { Position = new Vector3(x, WallH / 2, z) };
        w.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = s } });
        w.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = s, Material = new StandardMaterial3D { AlbedoColor = new Color(0.35f, 0.25f, 0.2f), Roughness = 0.9f } } });
        AddChild(w);
    }

    private void Content(int type, float cx, float cz)
    {
        if (type == 3) { PlayerSpawn(cx, cz); Pick(cx - 3, cz, "mp"); }
        else if (type == 2) { Cp(cx, cz + RoomD / 2 - 2); Spawn<VampireBossAI>(cx, cz, new VampireBossAI { MaxHPValue = 250, DisplayName = "血族伯爵" }, new Color(0.5f, 0.05f, 0.05f)); Pick(cx + 2, cz + 2, "armor"); }
        else { if (GD.Randf() < 0.4f) Spawn<EnemyAI>(cx, cz, new EnemyAI { Config = new EnemyData { MaxHP = 40, MoveSpeed = 2.5f, Damage = 12, DetectionRange = 10, AttackRange = 2.5f, ExpReward = 12, GoldReward = 6 } }, new Color(0.5f, 0.3f, 0.2f)); if (GD.Randf() < 0.3f) Pick(cx, cz, "hp"); }
    }

    private void PlayerSpawn(float cx, float cz)
    {
        var p = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (p != null) p.Position = new Vector3(cx, 1, cz + 4);
    }
    private void Cp(float cx, float cz)
    {
        var cp = new CheckPoint { CheckpointID = 2, DisplayName = "墓穴入口" };
        cp.Position = new Vector3(cx, 1, cz);
        cp.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(4, 2, 4) } });
        AddChild(cp);
    }
    private void Pick(float cx, float cz, string t)
    {
        var id = t == "hp" ? "potion_hp" : t == "mp" ? "potion_mp" : "iron_armor";
        var i = new PickupItem { Item = ResourceLoader.Load<ItemData>($"res://Resources/Items/{id}.tres"), Count = t == "armor" ? 1 : GD.RandRange(1, 2) };
        i.Position = new Vector3(cx + GD.RandRange(-4, 4), 0.5f, cz + GD.RandRange(-4, 4));
        AddChild(i);
    }
    private void Spawn<T>(float cx, float cz, CharacterBody3D body, Color color) where T : CharacterBody3D
    {
        body.Position = new Vector3(cx + GD.RandRange(-3, 3), 1, cz + GD.RandRange(-3, 3));
        body.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.35f, Height = 1.4f } });
        body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(0.7f, 1.2f, 0.5f), Material = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.6f } } });
        body.AddChild(new AnimationPlayer());
        AddChild(body);
    }
}
