using AttackLog.Core;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

public static class PowerReceivedService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.PowerReceived, OnPowerReceived);
    }

    private static void OnPowerReceived(IAttackLogEvent e)
    {
        var evt = (PowerReceivedEvent)e;
        AddPowerRecordToCombatRecord(evt.CombatState, evt.Power, evt.Amount, evt.Applier);
    }

    private static IPowerRecord? TryCatchPowerRecord(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        if (applier is null)
        {
            return null;
        }

        IPowerRecord? powerRecord = PowerRecordFactory.Create(power, applier, (int)amount, combatState.RoundNumber);

        return powerRecord;
    }

    public static void AddPowerRecordToCombatRecord(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        IPowerRecord? powerRecord = TryCatchPowerRecord(combatState, power, amount, applier);
        if (powerRecord is null) return;

        LogState.Instance.CombatRecord.AddPowerRecord(power.Owner, powerRecord);
    }
}
