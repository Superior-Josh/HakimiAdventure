using Godot;
using HakimiAdventure.Enemy;
using HakimiAdventure.Inventory;
using HakimiAdventure.Dialogue;

namespace HakimiAdventure.Core;

/// <summary> 冰洞迷宫 — 开阔+滑地+强敌，冷色调 </summary>
[GlobalClass]
public partial class IceCaveMaze : Node3D
{
    private const float RoomW = 14f, RoomD = 14f, WallH = 3f, DoorW = 3f;

    public override void _Ready() => Generate();

    private void Generate()
    {
        var l = new int[5, 5]
        {
            { 1, 0, 1, 0, 1 },
            { 1, 1, 1, 1, 0 },
            { 0, 1, 2, 1, 1 },
            { 1, 1, 1, 1, 1 },
            { 1, 0, 3, 0, 1 },
        };
        BuildMaze(l, new Color(0.6f, 0.65f, 0.7f), 0.5f);
    }

    private void BuildMaze(int[,] l, Color floorColor, float lightEnergy)
    {
        for (var y = 0; y < 5; y++)
            for (var x = 0; x < 5; x++)
            {
                if (l[y, x] == 0) continue;
                var cx = (x - 2) * RoomW; var cz = (y - 2) * RoomD;
                foreach (var (dx, dz, ax) in new[] { (0f, -RoomD / 2, 0), (0f, RoomD / 2, 0), (-RoomW / 2, 0f, 1), (RoomW / 2, 0f, 1) })
                    IceWall(cx + dx, cz + dz, ax == 0 ? RoomW : RoomD, ax, l, y, x);
                Content(l[y, x], cx, cz);
            }

        var m = new StandardMaterial3D { AlbedoColor = floorColor, Roughness = 0.2f, Metallic = 0.5f };
        AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(RoomW * 5, 0.3f, RoomD * 5), Material = m }, Position = new Vector3(0, -0.15f, 0) });
        var sun = new DirectionalLight3D { LightEnergy = lightEnergy, ShadowEnabled = true };
        sun.RotationDegrees = new Vector3(30, -40, 0); AddChild(sun);
        AddChild(new WorldEnvironment { Environment = new Godot.Environment { AmbientLightColor = new Color(0.3f, 0.35f, 0.45f), AmbientLightSource = Godot.Environment.AmbientSource.Color } });
    }

    private void IceWall(float x, float z, float len, int ax, int[,] l, int gy, int gx)
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
        w.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = s, Material = new StandardMaterial3D { AlbedoColor = new Color(0.5f, 0.55f, 0.6f), Roughness = 0.3f, Metallic = 0.6f } } });
        AddChild(w);
    }

    private void Content(int type, float cx, float cz)
    {
        if (type == 3) { PlayerSpawn(cx, cz); Npc(cx + 3, cz); }
        else if (type == 2) { Cp(cx, cz + RoomD / 2 - 2); IceBoss(cx, cz); Pick(cx + 2, cz + 2, "weapon"); }
        else
        {
            if (GD.Randf() < 0.5f) Spawn<FastEnemyAI>(cx, cz, new FastEnemyAI { MaxHPValue = 35, MoveSpeed = 7, Damage = 15, DetectionRange = 16, AttackRange = 2.5f, AttackCooldown = 0.6f }, new Color(0.6f, 0.7f, 0.8f));
            else Spawn<RangedEnemyAI>(cx, cz, new RangedEnemyAI { MaxHPValue = 40, MoveSpeed = 3f, Damage = 18, DetectionRange = 20, PreferredDist = 9, AttackCooldown = 1.8f }, new Color(0.3f, 0.5f, 0.7f));
            if (GD.Randf() < 0.4f) Pick(cx, cz, "mp");
        }
    }

    private void PlayerSpawn(float cx, float cz)
    {
        var p = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (p != null) p.Position = new Vector3(cx, 1, cz + 4);
    }
    private void Npc(float x, float z)
    {
        var n = new Npc { DisplayName = "冰洞探险者", Dialogue = ResourceLoader.Load<DialogueResource>("res://Resources/Dialogues/story_intro.tres") };
        n.Position = new Vector3(x, 0, z);
        n.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.3f, Height = 1.4f } });
        n.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(0.6f, 1.2f, 0.4f) }, MeshOverrideMaterial = new StandardMaterial3D { AlbedoColor = new Color(0.2f, 0.5f, 0.8f) } });
        AddChild(n);
    }
    private void Cp(float cx, float cz)
    {
        var cp = new CheckPoint { CheckpointID = 3, DisplayName = "冰洞深处" };
        cp.Position = new Vector3(cx, 1, cz);
        cp.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(4, 2, 4) } });
        AddChild(cp);
    }
    private void Pick(float cx, float cz, string t)
    {
        var id = t == "weapon" ? "iron_sword" : t == "armor" ? "iron_armor" : "potion_mp";
        var i = new PickupItem { Item = ResourceLoader.Load<ItemData>($"res://Resources/Items/{id}.tres"), Count = t == "mp" ? GD.RandRange(2, 4) : 1 };
        i.Position = new Vector3(cx + GD.RandRange(-4, 4), 0.5f, cz + GD.RandRange(-4, 4));
        AddChild(i);
    }
    private void IceBoss(float cx, float cz)
    {
        var b = new IceGolemBossAI { MaxHPValue = 400, DisplayName = "冰霜巨人" };
        b.Position = new Vector3(cx, 1, cz);
        b.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.5f, Height = 2f } });
        b.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(1.5f, 2.2f, 1f), Material = new StandardMaterial3D { AlbedoColor = new Color(0.5f, 0.6f, 0.8f), Roughness = 0.3f, Metallic = 0.5f } } });
        b.AddChild(new AnimationPlayer());
        AddChild(b);
        var w = new Label3D { Text = "⚔ BOSS ⚔", Position = new Vector3(cx, 4f, cz - 4), FontSize = 32, Modulate = new Color(0.3f, 0.6f, 1f) };
        AddChild(w);
    }
    private void Spawn<T>(float cx, float cz, CharacterBody3D body, Color color) where T : CharacterBody3D
    {
        body.Position = new Vector3(cx + GD.RandRange(-4, 4), 1, cz + GD.RandRange(-4, 4));
        body.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.35f, Height = 1.4f } });
        body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(0.7f, 1.2f, 0.5f), Material = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.6f } } });
        body.AddChild(new AnimationPlayer());
        AddChild(body);
    }
}
