using AttackLog.Service;

namespace AttackLog.Core;

public static class AttackLogServiceLocator
{
    private static readonly Dictionary<Type, IAttackLogService> _services = new();
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;

        var combatService = new AttackLogCombatService();
        var damageService = new AttackLogDamageService();
        var powerService = new AttackLogPowerService();
        var runService = new AttackLogRunService();

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
