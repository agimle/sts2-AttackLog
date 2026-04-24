using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

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

    private void OnRunStarted(RunStartedEvent evt)
    {
        CreateNewRun(evt.RunState);
        RegisterPlayers(evt.RunState);
        LoadLogState(evt.RunState);
    }

    private void CreateNewRun(IRunState runState)
    {
        if (State.RunLog is null || !State.RunLog.IsSameRun(runState))
        {
            State.RunLog = new RunLog(runState);
            State.InvalidateCache();
        }
    }

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

    private void LoadLogState(IRunState runState)
    {
        ModSaveUtils.Load(runState);
    }
}
