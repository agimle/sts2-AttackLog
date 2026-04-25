using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

public class CombatRecord
{
    private readonly Dictionary<Creature, MonsterRecord> _creatureToRecord = new();

    public IReadOnlyList<MonsterRecord> MonsterRecords => _creatureToRecord.Values.ToList();

    public void Clear()
    {
        _creatureToRecord.Clear();
    }

    public MonsterRecord EnsureExistMonster(Creature creature)
    {
        if (_creatureToRecord.TryGetValue(creature, out var existing))
            return existing;

        var record = new MonsterRecord(creature);
        _creatureToRecord[creature] = record;
        return record;
    }

    public void AddPowerRecord(Creature creature, IPowerRecord powerRecord)
    {
         MonsterRecord monsterRecord = EnsureExistMonster(creature);
         monsterRecord.Enqueue(powerRecord);
    }

    public MonsterRecord? GetMonsterRecord(Creature creature)
    {
        return _creatureToRecord.TryGetValue(creature, out var record) ? record : null;
    }

    public void RemoveMonsterRecord(Creature creature)
    {
        _creatureToRecord.Remove(creature);
    }
}
