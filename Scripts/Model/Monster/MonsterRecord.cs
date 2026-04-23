using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

public class MonsterPowerRecord
{
    public Creature Creature { get; set; }
    public Dictionary<Type, Queue<IPowerRecord>> Powers { get; set; }
    
    public MonsterPowerRecord(Creature creature)
    {
        Creature = creature;
        Powers = new Dictionary<Type, Queue<IPowerRecord>>();
    }
}