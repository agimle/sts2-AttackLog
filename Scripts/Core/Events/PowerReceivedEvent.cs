using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Core;

public sealed class PowerReceivedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.PowerReceived;
    public required CombatState CombatState { get; init; }
    public required PowerModel Power { get; init; }
    public decimal Amount { get; init; }
    public Creature? Applier { get; init; }
}
