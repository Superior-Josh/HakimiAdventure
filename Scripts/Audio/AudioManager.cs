using Godot;

namespace HakimiAdventure.Audio;

/// <summary>
/// AudioBus 名称常量 — Godot 编辑器需手动创建同名 Bus。
/// 也可在代码中通过 AudioServer 动态创建。
/// </summary>
public static class AudioBusName
{
    public const string Master = "Master";
    public const string Bgm    = "BGM";
    public const string Sfx    = "SFX";
    public const string Voice  = "Voice";
}

/// <summary>
/// 音频管理器 — 单例，管理三轨 AudioBus 和全局音量。
/// </summary>
[GlobalClass]
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; } = null!;

    // ── Bus 索引缓存 ──
    private int _masterIdx = -1;
    private int _bgmIdx    = -1;
    private int _sfxIdx    = -1;
    private int _voiceIdx  = -1;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        // 尝试获取 Bus，若不存在则动态创建
        _masterIdx = FindOrCreateBus(AudioBusName.Master, -1);
        _bgmIdx    = FindOrCreateBus(AudioBusName.Bgm, _masterIdx);
        _sfxIdx    = FindOrCreateBus(AudioBusName.Sfx, _masterIdx);
        _voiceIdx  = FindOrCreateBus(AudioBusName.Voice, _masterIdx);
    }

    // ── 音量 API ──

    public float MasterVolume
    {
        get => GetBusVolume(_masterIdx);
        set => SetBusVolume(_masterIdx, value);
    }

    public float BgmVolume
    {
        get => GetBusVolume(_bgmIdx);
        set => SetBusVolume(_bgmIdx, value);
    }

    public float SfxVolume
    {
        get => GetBusVolume(_sfxIdx);
        set => SetBusVolume(_sfxIdx, value);
    }

    public float VoiceVolume
    {
        get => GetBusVolume(_voiceIdx);
        set => SetBusVolume(_voiceIdx, value);
    }

    // ── 播放便捷方法 ──

    /// <summary> 在 SFX Bus 播放一次音效 </summary>
    public void PlaySfx(AudioStream stream, float pitchScale = 1.0f)
    {
        PlayOnBus(_sfxIdx, stream, pitchScale);
    }

    /// <summary> 在 BGM Bus 播放音乐（自动停止当前 BGM） </summary>
    public void PlayBgm(AudioStream stream, float pitchScale = 1.0f)
    {
        StopBus(_bgmIdx);
        PlayOnBus(_bgmIdx, stream, pitchScale);
    }

    /// <summary> 在 Voice Bus 播放语音 </summary>
    public void PlayVoice(AudioStream stream)
    {
        PlayOnBus(_voiceIdx, stream);
    }

    // ── 内部 ──

    private static float GetBusVolume(int idx) =>
        idx >= 0 ? Mathf.DbToLinear(AudioServer.GetBusVolumeDb(idx)) : 0f;

    private static void SetBusVolume(int idx, float linear)
    {
        if (idx >= 0)
            AudioServer.SetBusVolumeDb(idx, Mathf.LinearToDb(Mathf.Clamp(linear, 0f, 1f)));
    }

    private static void PlayOnBus(int busIdx, AudioStream stream, float pitch = 1f)
    {
        if (busIdx < 0 || stream == null) return;

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            PitchScale = pitch,
            Bus = AudioServer.GetBusName(busIdx)
        };

        var tree = Instance?.GetTree();
        if (tree?.Root == null) return;
        tree.Root.AddChild(player);
        player.Play();
        player.Finished += player.QueueFree;
    }

    private static void StopBus(int idx)
    {
        if (idx < 0) return;
        // 停止该 Bus 上所有 AudioStreamPlayer
        foreach (var child in (Instance?.GetTree().Root?.GetChildren() ?? []))
        {
            if (child is AudioStreamPlayer asp && asp.Bus == AudioServer.GetBusName(idx))
                asp.Stop();
        }
    }

    /// <summary> 查找 Bus，不存在则创建 </summary>
    private static int FindOrCreateBus(string name, int parentIdx)
    {
        for (var i = 0; i < AudioServer.BusCount; i++)
            if (AudioServer.GetBusName(i) == name)
                return i;

        // 动态创建
        var idx = AudioServer.BusCount;
        AudioServer.AddBus(idx);
        AudioServer.SetBusName(idx, name);

        // 设置父 Bus
        if (parentIdx >= 0)
            AudioServer.SetBusSend(idx, AudioServer.GetBusName(parentIdx));

        // 添加一个 Reverb 效果占位（保持扩展性）
        AudioServer.AddBusEffect(idx, new AudioEffectReverb());

        return idx;
    }
}
