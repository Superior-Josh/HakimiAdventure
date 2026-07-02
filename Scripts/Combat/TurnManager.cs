using Godot;

namespace HakimiAdventure;

/// <summary>
/// 战斗管理器 — 管理类国王密令风格战斗流程：行动阶段切换、队列、胜负判定。
/// 注：King's Field 为即时体力制，此模块采用信号驱动的阶段切换来模拟战斗节奏。
/// </summary>
public partial class TurnManager : Node
{
    [Signal] public delegate void TurnStartedEventHandler(int turnNumber);
    [Signal] public delegate void PlayerActionEventHandler();
    [Signal] public delegate void EnemyActionEventHandler();
    [Signal] public delegate void CombatEndedEventHandler(bool playerWon);

    public enum CombatState { Idle, PlayerTurn, EnemyTurn, Victory, Defeat }
    public CombatState State { get; private set; } = CombatState.Idle;

    public int TurnNumber { get; private set; } = 0;

    public void StartCombat()
    {
        TurnNumber = 0;
        State = CombatState.PlayerTurn;
        NextTurn();
    }

    public void NextTurn()
    {
        TurnNumber++;
        EmitSignal(SignalName.TurnStarted, TurnNumber);
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        State = CombatState.PlayerTurn;
        EmitSignal(SignalName.PlayerAction);
    }

    /// <summary>玩家行动结束 → 切换到敌人阶段</summary>
    public void EndPlayerTurn()
    {
        State = CombatState.EnemyTurn;
        EmitSignal(SignalName.EnemyAction);
    }

    /// <summary>敌人阶段结束 → 下一行动阶段</summary>
    public void EndEnemyTurn()
    {
        NextTurn();
    }

    public void EndCombat(bool playerWon)
    {
        State = playerWon ? CombatState.Victory : CombatState.Defeat;
        EmitSignal(SignalName.CombatEnded, playerWon);
    }
}
