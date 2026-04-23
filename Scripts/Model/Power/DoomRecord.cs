using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Model;

public class DoomRecord : IPowerRecord
{
    public Type PowerType => typeof(DoomPower);
    public Creature? Applier { get; set; }
    public int Amount { get; set; }
    
    
    public static void Register() => PowerRecordFactory.Register<DoomPower, DoomRecord>();
}