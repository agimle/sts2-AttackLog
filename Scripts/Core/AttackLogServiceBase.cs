using AttackLog.State;

namespace AttackLog.Core;

/// <summary>
/// 服务抽象基类，通过构造函数注入 LogState 实现依赖倒置
/// </summary>
public abstract class AttackLogServiceBase : IAttackLogService
{
    /// <summary>
    /// 注入的 LogState 实例，子类通过此字段访问状态，不再直接使用 LogState.Instance
    /// </summary>
    protected readonly LogState State;

    protected AttackLogServiceBase(LogState state)
    {
        State = state;
    }

    public abstract void Subscribe();
    public abstract void Unsubscribe();
}
