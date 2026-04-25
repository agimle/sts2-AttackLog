using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Model;

/// <summary>
/// 末日 Power 记录，追踪 DoomPower 的施加者和层数。
/// 通过 Register 方法向 PowerRecordFactory 注册类型映射。
/// </summary>
public class DoomRecord : IPowerRecord
{
    /// <summary>对应 DoomPower 类型</summary>
    public Type PowerType => typeof(DoomPower);

    /// <summary>施放者</summary>
    public Creature? Applier { get; set; }

    /// <summary>施加层数</summary>
    public int Amount { get; set; }

    /// <summary>
    /// 向 PowerRecordFactory 注册 DoomPower → DoomRecord 的映射
    /// </summary>
    public static void Register() => PowerRecordFactory.Register<DoomPower, DoomRecord>();
}
