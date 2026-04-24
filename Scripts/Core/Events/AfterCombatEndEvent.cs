using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Core;

public sealed class AfterCombatEndEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterCombatEnd;
    public IRunState? RunState { get; init; }
    public CombatState? CombatState { get; init; }
}
