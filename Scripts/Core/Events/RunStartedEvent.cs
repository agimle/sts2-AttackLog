using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Core;

public sealed class RunStartedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.RunStarted;
    public required IRunState RunState { get; init; }
}
