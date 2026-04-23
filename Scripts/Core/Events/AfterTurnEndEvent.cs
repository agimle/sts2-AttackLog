using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.Core;

public sealed class AfterTurnEndEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterTurnEnd;
    public required CombatState CombatState { get; init; }
    public CombatSide Side { get; init; }
}
