using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Core;

public sealed class AfterDeathEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterDeath;
    public required Creature Creature { get; init; }
}
