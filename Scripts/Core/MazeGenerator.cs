using Godot;
using HakimiAdventure.Enemy;
using HakimiAdventure.Inventory;
using HakimiAdventure.Dialogue;
using HakimiAdventure.Core;

namespace HakimiAdventure.Core;

/// <summary>
/// 迷宫关卡生成器 — 程序化创建 5x5 网格房间迷宫。
/// 所有敌人/物品/NPC 在代码中直接创建，无需额外 prefab。
/// </summary>
[GlobalClass]
public partial class MazeGenerator : Node3D
{
    private const float RoomW = 12f;
    private const float RoomD = 12f;
    private const float WallH = 3f;
    private const float DoorW = 3f;

    public override void _Ready()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        // 房间布局: 0=空, 1=房间, 2=BOSS房, 3=起点
        var layout = new int[5, 5]
        {
            { 1, 1, 2, 1, 1 },
            { 1, 0, 1, 0, 1 },
            { 1, 1, 1, 1, 1 },
            { 1, 0, 1, 0, 1 },
            { 1, 1, 3, 1, 1 },
        };

        CreateFloor();
        CreateWalls(layout);
        PopulateLevel(layout);
        CreateLighting();
    }

    private void CreateFloor()
    {
        var m = new StandardMaterial3D { AlbedoColor = new Color(0.35f, 0.35f, 0.4f), Roughness = 0.8f };
        AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(RoomW * 5, 0.3f, RoomD * 5), Material = m },
            Position = new Vector3(0, -0.15f, 0)
        });
    }

    private void CreateWalls(int[,] layout)
    {
        for (var y = 0; y < 5; y++)
            for (var x = 0; x < 5; x++)
            {
                if (layout[y, x] == 0) continue;
                var cx = (x - 2) * RoomW;
                var cz = (y - 2) * RoomD;
                var hw = RoomW / 2; var hd = RoomD / 2;

                TryWall(cx, cz - hd, RoomW, 0, y, x, layout); // N
                TryWall(cx, cz + hd, RoomW, 0, y, x, layout); // S
                TryWall(cx - hw, cz, RoomD, 1, y, x, layout); // W
                TryWall(cx + hw, cz, RoomD, 1, y, x, layout); // E
            }
    }

    private void TryWall(float x, float z, float len, int axis, int gy, int gx, int[,] l)
    {
        int ny = axis == 0 ? gy + (z < (gy - 2) * RoomD ? -1 : 1) : gy;
        int nx = axis == 1 ? gx + (x < (gx - 2) * RoomW ? -1 : 1) : gx;
        bool neighborExists = ny >= 0 && ny < 5 && nx >= 0 && nx < 5 && l[ny, nx] != 0;

        if (!neighborExists)
        {
            BuildWall(x, z, len, axis);
            return;
        }

        float seg = (len - DoorW) / 2;
        if (seg <= 0) return;
        float off = DoorW / 2 + seg / 2;
        if (axis == 0) { BuildWall(x - off, z, seg, axis); BuildWall(x + off, z, seg, axis); }
        else { BuildWall(x, z - off, seg, axis); BuildWall(x, z + off, seg, axis); }
    }

    private void BuildWall(float x, float z, float len, int axis)
    {
        var s = axis == 0 ? new Vector3(len, WallH, 0.3f) : new Vector3(0.3f, WallH, len);
        var w = new StaticBody3D { Position = new Vector3(x, WallH / 2, z) };
        w.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = s } });
        w.AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = s, Material = new StandardMaterial3D
                { AlbedoColor = new Color(0.4f, 0.4f, 0.45f), Roughness = 0.8f } }
        });
        AddChild(w);
    }

    private void PopulateLevel(int[,] l)
    {
        for (var y = 0; y < 5; y++)
            for (var x = 0; x < 5; x++)
            {
                if (l[y, x] == 0) continue;
                var cx = (x - 2) * RoomW; var cz = (y - 2) * RoomD;

                if (l[y, x] == 3) { SetPlayerSpawn(cx, cz); SpawnNpc(cx + 3, cz); SpawnPickup(cx - 3, cz, "hp"); }
                else if (l[y, x] == 2) { AddCheckpoint(cx, cz + RoomD / 2 - 2); SpawnBoss(cx, cz); SpawnSword(cx + 2, cz + 2); }
                else
                {
                    float r = GD.Randf();
                    if (r < 0.3f) SpawnEnemy(cx, cz);
                    else if (r < 0.5f) SpawnFastEnemy(cx, cz);
                    else SpawnRangedEnemy(cx, cz);
                    if (GD.Randf() < 0.3f) SpawnPickup(cx, cz, "hp");
                }
            }
    }

    private void CreateLighting()
    {
        var sun = new DirectionalLight3D { LightEnergy = 0.5f, ShadowEnabled = true };
        sun.RotationDegrees = new Vector3(45, -30, 0);
        AddChild(sun);
        AddChild(new WorldEnvironment
        {
            Environment = new Godot.Environment
            {
                AmbientLightColor = new Color(0.25f, 0.25f, 0.3f),
                AmbientLightSource = Godot.Environment.AmbientSource.Color
            }
        });
    }

    // ── 放置 ──

    private void SetPlayerSpawn(float cx, float cz)
    {
        var p = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (p != null) p.Position = new Vector3(cx, 1, cz + 4);
    }

    private void SpawnNpc(float x, float z)
    {
        var npc = new Npc
        {
            DisplayName = "向导",
            Dialogue = ResourceLoader.Load<DialogueResource>("res://Resources/Dialogues/story_intro.tres")
        };
        npc.Position = new Vector3(x, 0, z);
        npc.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.3f, Height = 1.4f } });
        npc.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(0.6f, 1.2f, 0.4f) },
            MeshOverrideMaterial = new StandardMaterial3D { AlbedoColor = new Color(0.2f, 0.5f, 0.8f) } });
        AddChild(npc);
    }

    private void SpawnEnemy(float cx, float cz)
    {
        var e = new EnemyAI { Config = new EnemyData { MaxHP = 50, MoveSpeed = 3, ChaseSpeed = 4.5f, Damage = 10, DetectionRange = 12, AttackRange = 2.5f, ExpReward = 10, GoldReward = 5 } };
        e.Position = new Vector3(cx + GD.RandRange(-3, 3), 1, cz + GD.RandRange(-3, 3));
        AddDefaultVisuals(e, new Color(0.85f, 0.15f, 0.15f));
        AddChild(e);
    }

    private void SpawnFastEnemy(float cx, float cz)
    {
        var e = new FastEnemyAI { MaxHPValue = 20, MoveSpeed = 6, Damage = 8, DetectionRange = 14, AttackRange = 2, AttackCooldown = 0.8f };
        e.Position = new Vector3(cx + GD.RandRange(-3, 3), 1, cz + GD.RandRange(-3, 3));
        AddDefaultVisuals(e, new Color(0.9f, 0.9f, 0.7f));
        AddChild(e);
    }

    private void SpawnRangedEnemy(float cx, float cz)
    {
        var e = new RangedEnemyAI { MaxHPValue = 30, MoveSpeed = 2.5f, Damage = 12, DetectionRange = 18, PreferredDist = 8, AttackCooldown = 2f };
        e.Position = new Vector3(cx + GD.RandRange(-3, 3), 1, cz + GD.RandRange(-3, 3));
        AddDefaultVisuals(e, new Color(0.4f, 0.7f, 0.3f));
        AddChild(e);
    }

    private void SpawnBoss(float cx, float cz)
    {
        var b = new BossAI { MaxHPValue = 300, MoveSpeed = 3.5f, AttackDamage = 20, SlamDamage = 30, DisplayName = "石像守卫" };
        b.Position = new Vector3(cx, 1, cz);
        AddDefaultVisuals(b, new Color(0.6f, 0.2f, 0.2f), new Vector3(1.2f, 1.8f, 0.8f));
        AddChild(b);
        var warn = new Label3D { Text = "⚔ BOSS ⚔", Position = new Vector3(cx, 3.5f, cz - 4), FontSize = 32, Modulate = new Color(1, 0.3f, 0.3f) };
        AddChild(warn);
    }

    private void SpawnPickup(float cx, float cz, string type)
    {
        var item = new PickupItem { Item = ResourceLoader.Load<ItemData>($"res://Resources/Items/potion_{type}.tres"), Count = GD.RandRange(1, 3) };
        item.Position = new Vector3(cx + GD.RandRange(-4, 4), 0.5f, cz + GD.RandRange(-4, 4));
        AddChild(item);
    }

    private void SpawnSword(float cx, float cz)
    {
        var item = new PickupItem { Item = ResourceLoader.Load<ItemData>("res://Resources/Items/iron_sword.tres"), Count = 1, AutoPickup = false };
        item.Position = new Vector3(cx, 0.5f, cz);
        AddChild(item);
    }

    private void AddCheckpoint(float cx, float cz)
    {
        var cp = new CheckPoint { CheckpointID = 1, DisplayName = "BOSS 房前" };
        cp.Position = new Vector3(cx, 1, cz);
        cp.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(4, 2, 4) } });
        AddChild(cp);
    }

    private static void AddDefaultVisuals(CharacterBody3D body, Color color, Vector3? size = null)
    {
        var s = size ?? new Vector3(0.7f, 1.2f, 0.5f);
        body.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.35f, Height = 1.4f } });
        body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = s, Material = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.6f } } });
        body.AddChild(new AnimationPlayer());
    }
}
