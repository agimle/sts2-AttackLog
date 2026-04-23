using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Core;

public sealed class DoomKillEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.DoomKill;
    public required IReadOnlyList<Creature> Creatures { get; init; }
}
