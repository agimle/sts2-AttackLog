using System.Collections.Generic;
using AttackLog.Logger;

namespace AttackLog.Core;

public static class AttackLogEventBus
{
    private static readonly Dictionary<AttackLogEventType, List<Action<IAttackLogEvent>>> Subscribers = new();

    public static void Subscribe(AttackLogEventType type, Action<IAttackLogEvent> handler)
    {
        if (!Subscribers.ContainsKey(type))
            Subscribers[type] = new List<Action<IAttackLogEvent>>();
        Subscribers[type].Add(handler);
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
