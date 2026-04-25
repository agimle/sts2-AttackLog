using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace AttackLog.Model;

/// <summary>
/// Power 记录工厂，使用泛型注册机制将游戏 PowerModel 类型映射到对应的 IPowerRecord 类型。
/// 运行时通过 PowerModel 的运行时类型查找并创建对应的 Record 实例。
/// </summary>
public static class PowerRecordFactory
{
    /// <summary>PowerModel 类型 → IPowerRecord 工厂方法的映射</summary>
    private static readonly Dictionary<Type, Func<IPowerRecord>> Factories = new();

    /// <summary>
    /// 注册 PowerModel 类型到 IPowerRecord 类型的映射
    /// </summary>
    /// <typeparam name="TPower">游戏 PowerModel 类型</typeparam>
    /// <typeparam name="TRecord">对应的 IPowerRecord 实现类型</typeparam>
    public static void Register<TPower, TRecord>()
        where TPower : PowerModel
        where TRecord : IPowerRecord, new()
    {
        Factories[typeof(TPower)] = () => new TRecord();
    }

    /// <summary>
    /// 根据 PowerModel 实例创建对应的 IPowerRecord
    /// </summary>
    /// <param name="power">游戏 PowerModel 实例</param>
    /// <param name="applier">Power 施放者</param>
    /// <param name="amount">施加层数</param>
    /// <param name="roundNumber">当前回合号</param>
    /// <returns>创建的 IPowerRecord，未注册的类型返回 null</returns>
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
