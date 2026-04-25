using AttackLog.Core;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

/// <summary>
/// 伤害处理服务，负责三种伤害来源的记录：
/// 1. 直接伤害（卡牌/攻击）→ 直接归因到玩家
/// 2. 中毒伤害（间接伤害）→ 通过 PowerDamageCalculateUtils 归因到施毒者
/// 3. 末日击杀 → 通过 PowerDamageCalculateUtils 归因到末日施放者
/// 同时处理怪物死亡时清理 Power 记录
/// </summary>
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

    /// <summary>
    /// 伤害结算事件处理
    /// </summary>
    private void OnAfterDamageGiven(AfterDamageGivenEvent evt)
    {
        HandleDamageGiven(evt.Dealer, evt.Result, evt.Target, evt.CardSource);
    }

    /// <summary>
    /// 生物死亡事件处理：清理怪物 Power 记录
    /// </summary>
    private void OnAfterDeath(AfterDeathEvent evt)
    {
        ClearMonsterPower(evt.Creature);
    }

    /// <summary>
    /// 末日击杀事件处理：归因末日伤害
    /// </summary>
    private void OnDoomKill(DoomKillEvent evt)
    {
        HandleDoomKill(evt.Creatures);
    }

    /// <summary>
    /// 处理伤害结算，区分直接伤害和间接伤害（中毒等）
    /// </summary>
    /// <param name="dealer">伤害来源</param>
    /// <param name="result">伤害结果</param>
    /// <param name="target">受伤目标</param>
    /// <param name="cardSource">卡牌来源</param>
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
        turnLogData?.TurnLogSum.Accumulate(newAttackLog);

        State.InvalidateCache();
    }

    /// <summary>
    /// 处理中毒等间接伤害，通过 PowerDamageCalculateUtils 按施加顺序归因到施毒者
    /// </summary>
    /// <param name="dealer">伤害来源（中毒时为 null）</param>
    /// <param name="result">伤害结果</param>
    /// <param name="target">受伤目标</param>
    /// <param name="cardSource">卡牌来源</param>
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
            turnLogData?.TurnLogSum.Accumulate(newAttackLog);

            State.InvalidateCache();
        }
    }

    /// <summary>
    /// 清理怪物死亡后的 Power 记录
    /// </summary>
    /// <param name="creature">死亡的生物</param>
    private void ClearMonsterPower(Creature creature)
    {
        if (!creature.IsEnemy) return;

        State.CombatRecord.RemoveMonsterRecord(creature);
    }

    /// <summary>
    /// 处理末日击杀，将怪物的剩余血量归因到末日 Power 的施放者
    /// </summary>
    /// <param name="creatures">被末日击杀的生物列表</param>
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
                turnLogData?.TurnLogSum.Accumulate(newAttackLog);
            }
        }
    }
}
