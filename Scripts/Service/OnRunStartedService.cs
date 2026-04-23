using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using AttackLog.View;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

public static class OnRunStartedService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.RunStarted, OnRunStarted);
    }

    private static void OnRunStarted(IAttackLogEvent e)
    {
        var evt = (RunStartedEvent)e;
        CreateNewRun(evt.RunState);
        RegisterPlayers(evt.RunState);
        LoadLogState(evt.RunState);

        if (!LogState.Instance.IsLogPanelCreated)
        {
            AttackLogPanel.EnsureCreated();
        }
    }

    private static void CreateNewRun(IRunState runState)
    {
        if (LogState.Instance.RunLog is null || !LogState.Instance.RunLog.IsSameRun(runState))
        {
            LogState.Instance.RunLog = new RunLog(runState);
            LogState.Instance.InvalidateCache();
        }
    }

    private static void RegisterPlayers(IRunState runState)
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

    private static void LoadLogState(IRunState runState)
    {
        ModSaveUtils.Load(runState);
    }
}
