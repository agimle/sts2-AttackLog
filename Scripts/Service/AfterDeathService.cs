using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Service;

public static class AfterDeathService
{
    /// <summary>
    /// 清除power
    /// </summary>
    /// <param name="creature"></param>
    public static void ClearMonsterPower(Creature creature)
    {
        if(!creature.IsEnemy) return;
        
        LogState.Instance.CombatRecord.RemoveMonsterRecord(creature);
    }
}