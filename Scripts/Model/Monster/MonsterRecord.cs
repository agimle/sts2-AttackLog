using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

/// <summary>
/// 怪物记录，管理单个怪物身上所有 Power 的施加历史。
/// 按 Power 类型分组存储，每组上限 50 条记录，超出自动淘汰。
/// </summary>
public class MonsterRecord
{
    /// <summary>每种 Power 类型的记录历史上限</summary>
    private const int MaxPowerRecordHistory = 50;

    /// <summary>关联的怪物实例</summary>
    public Creature Creature { get; set; }

    /// <summary>Power 类型 → 施加记录队列</summary>
    public Dictionary<Type, Queue<IPowerRecord>> Powers { get; private set; }

    public MonsterRecord(Creature creature)
    {
        Creature = creature;
        Powers = new Dictionary<Type, Queue<IPowerRecord>>();
    }

    /// <summary>
    /// 入队 Power 记录，按类型分组存储，超过上限时自动淘汰最旧记录
    /// </summary>
    /// <param name="record">Power 记录</param>
    public void Enqueue(IPowerRecord record)
    {
        Powers.TryGetValue(record.PowerType, out var queue);
        if (queue is null)
        {
            Powers[record.PowerType] = new Queue<IPowerRecord>();
            Powers[record.PowerType].Enqueue(record);
        }
        else
        {
            queue.Enqueue(record);

            while (queue.Count > MaxPowerRecordHistory)
            {
                queue.Dequeue();
            }
        }
    }

    /// <summary>
    /// 获取指定 Power 类型的记录队列
    /// </summary>
    /// <param name="powerType">Power 类型</param>
    /// <returns>记录队列，不存在返回 null</returns>
    public Queue<IPowerRecord>? GetPowerQueue(Type powerType)
    {
        return Powers.TryGetValue(powerType, out var queue) ? queue : null;
    }
}
