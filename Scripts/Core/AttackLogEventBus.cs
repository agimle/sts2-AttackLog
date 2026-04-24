using System.Collections.Generic;
using System.Runtime.Serialization;
using AttackLog.Logger;

namespace AttackLog.Core;

public static class AttackLogEventBus
{
    private static readonly Dictionary<AttackLogEventType, List<Action<IAttackLogEvent>>> Subscribers = new();

    public static void Subscribe<T>(Action<T> handler) where T : class, IAttackLogEvent
    {
        var dummy = (T)FormatterServices.GetUninitializedObject(typeof(T));
        var type = dummy.Type;

        if (!Subscribers.ContainsKey(type))
            Subscribers[type] = new List<Action<IAttackLogEvent>>();
        Subscribers[type].Add(e =>
        {
            if (e is T typed)
                handler(typed);
            else
                ModLogger.Log("EventBus", $"Type mismatch: expected {typeof(T).Name}, got {e.GetType().Name}");
        });
    }

    public static void Subscribe(AttackLogEventType type, Action<IAttackLogEvent> handler)
    {
        if (!Subscribers.ContainsKey(type))
            Subscribers[type] = new List<Action<IAttackLogEvent>>();
        Subscribers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : class, IAttackLogEvent
    {
        var dummy = (T)FormatterServices.GetUninitializedObject(typeof(T));
        var type = dummy.Type;
        if (!Subscribers.TryGetValue(type, out var handlers)) return;

        handlers.RemoveAll(wrapped =>
        {
            if (wrapped.Target is Action<IAttackLogEvent> wrapperAction)
                return wrapperAction == handler;
            return false;
        });
    }

    public static void Unsubscribe(AttackLogEventType type, Action<IAttackLogEvent> handler)
    {
        if (Subscribers.TryGetValue(type, out var handlers))
            handlers.Remove(handler);
    }

    public static void Publish(IAttackLogEvent evt)
    {
        if (!Subscribers.TryGetValue(evt.Type, out var handlers))
            return;

        for (int i = 0; i < handlers.Count; i++)
        {
            try
            {
                handlers[i](evt);
            }
            catch (Exception ex)
            {
                ModLogger.Log("EventBus", $"Handler error for {evt.Type}: {ex.Message}");
            }
        }
    }

    public static void Clear()
    {
        Subscribers.Clear();
    }
}
