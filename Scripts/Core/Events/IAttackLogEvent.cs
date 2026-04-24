namespace AttackLog.Core;

public interface IAttackLogEvent
{
    AttackLogEventType Type { get; }
}
