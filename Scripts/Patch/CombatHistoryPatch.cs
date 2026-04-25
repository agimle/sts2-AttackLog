using AttackLog.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Patch;

/// <summary>
/// 战斗历史补丁，在 PowerReceived 后发布事件。
/// 用于追踪能力（如中毒、易伤等）的施加情况
/// </summary>
public static class CombatHistoryPatch
{
    /// <summary>
    /// PowerReceived 后置回调，发布 PowerReceivedEvent
    /// </summary>
    /// <param name="combatState">当前战斗状态</param>
    /// <param name="power">施加的能力模型</param>
    /// <param name="amount">施加数量</param>
    /// <param name="applier">施加者（可能为 null）</param>
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
