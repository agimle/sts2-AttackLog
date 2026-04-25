using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.Core;

/// <summary>
/// 回合结束后触发的事件，携带当前回合的敌我方标识
/// </summary>
public sealed class AfterTurnEndEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterTurnEnd;

    /// <summary>当前战斗状态</summary>
    public required CombatState CombatState { get; init; }

    /// <summary>当前回合的敌我方标识</summary>
    public CombatSide Side { get; init; }
}
