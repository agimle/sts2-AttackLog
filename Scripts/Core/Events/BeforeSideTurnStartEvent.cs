using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.Core;

public sealed class BeforeSideTurnStartEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.BeforeSideTurnStart;
    public required CombatState CombatState { get; init; }
    public CombatSide Side { get; init; }
}
