using AttackLog.State;

namespace AttackLog.Core;

public abstract class AttackLogServiceBase : IAttackLogService
{
    protected readonly LogState State;

    protected AttackLogServiceBase(LogState state)
    {
        State = state;
    }

    public abstract void Subscribe();
    public abstract void Unsubscribe();
}
