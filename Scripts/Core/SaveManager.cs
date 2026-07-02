using Godot;
using Godot.Collections;

namespace HakimiAdventure;

/// <summary>
/// 存档管理器 — 检查点存档、死亡惩罚（回到上一个检查点）。
/// </summary>
public partial class SaveManager : Node
{
    [Signal] public delegate void GameSavedEventHandler(string checkpointName);
    [Signal] public delegate void GameLoadedEventHandler(string checkpointName);

    private const string SavePath = "user://save_game.cfg";
    private ConfigFile _config = new();

    public string CurrentCheckpoint { get; private set; } = "start";

    /// <summary>在检查点存档</summary>
    public void SaveAtCheckpoint(string checkpointName, Vector3 position, Vector3 rotation,
        CombatStats playerStats, int gold, Dictionary<string, int> inventory)
    {
        _config.SetValue("meta", "checkpoint", checkpointName);
        _config.SetValue("player", "pos_x", position.X);
        _config.SetValue("player", "pos_y", position.Y);
        _config.SetValue("player", "pos_z", position.Z);
        _config.SetValue("player", "rot_y", rotation.Y);
        _config.SetValue("player", "hp", playerStats.CurrentHP);
        _config.SetValue("player", "mp", playerStats.CurrentMP);
        _config.SetValue("player", "gold", gold);

        // 保存物品
        var itemKeys = new Array<string>();
        var itemValues = new Array<int>();
        foreach (var kv in inventory)
        {
            itemKeys.Add(kv.Key);
            itemValues.Add(kv.Value);
        }
        _config.SetValue("inventory", "keys", itemKeys);
        _config.SetValue("inventory", "values", itemValues);

        _config.Save(SavePath);
        CurrentCheckpoint = checkpointName;
        EmitSignal(SignalName.GameSaved, checkpointName);
        GD.Print($"[SaveManager] 已存档: {checkpointName}");
    }

    /// <summary>死亡时加载最近检查点</summary>
    public bool LoadLastCheckpoint(out Vector3 position, out Vector3 rotation,
        out int hp, out int mp, out int gold)
    {
        position = Vector3.Zero;
        rotation = Vector3.Zero;
        hp = 1;
        mp = 0;
        gold = 0;

        var err = _config.Load(SavePath);
        if (err != Error.Ok)
        {
            GD.Print("[SaveManager] 未找到存档");
            return false;
        }

        CurrentCheckpoint = (string)_config.GetValue("meta", "checkpoint", "start");
        position = new Vector3(
            (float)_config.GetValue("player", "pos_x", 0.0),
            (float)_config.GetValue("player", "pos_y", 0.0),
            (float)_config.GetValue("player", "pos_z", 0.0));
        rotation = new Vector3(0, (float)_config.GetValue("player", "rot_y", 0.0), 0);
        hp = (int)_config.GetValue("player", "hp", 1);
        mp = (int)_config.GetValue("player", "mp", 0);
        gold = (int)_config.GetValue("player", "gold", 0);

        EmitSignal(SignalName.GameLoaded, CurrentCheckpoint);
        GD.Print($"[SaveManager] 读档: {CurrentCheckpoint}");
        return true;
    }

    /// <summary>死亡惩罚：扣减金币/经验后回到检查点</summary>
    public void ApplyDeathPenalty(ref int gold, float penaltyRatio = 0.1f)
    {
        int lost = Mathf.CeilToInt(gold * penaltyRatio);
        gold -= lost;
        GD.Print($"[SaveManager] 死亡惩罚: 损失 {lost} 金币");
    }
}
