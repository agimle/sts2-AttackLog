using AttackLog.Core;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Service;

public class AttackLogPowerService : AttackLogServiceBase
{
    public AttackLogPowerService(LogState state) : base(state) { }

    public override void Subscribe()
    {
        AttackLogEventBus.Subscribe<PowerReceivedEvent>(OnPowerReceived);
    }

    public override void Unsubscribe()
    {
        AttackLogEventBus.Unsubscribe<PowerReceivedEvent>(OnPowerReceived);
    }

    private void OnPowerReceived(PowerReceivedEvent evt)
    {
        AddPowerRecordToCombatRecord(evt.CombatState, evt.Power, evt.Amount, evt.Applier);
    }

    private void AddPowerRecordToCombatRecord(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        IPowerRecord? powerRecord = TryCreatePowerRecord(combatState, power, amount, applier);
        if (powerRecord is null) return;

        State.CombatRecord.AddPowerRecord(power.Owner, powerRecord);
    }

    private static IPowerRecord? TryCreatePowerRecord(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        if (applier is null) return null;

        return PowerRecordFactory.Create(power, applier, (int)amount, combatState.RoundNumber);
    }
}
