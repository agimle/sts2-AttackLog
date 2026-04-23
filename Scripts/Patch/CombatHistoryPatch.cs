using AttackLog.Model;
using AttackLog.Service;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Patch;

/// <summary>
/// 战斗记录补丁
/// </summary>
public class CombatHistoryPatch
{
    public static void PowerReceivedPostfix(CombatState combatState, PowerModel power, decimal amount,
        Creature? applier)
    {
        PowerReceivedService.AddPowerRecordToCombatRecord(combatState, power, amount, applier);
    }
}