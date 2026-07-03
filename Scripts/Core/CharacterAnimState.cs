using Godot;

namespace HakimiAdventure.Core;

/// <summary> 动画状态枚举 — 配合 AnimationPlayer.Play() 字符串驱动 </summary>
public enum CharacterAnimState
{
    Idle,
    Walk,
    Run,
    Attack,
    Hit,
    Death,
    Cast,
    Jump
}

/// <summary> 动画状态工具方法 </summary>
public static class CharacterAnimHelper
{
    /// <summary> 根据状态获取动画名称（可在子类中 override 或直接使用） </summary>
    public static string GetAnimName(CharacterAnimState state) => state switch
    {
        CharacterAnimState.Idle   => "idle",
        CharacterAnimState.Walk   => "walk",
        CharacterAnimState.Run    => "run",
        CharacterAnimState.Attack => "attack",
        CharacterAnimState.Hit    => "hit",
        CharacterAnimState.Death  => "death",
        CharacterAnimState.Cast   => "cast",
        CharacterAnimState.Jump   => "jump",
        _                         => "idle"
    };
}
