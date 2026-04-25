namespace AttackLog.Core;

/// <summary>
/// 服务基础接口，所有 Service 必须实现订阅与取消订阅生命周期
/// </summary>
public interface IAttackLogService
{
    /// <summary>
    /// 订阅事件总线中的事件
    /// </summary>
    void Subscribe();

    /// <summary>
    /// 取消订阅事件总线中的事件
    /// </summary>
    void Unsubscribe();
}
