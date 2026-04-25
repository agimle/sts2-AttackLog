using AttackLog.Core;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Service;

/// <summary>
/// Power 施加服务，当 Power 被施加到怪物身上时，
/// 通过 PowerRecordFactory 创建对应的 Power 记录并存入 CombatRecord
/// </summary>
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

    /// <summary>
    /// Power 施加事件处理
    /// </summary>
    private void OnPowerReceived(PowerReceivedEvent evt)
    {
        AddPowerRecordToCombatRecord(evt.CombatState, evt.Power, evt.Amount, evt.Applier);
    }

    /// <summary>
    /// 创建 Power 记录并添加到 CombatRecord
    /// </summary>
    /// <param name="combatState">当前战斗状态</param>
    /// <param name="power">Power 模型</param>
    /// <param name="amount">施加层数</param>
    /// <param name="applier">施放者</param>
    private void AddPowerRecordToCombatRecord(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        IPowerRecord? powerRecord = TryCreatePowerRecord(combatState, power, amount, applier);
        if (powerRecord is null) return;

        State.CombatRecord.AddPowerRecord(power.Owner, powerRecord);
    }

    /// <summary>
    /// 尝试通过 PowerRecordFactory 创建 Power 记录。
    /// 仅当施放者存在且 Power 类型已注册时返回记录实例
    /// </summary>
    /// <param name="combatState">当前战斗状态</param>
    /// <param name="power">Power 模型</param>
    /// <param name="amount">施加层数</param>
    /// <param name="applier">施放者</param>
    /// <returns>Power 记录，无法创建时返回 null</returns>
    private static IPowerRecord? TryCreatePowerRecord(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        if (applier is null) return null;

        return PowerRecordFactory.Create(power, applier, (int)amount, combatState.RoundNumber);
    }
}
