using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

/// <summary>
/// Power 记录接口，所有 Power 记录必须实现此接口。
/// 用于追踪 Power 的施加者、层数和类型，支持伤害归因计算。
/// </summary>
public interface IPowerRecord
{
    /// <summary>Power 的运行时类型，用于分组存储和查找</summary>
    public Type PowerType { get; }

    /// <summary>Power 的施放者，可能为 null</summary>
    public Creature? Applier { get; set; }

    /// <summary>施加的层数</summary>
    public int Amount { get; set; }
}
