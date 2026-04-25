using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Core;

/// <summary>
/// 战斗开始前触发的事件
/// </summary>
public sealed class BeforeCombatStartEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.BeforeCombatStart;

    /// <summary>当前 Run 的状态</summary>
    public required IRunState RunState { get; init; }

    /// <summary>当前战斗状态</summary>
    public required CombatState CombatState { get; init; }
}
