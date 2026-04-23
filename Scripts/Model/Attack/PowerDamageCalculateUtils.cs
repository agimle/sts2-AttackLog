using AttackLog.Logger;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

public static class PowerDamageCalculateUtils
{
    public static Dictionary<Creature,AttackLogModel> DamageCalculate(Queue<IPowerRecord> powerRecords,DamageGivenData damageGivenData)
    {
        Dictionary<Creature,AttackLogModel> result = new Dictionary<Creature, AttackLogModel>();
        Queue<IPowerRecord> recordQueue = new Queue<IPowerRecord>(powerRecords);
        
        // 先算被阻挡伤害
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
        
        // 再算打血
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

        // 最后算过量
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