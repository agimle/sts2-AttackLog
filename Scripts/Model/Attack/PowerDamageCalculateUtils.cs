using AttackLog.Logger;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

/// <summary>
/// Power 伤害归因计算工具。
/// 按照 Power 施加顺序（FIFO），依次分配格挡伤害、血量伤害和过量伤害到各施放者。
/// 核心算法：先消耗格挡伤害 → 再消耗血量伤害 → 最后消耗过量伤害，
/// 每一步按 Power 记录队列顺序逐个分配，直到伤害耗尽或队列清空。
/// </summary>
public static class PowerDamageCalculateUtils
{
    /// <summary>
    /// 根据 Power 记录队列和伤害数据，计算每个施放者造成的伤害分布
    /// </summary>
    /// <param name="powerRecords">Power 施加记录队列（按施加时间排序）</param>
    /// <param name="damageGivenData">伤害数据</param>
    /// <returns>施放者 → AttackLogModel 的映射，包含各施放者的伤害分布</returns>
    public static Dictionary<Creature,AttackLogModel> DamageCalculate(Queue<IPowerRecord> powerRecords,DamageGivenData damageGivenData)
    {
        Dictionary<Creature,AttackLogModel> result = new Dictionary<Creature, AttackLogModel>();
        Queue<IPowerRecord> recordQueue = new Queue<IPowerRecord>(powerRecords);

        while (damageGivenData.BlockedDamage > 0)
        {
            if(recordQueue.Count == 0) break;
            IPowerRecord damageRecord = recordQueue.Dequeue();
            int damage = damageGivenData.BlockedDamage > damageRecord.Amount ? damageRecord.Amount : damageGivenData.BlockedDamage;
            damageGivenData.BlockedDamage -= damage;
            Creature? applier = damageRecord.Applier;

            if(applier == null) continue;

            if (result.TryGetValue(applier, out var attackLog))
            {
                attackLog.DamageOnBlock += damage;
            }
            else
            {
                result[applier] = new AttackLogModel
                {
                    DamageOnBlock = damage,
                    DamageOnHp = 0,
                    OverkillDamage = 0
                };
            }
        }

        while (damageGivenData.UnblockedDamage > 0)
        {
            if(recordQueue.Count == 0) break;
            IPowerRecord damageRecord = recordQueue.Dequeue();
            int damage = damageGivenData.UnblockedDamage > damageRecord.Amount ? damageRecord.Amount : damageGivenData.UnblockedDamage;
            damageGivenData.UnblockedDamage -= damage;
            Creature? applier = damageRecord.Applier;

            if(applier == null) continue;

            if (result.TryGetValue(applier, out var attackLog))
            {
                attackLog.DamageOnHp += damage;
            }
            else
            {
                result[applier] = new AttackLogModel
                {
                    DamageOnBlock = 0,
                    DamageOnHp = damage,
                    OverkillDamage = 0
                };
            }
        }

        while (damageGivenData.OverkillDamage > 0)
        {
            if(recordQueue.Count == 0) break;
            IPowerRecord damageRecord = recordQueue.Dequeue();
            int damage = damageGivenData.OverkillDamage > damageRecord.Amount ? damageRecord.Amount : damageGivenData.OverkillDamage;
            damageGivenData.OverkillDamage -= damage;
            Creature? applier = damageRecord.Applier;

            if(applier == null) continue;

            if (result.TryGetValue(applier, out var attackLog))
            {
                attackLog.OverkillDamage += damage;
            }
            else
            {
                result[applier] = new AttackLogModel
                {
                    DamageOnBlock = 0,
                    DamageOnHp = 0,
                    OverkillDamage = damage
                };
            }
        }

        return result;
    }
}
