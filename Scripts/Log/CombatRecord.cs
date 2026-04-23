using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

/// <summary>
/// 战斗记录，用于记录部分信息
/// Power
/// </summary>
public class CombatRecord
{
    // 记录怪物
    public Dictionary<Creature,MonsterRecord> MonsterRecords { get; private set; }
    
    // 战斗中记录 PoisonPower 和 DoomPower
    
    public CombatRecord()
    {
        MonsterRecords = new Dictionary<Creature,MonsterRecord>();
    }
    
    public void Clear()
    {
        MonsterRecords.Clear();
    }

    /// <summary>
    /// 确保怪物列表里有怪物
    /// </summary>
    /// <param name="creature"></param>
    /// <returns></returns>
    public MonsterRecord EnsureExistMonster(Creature creature)
    {
        if (!MonsterRecords.ContainsKey(creature))
        {
            MonsterRecords[creature] = new MonsterRecord(creature);
        }
        
        return MonsterRecords[creature];
    }

    /// <summary>
    /// 添加Power记录
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="powerRecord"></param>
    public void AddPowerRecord(Creature creature, IPowerRecord powerRecord)
    {
         MonsterRecord monsterRecord = EnsureExistMonster(creature);
         
         monsterRecord.Enqueue(powerRecord);
    }
    
    /// <summary>
    /// 获取怪物的全部记录
    /// </summary>
    /// <param name="creature"></param>
    /// <returns></returns>
    public MonsterRecord? GetMonsterRecord(Creature creature)
    {
        return MonsterRecords.TryGetValue(creature, out var record) ? record : null;
    }
    
    public void RemoveMonsterRecord(Creature creature)
    {
        MonsterRecords.Remove(creature);
    }
}