using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Core;

/// <summary>
/// 生物死亡后触发的事件
/// </summary>
public sealed class AfterDeathEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterDeath;

    /// <summary>死亡的生物</summary>
    public required Creature Creature { get; init; }
}
