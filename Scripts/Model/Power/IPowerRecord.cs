using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

public interface IPowerRecord
{
    public Type PowerType { get;}
    public Creature? Applier { get; set; }
    public int Amount { get; set; }          // 施加层数
}