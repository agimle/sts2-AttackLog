using AttackLog.Service;
using AttackLog.State;

namespace AttackLog.Core;

public static class AttackLogServiceLocator
{
    private static readonly Dictionary<Type, IAttackLogService> _services = new();
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;

        var logState = LogState.Instance;

        var combatService = new AttackLogCombatService(logState);
        var damageService = new AttackLogDamageService(logState);
        var powerService = new AttackLogPowerService(logState);
        var runService = new AttackLogRunService(logState);

        _services[typeof(AttackLogCombatService)] = combatService;
        _services[typeof(AttackLogDamageService)] = damageService;
        _services[typeof(AttackLogPowerService)] = powerService;
        _services[typeof(AttackLogRunService)] = runService;

        foreach (var service in _services.Values)
        {
            service.Subscribe();
        }

        _initialized = true;
    }

    public static T? GetService<T>() where T : class, IAttackLogService
    {
        return _services.TryGetValue(typeof(T), out var service) ? (T)service : null;
    }

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
