using System.Collections.Generic;
using System.Runtime.Serialization;
using AttackLog.Logger;

namespace AttackLog.Core;

/// <summary>
/// 事件总线，负责框架内所有事件的发布与订阅。
/// 支持泛型订阅（类型安全）和非泛型订阅两种方式。
/// </summary>
public static class AttackLogEventBus
{
    /// <summary>
    /// 事件类型 → 订阅者列表的映射
    /// </summary>
    private static readonly Dictionary<AttackLogEventType, List<Action<IAttackLogEvent>>> Subscribers = new();

    /// <summary>
    /// 泛型订阅。自动推断事件类型，处理器直接接收强类型事件参数。
    /// 内部使用 is 模式匹配进行安全类型转换，类型不匹配时输出 Warning 日志。
    /// </summary>
    /// <typeparam name="T">事件类型，必须实现 IAttackLogEvent 且为引用类型</typeparam>
    /// <param name="handler">强类型事件处理器</param>
    public static void Subscribe<T>(Action<T> handler) where T : class, IAttackLogEvent
    {
        // 通过 FormatterServices 创建未初始化实例以获取事件类型标识
        var dummy = (T)FormatterServices.GetUninitializedObject(typeof(T));
        var type = dummy.Type;

        if (!Subscribers.ContainsKey(type))
            Subscribers[type] = new List<Action<IAttackLogEvent>>();

        // 包装为 IAttackLogEvent 处理器，内部进行安全类型转换
        Subscribers[type].Add(e =>
        {
            if (e is T typed)
                handler(typed);
            else
                ModLogger.Warning("EventBus", $"Type mismatch: expected {typeof(T).Name}, got {e.GetType().Name}");
        });
    }

    /// <summary>
    /// 非泛型订阅。用于需要监听多种事件类型的场景，处理器接收 IAttackLogEvent 基接口
    /// </summary>
    /// <param name="type">要订阅的事件类型</param>
    /// <param name="handler">事件处理器</param>
    public static void Subscribe(AttackLogEventType type, Action<IAttackLogEvent> handler)
    {
        if (!Subscribers.ContainsKey(type))
            Subscribers[type] = new List<Action<IAttackLogEvent>>();
        Subscribers[type].Add(handler);
    }

    /// <summary>
    /// 泛型取消订阅。移除与指定处理器关联的订阅
    /// </summary>
    public static void Unsubscribe<T>(Action<T> handler) where T : class, IAttackLogEvent
    {
        var dummy = (T)FormatterServices.GetUninitializedObject(typeof(T));
        var type = dummy.Type;
        if (!Subscribers.TryGetValue(type, out var handlers)) return;

        // 移除与原始处理器匹配的包装器
        handlers.RemoveAll(wrapped =>
        {
            if (wrapped.Target is Action<IAttackLogEvent> wrapperAction)
                return wrapperAction == handler;
            return false;
        });
    }

    /// <summary>
    /// 非泛型取消订阅
    /// </summary>
    public static void Unsubscribe(AttackLogEventType type, Action<IAttackLogEvent> handler)
    {
        if (Subscribers.TryGetValue(type, out var handlers))
            handlers.Remove(handler);
    }

    /// <summary>
    /// 发布事件。按顺序调用所有订阅者，单个处理器异常不会中断其他订阅者执行
    /// </summary>
    /// <param name="evt">要发布的事件实例</param>
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
                ModLogger.Error("EventBus", $"Handler error for {evt.Type}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 清空所有订阅者
    /// </summary>
    public static void Clear()
    {
        Subscribers.Clear();
    }
}
