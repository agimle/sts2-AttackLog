using AttackLog.Core;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

public static class DoomKillService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.DoomKill, OnDoomKill);
    }

    private static void OnDoomKill(IAttackLogEvent e)
    {
        var evt = (DoomKillEvent)e;
        HandleDoomKill(evt.Creatures);
    }

    public static void HandleDoomKill(IReadOnlyList<Creature> creatures)
    {
        if (creatures.Count == 0) return;

        foreach (var creature in creatures)
        {
            MonsterRecord? monsterRecord = LogState.Instance.CombatRecord.GetMonsterRecord(creature);

            if (monsterRecord == null) continue;

            Queue<IPowerRecord>? powerRecords = monsterRecord.GetPowerQueue(typeof(DoomPower));

            if (powerRecords == null) continue;

            DamageGivenData damageGivenData = new DamageGivenData
            {
                Receiver = creature,
                BlockedDamage = 0,
                UnblockedDamage = creature.CurrentHp,
                OverkillDamage = 0
            };

            var playersDamageDict = PowerDamageCalculateUtils.DamageCalculate(powerRecords, damageGivenData);

            foreach ((Creature playerCreature, AttackLogModel newAttackLog) in playersDamageDict)
            {
                PlayerData? player = LogState.Instance.RunLog?.GetPlayerByCreature(playerCreature);
                if (player == null) continue;
                LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);

                turnLogData?.EnqueueAttackLogData(newAttackLog);
                turnLogData?.TurnLogSum.Plus(newAttackLog);
            }
        }
    }
}
