using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

/// <summary>
/// Run 生命周期服务，处理新 Run 开始时的初始化逻辑：
/// 创建 RunLog、注册玩家、加载存档数据
/// </summary>
public class AttackLogRunService : AttackLogServiceBase
{
    public AttackLogRunService(LogState state) : base(state) { }

    public override void Subscribe()
    {
        AttackLogEventBus.Subscribe<RunStartedEvent>(OnRunStarted);
    }

    public override void Unsubscribe()
    {
        AttackLogEventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
    }

    /// <summary>
    /// Run 开始事件处理：创建新 Run、注册玩家、加载存档
    /// </summary>
    private void OnRunStarted(RunStartedEvent evt)
    {
        CreateNewRun(evt.RunState);
        RegisterPlayers(evt.RunState);
        LoadLogState(evt.RunState);
    }

    /// <summary>
    /// 创建新的 RunLog，仅在 Run 变更时创建
    /// </summary>
    /// <param name="runState">当前 RunState</param>
    private void CreateNewRun(IRunState runState)
    {
        if (State.RunLog is null || !State.RunLog.IsSameRun(runState))
        {
            State.RunLog = new RunLog(runState);
            State.InvalidateCache();
        }
    }

    /// <summary>
    /// 注册所有玩家到 RunLog，维护双索引
    /// </summary>
    /// <param name="runState">当前 RunState</param>
    private void RegisterPlayers(IRunState runState)
    {
        if (State.RunLog is null || !State.RunLog.IsSameRun(runState))
        {
            CreateNewRun(runState);
        }

        foreach (var player in runState.Players)
        {
            State.RunLog?.RegisterPlayer(player);
        }

        State.InvalidateCache();
    }

    /// <summary>
    /// 从存档加载日志状态
    /// </summary>
    /// <param name="runState">当前 RunState</param>
    private void LoadLogState(IRunState runState)
    {
        ModSaveUtils.Load(runState);
    }
}
