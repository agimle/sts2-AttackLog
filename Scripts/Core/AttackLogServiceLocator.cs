using AttackLog.Service;
using AttackLog.State;

namespace AttackLog.Core;

/// <summary>
/// 服务定位器，管理所有 Service 实例的生命周期。
/// 初始化时创建所有 Service（注入 LogState）并自动订阅事件。
/// </summary>
public static class AttackLogServiceLocator
{
    private static readonly Dictionary<Type, IAttackLogService> _services = new();
    private static bool _initialized;

    /// <summary>
    /// 初始化所有 Service 实例并订阅事件。重复调用无效
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;

        var logState = LogState.Instance;

        // 创建所有 Service 实例，通过构造函数注入 LogState
        var combatService = new AttackLogCombatService(logState);
        var damageService = new AttackLogDamageService(logState);
        var powerService = new AttackLogPowerService(logState);
        var runService = new AttackLogRunService(logState);

        _services[typeof(AttackLogCombatService)] = combatService;
        _services[typeof(AttackLogDamageService)] = damageService;
        _services[typeof(AttackLogPowerService)] = powerService;
        _services[typeof(AttackLogRunService)] = runService;

        // 统一订阅所有 Service 的事件
        foreach (var service in _services.Values)
        {
            service.Subscribe();
        }

        _initialized = true;
    }

    /// <summary>
    /// 获取指定类型的 Service 实例
    /// </summary>
    /// <typeparam name="T">Service 类型</typeparam>
    /// <returns>Service 实例，未初始化或类型不存在时返回 null</returns>
    public static T? GetService<T>() where T : class, IAttackLogService
    {
        return _services.TryGetValue(typeof(T), out var service) ? (T)service : null;
    }

    /// <summary>
    /// 关闭所有 Service：取消订阅并清空注册表
    /// </summary>
    public static void Shutdown()
    {
        foreach (var service in _services.Values)
        {
            service.Unsubscribe();
        }
        _services.Clear();
        _initialized = false;
    }
}
