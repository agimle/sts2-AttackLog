using AttackLog.Core;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

public class AttackLogDamageService : AttackLogServiceBase
{
    public AttackLogDamageService(LogState state) : base(state) { }

    public override void Subscribe()
    {
        AttackLogEventBus.Subscribe<AfterDamageGivenEvent>(OnAfterDamageGiven);
        AttackLogEventBus.Subscribe<AfterDeathEvent>(OnAfterDeath);
        AttackLogEventBus.Subscribe<DoomKillEvent>(OnDoomKill);
    }

    public override void Unsubscribe()
    {
        AttackLogEventBus.Unsubscribe<AfterDamageGivenEvent>(OnAfterDamageGiven);
        AttackLogEventBus.Unsubscribe<AfterDeathEvent>(OnAfterDeath);
        AttackLogEventBus.Unsubscribe<DoomKillEvent>(OnDoomKill);
    }

    private void OnAfterDamageGiven(AfterDamageGivenEvent evt)
    {
        HandleDamageGiven(evt.Dealer, evt.Result, evt.Target, evt.CardSource);
    }

    private void OnAfterDeath(AfterDeathEvent evt)
    {
        ClearMonsterPower(evt.Creature);
    }

    private void OnDoomKill(DoomKillEvent evt)
    {
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

        if (State.RunLog is null) return;
        if (dealer == null) return;

        PlayerData? player = State.RunLog.GetPlayerByCreature(dealer);
        if (player == null) return;
        State.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);

        turnLogData?.EnqueueAttackLogData(newAttackLog);
        turnLogData?.TurnLogSum.Add(newAttackLog);

        State.InvalidateCache();
    }

    private void HandlePoisonDamage(Creature? dealer, DamageResult result, Creature target,
        CardModel? cardSource)
    {
        if (dealer != null || cardSource != null) return;
        if (!target.IsEnemy) return;

        MonsterRecord? monsterRecord = State.CombatRecord.GetMonsterRecord(target);
        if (monsterRecord == null) return;

        Queue<IPowerRecord>? powerRecords = monsterRecord.GetPowerQueue(typeof(PoisonPower));
        if (powerRecords == null) return;

        DamageGivenData damageLog = new DamageGivenData(result);

        var playersDamageDict = PowerDamageCalculateUtils.DamageCalculate(powerRecords, damageLog);

        foreach ((Creature playerCreature, AttackLogModel newAttackLog) in playersDamageDict)
        {
            PlayerData? player = State.RunLog?.GetPlayerByCreature(playerCreature);
            if (player == null) return;
            State.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);

            turnLogData?.EnqueueAttackLogData(newAttackLog);
            turnLogData?.TurnLogSum.Add(newAttackLog);

            State.InvalidateCache();
        }
    }

    private void ClearMonsterPower(Creature creature)
    {
        if (!creature.IsEnemy) return;

        State.CombatRecord.RemoveMonsterRecord(creature);
    }

    private void HandleDoomKill(IReadOnlyList<Creature> creatures)
    {
        if (creatures.Count == 0) return;

        foreach (var creature in creatures)
        {
            MonsterRecord? monsterRecord = State.CombatRecord.GetMonsterRecord(creature);

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
                PlayerData? player = State.RunLog?.GetPlayerByCreature(playerCreature);
                if (player == null) continue;
                State.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);

                turnLogData?.EnqueueAttackLogData(newAttackLog);
                turnLogData?.TurnLogSum.Add(newAttackLog);
            }
        }
    }
}
