using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Core;

/// <summary>
/// Power 被施加后触发的事件
/// </summary>
public sealed class PowerReceivedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.PowerReceived;

    /// <summary>当前战斗状态</summary>
    public required CombatState CombatState { get; init; }

    /// <summary>被施加的 Power 模型</summary>
    public required PowerModel Power { get; init; }

    /// <summary>Power 施加的层数</summary>
    public decimal Amount { get; init; }

    /// <summary>Power 的施放者，可能为 null</summary>
    public Creature? Applier { get; init; }
}
