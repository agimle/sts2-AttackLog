using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using AttackLog.View;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

public class AttackLogRunService : IAttackLogService
{
    public void Subscribe()
    {
        AttackLogEventBus.Subscribe<RunStartedEvent>(OnRunStarted);
    }

    public void Unsubscribe()
    {
        AttackLogEventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
    }

    private void OnRunStarted(RunStartedEvent evt)
    {
        CreateNewRun(evt.RunState);
        RegisterPlayers(evt.RunState);
        LoadLogState(evt.RunState);
    }

    private void CreateNewRun(IRunState runState)
    {
        if (LogState.Instance.RunLog is null || !LogState.Instance.RunLog.IsSameRun(runState))
        {
            LogState.Instance.RunLog = new RunLog(runState);
            LogState.Instance.InvalidateCache();
        }
    }

    private void RegisterPlayers(IRunState runState)
    {
        if (LogState.Instance.RunLog is null || !LogState.Instance.RunLog.IsSameRun(runState))
        {
            CreateNewRun(runState);
        }

        foreach (var player in runState.Players)
        {
            LogState.Instance.RunLog?.RegisterPlayer(player);
        }

        LogState.Instance.InvalidateCache();
    }

    private void LoadLogState(IRunState runState)
    {
        ModSaveUtils.Load(runState);
    }
}
