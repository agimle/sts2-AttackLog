using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

public interface IPowerRecord
{
    public Creature? Applier { get; set; }
    public int Amount { get; set; }          // 施加层数
    public int SetRoundNumber { get; set; }     // 在哪个回合施加

    public bool ShouldBeRemoved(CombatState combatState);
}