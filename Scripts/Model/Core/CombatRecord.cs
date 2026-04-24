using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

public class CombatRecord
{
    private readonly List<MonsterRecord> _monsterRecords = new();

    public IReadOnlyList<MonsterRecord> MonsterRecords => _monsterRecords;

    public void Clear()
    {
        _monsterRecords.Clear();
    }

    public MonsterRecord EnsureExistMonster(Creature creature)
    {
        for (int i = 0; i < _monsterRecords.Count; i++)
        {
            if (ReferenceEquals(_monsterRecords[i].Creature, creature))
                return _monsterRecords[i];
        }

        var record = new MonsterRecord(creature);
        _monsterRecords.Add(record);
        return record;
    }

    public void AddPowerRecord(Creature creature, IPowerRecord powerRecord)
    {
         MonsterRecord monsterRecord = EnsureExistMonster(creature);
         monsterRecord.Enqueue(powerRecord);
    }

    public MonsterRecord? GetMonsterRecord(Creature creature)
    {
        for (int i = 0; i < _monsterRecords.Count; i++)
        {
            if (ReferenceEquals(_monsterRecords[i].Creature, creature))
                return _monsterRecords[i];
        }
        return null;
    }

    public void RemoveMonsterRecord(Creature creature)
    {
        for (int i = 0; i < _monsterRecords.Count; i++)
        {
            if (ReferenceEquals(_monsterRecords[i].Creature, creature))
            {
                _monsterRecords.RemoveAt(i);
                return;
            }
        }
    }
}
