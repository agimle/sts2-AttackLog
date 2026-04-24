using AttackLog.Core;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

public class AttackLogDamageService : IAttackLogService
{
    public void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.AfterDamageGiven, OnAfterDamageGiven);
        AttackLogEventBus.Subscribe(AttackLogEventType.AfterDeath, OnAfterDeath);
        AttackLogEventBus.Subscribe(AttackLogEventType.DoomKill, OnDoomKill);
    }

    public void Unsubscribe()
    {
        AttackLogEventBus.Unsubscribe(AttackLogEventType.AfterDamageGiven, OnAfterDamageGiven);
        AttackLogEventBus.Unsubscribe(AttackLogEventType.AfterDeath, OnAfterDeath);
        AttackLogEventBus.Unsubscribe(AttackLogEventType.DoomKill, OnDoomKill);
    }

    private void OnAfterDamageGiven(IAttackLogEvent e)
    {
        var evt = (AfterDamageGivenEvent)e;
        HandleDamageGiven(evt.Dealer, evt.Result, evt.Target, evt.CardSource);
    }

    private void OnAfterDeath(IAttackLogEvent e)
    {
        var evt = (AfterDeathEvent)e;
        ClearMonsterPower(evt.Creature);
    }

    private void OnDoomKill(IAttackLogEvent e)
    {
        var evt = (DoomKillEvent)e;
        HandleDoomKill(evt.Creatures);
    }

    private void HandleDamageGiven(Creature? dealer, DamageResult result, Creature target, CardModel? cardSource)
    {
        if (dealer == null)
        {
            HandlePoisonDamage(dealer, result, target, cardSource);
            return;
        }

        DamageGivenData damageLog = new DamageGivenData(result);
        AttackLogModel newAttackLog;
        if (dealer.IsPlayer)
        {
            newAttackLog = new AttackLogModel
            {
                DamageOnBlock = damageLog.BlockedDamage,
                DamageOnHp = damageLog.UnblockedDamage,
                OverkillDamage = damageLog.OverkillDamage
            };
        }
        else if (dealer.IsPet)
        {
            newAttackLog = new AttackLogModel
            {
                DamageOnBlock = damageLog.BlockedDamage,
                DamageOnHp = damageLog.UnblockedDamage,
                OverkillDamage = damageLog.OverkillDamage
            };
            Player? petOwner = dealer.PetOwner;
            dealer = petOwner?.Creature;
        }
        else if (dealer.IsEnemy)
        {
            newAttackLog = new AttackLogModel
            {
                EffectiveShield = damageLog.BlockedDamage,
            };
        }
        else
        {
            return;
        }

        if (LogState.Instance.RunLog is null) return;
        if (dealer == null) return;

        PlayerData? player = LogState.Instance.RunLog.GetPlayerByCreature(dealer);
        if (player == null) return;
        LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);

        turnLogData?.EnqueueAttackLogData(newAttackLog);
        turnLogData?.TurnLogSum.Plus(newAttackLog);

        LogState.Instance.InvalidateCache();
    }

    private void HandlePoisonDamage(Creature? dealer, DamageResult result, Creature target,
        CardModel? cardSource)
    {
        if (dealer != null || cardSource != null) return;
        if (!target.IsEnemy) return;

        MonsterRecord? monsterRecord = LogState.Instance.CombatRecord.GetMonsterRecord(target);
        if (monsterRecord == null) return;

        Queue<IPowerRecord>? powerRecords = monsterRecord.GetPowerQueue(typeof(PoisonPower));
        if (powerRecords == null) return;

        DamageGivenData damageLog = new DamageGivenData(result);

        var playersDamageDict = PowerDamageCalculateUtils.DamageCalculate(powerRecords, damageLog);

        foreach ((Creature playerCreature, AttackLogModel newAttackLog) in playersDamageDict)
        {
            PlayerData? player = LogState.Instance.RunLog?.GetPlayerByCreature(playerCreature);
            if (player == null) return;
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);

            turnLogData?.EnqueueAttackLogData(newAttackLog);
            turnLogData?.TurnLogSum.Plus(newAttackLog);

            LogState.Instance.InvalidateCache();
        }
    }

    private void ClearMonsterPower(Creature creature)
    {
        if (!creature.IsEnemy) return;

        LogState.Instance.CombatRecord.RemoveMonsterRecord(creature);
    }

    private void HandleDoomKill(IReadOnlyList<Creature> creatures)
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
