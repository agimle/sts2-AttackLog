namespace AttackLog.Core;

/// <summary>
/// 事件基础接口，所有框架事件必须实现此接口并提供事件类型标识
/// </summary>
public interface IAttackLogEvent
{
    /// <summary>
    /// 事件类型标识，用于 EventBus 路由到对应的订阅者
    /// </summary>
    AttackLogEventType Type { get; }
}
