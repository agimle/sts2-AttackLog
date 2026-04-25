namespace AttackLog.Core;

/// <summary>
/// 请求刷新 UI 的事件，由 Panel 外部触发以通知面板重新渲染
/// </summary>
public sealed class RefreshRequestedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.RefreshRequested;
}
