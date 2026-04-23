using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

public static class PowerRecordFactory
{
    private static readonly Dictionary<Type, Func<IPowerRecord>> Factories = new();

    public static void Register<TPower, TRecord>() 
        where TPower : PowerModel 
        where TRecord : IPowerRecord, new()
    {
        Factories[typeof(TPower)] = () => new TRecord();
    }

    public static IPowerRecord? Create(PowerModel power, Creature? applier, int amount, int roundNumber)
    {
        if (!Factories.TryGetValue(power.GetType(), out var factory))
            return null;
        
        var record = factory();
        record.Applier = applier;
        record.Amount = amount;
        return record;
    }
}