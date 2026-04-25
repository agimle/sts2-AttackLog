using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.Core;

/// <summary>
/// 回合开始前触发的事件，携带当前回合的敌我方标识
/// </summary>
public sealed class BeforeSideTurnStartEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.BeforeSideTurnStart;

    /// <summary>当前战斗状态</summary>
    public required CombatState CombatState { get; init; }

    /// <summary>当前回合的敌我方标识</summary>
    public CombatSide Side { get; init; }
}
