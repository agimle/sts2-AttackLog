using AttackLog.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Patch;

public static class CombatHistoryPatch
{
    public static void PowerReceivedPostfix(CombatState combatState, PowerModel power, decimal amount, Creature? applier)
    {
        AttackLogEventBus.Publish(new PowerReceivedEvent
        {
            CombatState = combatState,
            Power = power,
            Amount = amount,
            Applier = applier
        });
    }
}
