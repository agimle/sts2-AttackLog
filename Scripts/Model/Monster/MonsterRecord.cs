using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

public class MonsterRecord
{
    public Creature Creature { get; set; }
    public Dictionary<Type, Queue<IPowerRecord>> Powers { get; private set; }
    
    public MonsterRecord(Creature creature)
    {
        Creature = creature;
        Powers = new Dictionary<Type, Queue<IPowerRecord>>();
    }

    /// <summary>
    /// Power入队
    /// </summary>
    /// <param name="record"></param>
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
        }
    }
    
    public Queue<IPowerRecord>? GetPowerQueue(Type powerType)
    {
        return Powers.TryGetValue(powerType, out var queue) ? queue : null;
    }
}