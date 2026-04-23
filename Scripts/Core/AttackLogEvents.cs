using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace AttackLog.Core;

public enum AttackLogEventType
{
    RunStarted,
    BeforeCombatStart,
    BeforeSideTurnStart,
    AfterTurnEnd,
    AfterCombatEnd,
    AfterDamageGiven,
    AfterDeath,
    PowerReceived,
    DoomKill,
    RefreshRequested
}

public interface IAttackLogEvent
{
    AttackLogEventType Type { get; }
}

public sealed class RunStartedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.RunStarted;
    public required IRunState RunState { get; init; }
}

public sealed class BeforeCombatStartEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.BeforeCombatStart;
    public IRunState? RunState { get; init; }
    public CombatState? CombatState { get; init; }
}

public sealed class BeforeSideTurnStartEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.BeforeSideTurnStart;
    public required CombatState CombatState { get; init; }
    public CombatSide Side { get; init; }
}

public sealed class AfterTurnEndEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterTurnEnd;
    public required CombatState CombatState { get; init; }
    public CombatSide Side { get; init; }
}

public sealed class AfterCombatEndEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterCombatEnd;
    public IRunState? RunState { get; init; }
    public CombatState? CombatState { get; init; }
}

public sealed class AfterDamageGivenEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterDamageGiven;
    public Creature? Dealer { get; init; }
    public required DamageResult Result { get; init; }
    public required Creature Target { get; init; }
    public CardModel? CardSource { get; init; }
}

public sealed class AfterDeathEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterDeath;
    public required Creature Creature { get; init; }
}

public sealed class PowerReceivedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.PowerReceived;
    public required CombatState CombatState { get; init; }
    public required PowerModel Power { get; init; }
    public decimal Amount { get; init; }
    public Creature? Applier { get; init; }
}

public sealed class DoomKillEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.DoomKill;
    public required IReadOnlyList<Creature> Creatures { get; init; }
}

public sealed class RefreshRequestedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.RefreshRequested;
}
