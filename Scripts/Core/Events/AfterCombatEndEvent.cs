using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Core;

public sealed class AfterCombatEndEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterCombatEnd;
    public required IRunState RunState { get; init; }
    public required CombatState CombatState { get; init; }
}
