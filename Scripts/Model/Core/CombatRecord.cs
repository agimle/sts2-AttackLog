using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

/// <summary>
/// 战斗记录，管理当前战斗中所有怪物的 Power 记录。
/// 每场战斗开始时清空，战斗结束时随 CombatState 一起重置。
/// </summary>
public class CombatRecord
{
    /// <summary>Creature → MonsterRecord 的映射</summary>
    private readonly Dictionary<Creature, MonsterRecord> _creatureToRecord = new();

    /// <summary>所有怪物记录的只读列表</summary>
    public IReadOnlyList<MonsterRecord> MonsterRecords => _creatureToRecord.Values.ToList();

    /// <summary>
    /// 清空所有怪物记录
    /// </summary>
    public void Clear()
    {
        _creatureToRecord.Clear();
    }

    /// <summary>
    /// 确保指定怪物的记录存在，不存在则创建
    /// </summary>
    /// <param name="creature">怪物实例</param>
    /// <returns>怪物的记录实例</returns>
    public MonsterRecord EnsureExistMonster(Creature creature)
    {
        if (_creatureToRecord.TryGetValue(creature, out var existing))
            return existing;

        var record = new MonsterRecord(creature);
        _creatureToRecord[creature] = record;
        return record;
    }

    /// <summary>
    /// 为指定怪物添加 Power 记录
    /// </summary>
    /// <param name="creature">怪物实例</param>
    /// <param name="powerRecord">Power 记录</param>
    public void AddPowerRecord(Creature creature, IPowerRecord powerRecord)
    {
         MonsterRecord monsterRecord = EnsureExistMonster(creature);
         monsterRecord.Enqueue(powerRecord);
    }

    /// <summary>
    /// 获取指定怪物的记录
    /// </summary>
    /// <param name="creature">怪物实例</param>
    /// <returns>怪物记录，不存在返回 null</returns>
    public MonsterRecord? GetMonsterRecord(Creature creature)
    {
        return _creatureToRecord.TryGetValue(creature, out var record) ? record : null;
    }

    /// <summary>
    /// 移除指定怪物的记录（怪物死亡时调用）
    /// </summary>
    /// <param name="creature">怪物实例</param>
    public void RemoveMonsterRecord(Creature creature)
    {
        _creatureToRecord.Remove(creature);
    }
}
