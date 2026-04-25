using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Core;

/// <summary>
/// 末日 Power 击杀时触发的事件，携带所有被击杀的生物列表
/// </summary>
public sealed class DoomKillEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.DoomKill;

    /// <summary>被末日击杀的生物列表</summary>
    public required IReadOnlyList<Creature> Creatures { get; init; }
}
