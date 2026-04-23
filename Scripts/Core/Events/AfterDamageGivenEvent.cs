using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Core;

public sealed class AfterDamageGivenEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterDamageGiven;
    public Creature? Dealer { get; init; }
    public required DamageResult Result { get; init; }
    public required Creature Target { get; init; }
    public CardModel? CardSource { get; init; }
}
