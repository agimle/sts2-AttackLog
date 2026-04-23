using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

/// <summary>
/// 造成伤害
/// </summary>
public static class AfterDamageGivenService
{
    /// <summary>
    /// 处理造成伤害
    /// </summary>
    /// <param name="dealer"></param>
    /// <param name="result"></param>
    /// <param name="target"></param>
    /// <param name="cardSource"></param>
    public static void HandleDamageGiven(Creature? dealer,DamageResult result, Creature target, CardModel? cardSource)
    {
        // 先通过dealer查找对应的playerData
        if (dealer == null)
        {
            HandlePoisonDamage(dealer, result, target, cardSource);
            return;
        }

        
        DamageGivenData damageLog = new DamageGivenData(result);
        AttackLogModel newAttackLog;
        if (dealer.IsPlayer)
        {
            // 玩家造成的伤害
            newAttackLog = new AttackLogModel
            {
                DamageOnBlock = damageLog.BlockedDamage,
                DamageOnHp = damageLog.UnblockedDamage,
                OverkillDamage = damageLog.OverkillDamage
            };
        }
        else if (dealer.IsPet)
        {
            // 召唤物造成的伤害
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
            // 敌人造成的伤害
            // 对于玩家而言只需要算盾量
            newAttackLog = new AttackLogModel
            {
                EffectiveShield = damageLog.BlockedDamage,
            };
        }
        else
        {
            return;
        }
        
        // 将攻击记录存入回合记录
        if (LogState.Instance.RunLog is null) return;
        if(dealer == null) return;
        
        // 获取玩家
        PlayerData? player = LogState.Instance.RunLog.GetPlayerByCreature(dealer);
        if(player == null) return;
        LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
        
        // 存
        
        turnLogData?.EnqueueAttackLogData(newAttackLog);
        turnLogData?.TurnLogSum.Plus(newAttackLog);
    }


    /// <summary>
    /// 处理 PoisonPower 造成的 Damage
    /// </summary>
    /// <param name="dealer"></param>
    /// <param name="result"></param>
    /// <param name="target"></param>
    /// <param name="cardSource"></param>
    private static void HandlePoisonDamage(Creature? dealer, DamageResult result, Creature target,
        CardModel? cardSource)
    {
        if (dealer != null || cardSource != null)
        {
            return;
        }

        if(!target.IsEnemy)
        {
            return;
        }
        
        // 获取目标身上的PoisonPower
        MonsterRecord? monsterRecord = LogState.Instance.CombatRecord.GetMonsterRecord(target);
        if (monsterRecord == null)
        {
            return;
        }
        Queue<IPowerRecord>? powerRecords = monsterRecord.GetPowerQueue(typeof(PoisonPower));
        if(powerRecords == null)
        {
            return;
        }

        DamageGivenData damageLog = new DamageGivenData(result);

        var playersDamageDict = PowerDamageCalculateUtils.DamageCalculate(powerRecords, damageLog);


        foreach ((Creature playerCreature, AttackLogModel newAttackLog) in playersDamageDict)
        {
            // 获取玩家
            PlayerData? player = LogState.Instance.RunLog?.GetPlayerByCreature(playerCreature);
            if(player == null) return;
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
        
            // 存
        
            turnLogData?.EnqueueAttackLogData(newAttackLog);
            turnLogData?.TurnLogSum.Plus(newAttackLog); 
        }
    }
}