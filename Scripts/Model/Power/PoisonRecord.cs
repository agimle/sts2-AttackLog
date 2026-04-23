using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Model;

public class PoisonRecord : IPowerRecord
{
    public Type PowerType => typeof(PoisonPower);
    public Creature? Applier { get; set; }
    public int Amount { get; set; }          // 施加层数
    
    public static void Register() => PowerRecordFactory.Register<PoisonPower, PoisonRecord>();
}