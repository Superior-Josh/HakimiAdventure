using Godot;

namespace HakimiAdventure.Core;

/// <summary>
/// 存档管理器 — 单例，ConfigFile 序列化。
/// Sprint 2 存储：位置/朝向/HP/MP/金币。
/// </summary>
[GlobalClass]
public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; } = null!;

    private const string SavePath = "user://save.cfg";
    private const string Section  = "Game";

    // ── 存档数据 ──

    public Vector3 LastCheckpointPos { get; set; } = Vector3.Zero;
    public float   LastCheckpointYaw { get; set; } = 0f;
    public float   HP                { get; set; } = 100f;
    public float   MP                { get; set; } = 50f;
    public int     Gold              { get; set; } = 0;
    public string  LastScene         { get; set; } = "";

    // ── Sprint 4 扩展 ──
    public int     Level             { get; set; } = 1;
    public int     CurrentExp        { get; set; }
    public int     StatPoints        { get; set; }
    public int     Strength          { get; set; } = 5;
    public int     Agility           { get; set; } = 5;
    public string  LearnedSpells     { get; set; } = ""; // 逗号分隔 ID
    public string  InventoryData     { get; set; } = ""; // itemID:count;...

    // ── 生命周期 ──

    public override void _EnterTree()
    {
        if (Instance != null) { QueueFree(); return; }
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    // ── 公开 API ──

    public void SaveGame()
    {
        var cfg = new ConfigFile();
        cfg.SetValue(Section, "LastCheckpointPos_X", LastCheckpointPos.X);
        cfg.SetValue(Section, "LastCheckpointPos_Y", LastCheckpointPos.Y);
        cfg.SetValue(Section, "LastCheckpointPos_Z", LastCheckpointPos.Z);
        cfg.SetValue(Section, "LastCheckpointYaw",   LastCheckpointYaw);
        cfg.SetValue(Section, "HP",  HP);
        cfg.SetValue(Section, "MP",  MP);
        cfg.SetValue(Section, "Gold", Gold);
        cfg.SetValue(Section, "LastScene", LastScene);
        cfg.SetValue(Section, "Level", Level);
        cfg.SetValue(Section, "CurrentExp", CurrentExp);
        cfg.SetValue(Section, "StatPoints", StatPoints);
        cfg.SetValue(Section, "Strength", Strength);
        cfg.SetValue(Section, "Agility", Agility);
        cfg.SetValue(Section, "LearnedSpells", LearnedSpells);
        cfg.SetValue(Section, "InventoryData", InventoryData);

        var err = cfg.Save(SavePath);
        if (err != Error.Ok)
            GD.PrintErr($"Save failed: {err}");
    }

    public void LoadGame()
    {
        var cfg = new ConfigFile();
        var err = cfg.Load(SavePath);
        if (err != Error.Ok) return;  // 无存档

        var x = (float)cfg.GetValue(Section, "LastCheckpointPos_X", 0f);
        var y = (float)cfg.GetValue(Section, "LastCheckpointPos_Y", 1f);
        var z = (float)cfg.GetValue(Section, "LastCheckpointPos_Z", 0f);
        LastCheckpointPos = new Vector3(x, y, z);
        LastCheckpointYaw = (float)cfg.GetValue(Section, "LastCheckpointYaw", 0f);
        HP   = (float)cfg.GetValue(Section, "HP",   100f);
        MP   = (float)cfg.GetValue(Section, "MP",   50f);
        Gold = (int)cfg.GetValue(Section, "Gold", 0);
        LastScene = (string)cfg.GetValue(Section, "LastScene", "");
        Level = (int)cfg.GetValue(Section, "Level", 1);
        CurrentExp = (int)cfg.GetValue(Section, "CurrentExp", 0);
        StatPoints = (int)cfg.GetValue(Section, "StatPoints", 0);
        Strength = (int)cfg.GetValue(Section, "Strength", 5);
        Agility = (int)cfg.GetValue(Section, "Agility", 5);
        LearnedSpells = (string)cfg.GetValue(Section, "LearnedSpells", "");
        InventoryData = (string)cfg.GetValue(Section, "InventoryData", "");
    }

    /// <summary> 是否有存档 </summary>
    public bool HasSaveData()
    {
        return FileAccess.FileExists(SavePath);
    }

    /// <summary> 删除存档 </summary>
    public void DeleteSave()
    {
        if (HasSaveData())
            DirAccess.RemoveAbsolute(SavePath);
    }
}
