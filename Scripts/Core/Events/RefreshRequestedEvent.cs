namespace AttackLog.Core;

public sealed class RefreshRequestedEvent : IAttackLogEvent
{
    public AttackLogEventType Type => AttackLogEventType.RefreshRequested;
}
