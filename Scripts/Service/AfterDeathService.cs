using AttackLog.Core;
using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Service;

public static class AfterDeathService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.AfterDeath, OnAfterDeath);
    }

    private static void OnAfterDeath(IAttackLogEvent e)
    {
        var evt = (AfterDeathEvent)e;
        ClearMonsterPower(evt.Creature);
    }

    public static void ClearMonsterPower(Creature creature)
    {
        if (!creature.IsEnemy) return;

        LogState.Instance.CombatRecord.RemoveMonsterRecord(creature);
    }
}
