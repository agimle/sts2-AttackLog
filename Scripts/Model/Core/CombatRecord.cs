using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

public class CombatRecord
{
    public Dictionary<Creature,MonsterRecord> MonsterRecords { get; private set; }

    public CombatRecord()
    {
        MonsterRecords = new Dictionary<Creature,MonsterRecord>();
    }

    public void Clear()
    {
        MonsterRecords.Clear();
    }

    public MonsterRecord EnsureExistMonster(Creature creature)
    {
        if (!MonsterRecords.ContainsKey(creature))
        {
            MonsterRecords[creature] = new MonsterRecord(creature);
        }

        return MonsterRecords[creature];
    }

    public void AddPowerRecord(Creature creature, IPowerRecord powerRecord)
    {
         MonsterRecord monsterRecord = EnsureExistMonster(creature);

         monsterRecord.Enqueue(powerRecord);
    }

    public MonsterRecord? GetMonsterRecord(Creature creature)
    {
        return MonsterRecords.TryGetValue(creature, out var record) ? record : null;
    }

    public void RemoveMonsterRecord(Creature creature)
    {
        MonsterRecords.Remove(creature);
    }
}
