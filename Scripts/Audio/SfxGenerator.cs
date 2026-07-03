using Godot;

namespace HakimiAdventure.Audio;

/// <summary>
/// 音效库 — 生成占位音效（纯正弦波），待替换为真实音效文件。
/// 在 Godot 编辑器中导入音频文件后，替换为 AudioStreamMP3 引用。
/// </summary>
public static class SfxGenerator
{
    /// <summary> 生成一个短的占位音效 </summary>
    public static AudioStreamWav CreateTone(float frequency = 440f, float duration = 0.15f, float volume = 0.3f)
    {
        var sampleRate = 22050;
        var sampleCount = (int)(sampleRate * duration);
        var data = new byte[sampleCount * 2]; // 16-bit mono

        for (var i = 0; i < sampleCount; i++)
        {
            var t = (float)i / sampleRate;
            var value = Mathf.Sin(t * frequency * Mathf.Tau) * volume;
            var sample = (short)(value * short.MaxValue);
            data[i * 2]     = (byte)(sample & 0xFF);
            data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        var wav = new AudioStreamWav
        {
            Data = data,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = sampleRate,
            Stereo = false
        };
        return wav;
    }

    // ── 预定义音效 ──

    public static AudioStreamWav HitSfx()          => CreateTone(800f, 0.08f, 0.4f);
    public static AudioStreamWav AttackSfx()        => CreateTone(300f, 0.2f, 0.3f);
    public static AudioStreamWav FootstepSfx()      => CreateTone(120f, 0.05f, 0.15f);
    public static AudioStreamWav DeathSfx()         => CreateTone(150f, 0.5f, 0.5f);
    public static AudioStreamWav PickupSfx()        => CreateTone(1000f, 0.12f, 0.3f);
    public static AudioStreamWav CheckpointSfx()    => CreateTone(600f, 0.3f, 0.35f);
    public static AudioStreamWav BossRoarSfx()       => CreateTone(100f, 0.8f, 0.6f);
    public static AudioStreamWav ExplosionSfx()       => CreateTone(60f, 0.4f, 0.5f);

    // ── BGM 占位 (1 秒循环) ──
    public static AudioStreamWav ExploreBgm() => CreateTone(200f, 1.0f, 0.15f);
    public static AudioStreamWav CombatBgm()  => CreateTone(300f, 1.0f, 0.2f);
    public static AudioStreamWav BossBgm()    => CreateTone(80f, 1.0f, 0.25f);
}
