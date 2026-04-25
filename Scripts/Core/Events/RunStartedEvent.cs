using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Core;

/// <summary>
/// 新 Run 开始时触发的事件
/// </summary>
public sealed class RunStartedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.RunStarted;

    /// <summary>当前 Run 的状态</summary>
    public required IRunState RunState { get; init; }
}
