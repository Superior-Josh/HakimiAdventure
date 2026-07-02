using Godot;

namespace HakimiAdventure;

/// <summary>
/// 回合制战斗管理器 — 管理战斗流程：回合切换、行动队列、胜负判定。
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

    /// <summary>玩家行动结束 → 切换到敌人回合</summary>
    public void EndPlayerTurn()
    {
        State = CombatState.EnemyTurn;
        EmitSignal(SignalName.EnemyAction);
    }

    /// <summary>敌人回合结束 → 下一回合</summary>
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
