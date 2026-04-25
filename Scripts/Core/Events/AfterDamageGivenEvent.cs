using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Core;

/// <summary>
/// 伤害结算后触发的事件，携带伤害来源、目标和结果
/// </summary>
public sealed class AfterDamageGivenEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.AfterDamageGiven;

    /// <summary>伤害来源，可能为 null（如中毒等间接伤害）</summary>
    public Creature? Dealer { get; init; }

    /// <summary>伤害结果（包含格挡、血量、过量伤害等）</summary>
    public required DamageResult Result { get; init; }

    /// <summary>受伤目标</summary>
    public required Creature Target { get; init; }

    /// <summary>造成伤害的卡牌来源，可能为 null</summary>
    public CardModel? CardSource { get; init; }
}
