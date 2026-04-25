using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Model;

/// <summary>
/// 中毒 Power 记录，追踪 PoisonPower 的施加者和层数。
/// 通过 Register 方法向 PowerRecordFactory 注册类型映射。
/// </summary>
public class PoisonRecord : IPowerRecord
{
    /// <summary>对应 PoisonPower 类型</summary>
    public Type PowerType => typeof(PoisonPower);

    /// <summary>施放者</summary>
    public Creature? Applier { get; set; }

    /// <summary>施加层数</summary>
    public int Amount { get; set; }

    /// <summary>
    /// 向 PowerRecordFactory 注册 PoisonPower → PoisonRecord 的映射
    /// </summary>
    public static void Register() => PowerRecordFactory.Register<PoisonPower, PoisonRecord>();
}
