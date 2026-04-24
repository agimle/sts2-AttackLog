using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

public class AttackLogCombatService : AttackLogServiceBase
{
    public AttackLogCombatService(LogState state) : base(state) { }

    public override void Subscribe()
    {
        AttackLogEventBus.Subscribe<BeforeCombatStartEvent>(OnBeforeCombatStart);
        AttackLogEventBus.Subscribe<AfterCombatEndEvent>(OnAfterCombatEnd);
        AttackLogEventBus.Subscribe<BeforeSideTurnStartEvent>(OnBeforeSideTurnStart);
        AttackLogEventBus.Subscribe<AfterTurnEndEvent>(OnAfterTurnEnd);
    }

    public override void Unsubscribe()
    {
        AttackLogEventBus.Unsubscribe<BeforeCombatStartEvent>(OnBeforeCombatStart);
        AttackLogEventBus.Unsubscribe<AfterCombatEndEvent>(OnAfterCombatEnd);
        AttackLogEventBus.Unsubscribe<BeforeSideTurnStartEvent>(OnBeforeSideTurnStart);
        AttackLogEventBus.Unsubscribe<AfterTurnEndEvent>(OnAfterTurnEnd);
    }

    private void OnBeforeCombatStart(BeforeCombatStartEvent evt)
    {
        CreateNewRoomLog(evt.RunState, evt.CombatState);
    }

    private void OnAfterCombatEnd(AfterCombatEndEvent evt)
    {
        SaveRoomLog(evt.RunState, evt.CombatState);
        SaveLogState(evt.RunState, evt.CombatState);
    }

    private void OnBeforeSideTurnStart(BeforeSideTurnStartEvent evt)
    {
        CreateNewTurnLog(evt.CombatState, evt.Side);
        UpdatePoisonPower(evt.CombatState, evt.Side);
    }

    private void OnAfterTurnEnd(AfterTurnEndEvent evt)
    {
        SaveTurnLog(evt.CombatState, evt.Side);
    }

    private void CreateNewRoomLog(IRunState runState, CombatState combatState)
    {
        if (State.RunLog is null) return;
        if (!State.RunLog.IsSameRun(runState)) return;

        State.OnCombatStart();

        foreach (var player in State.RunLog.GetAllPlayers())
        {
            SingleRoomLogData newRoomLogData = new SingleRoomLogData();
            State.RoomLogsData.TryAdd(player.PlayerInfo, newRoomLogData);
        }

        State.InvalidateCache();
    }

    private void SaveRoomLog(IRunState runState, CombatState combatState)
    {
        if (State.RunLog == null) return;
        if (!State.RunLog.IsSameRun(runState)) return;

        foreach (var player in State.RunLog.GetAllPlayers())
        {
            State.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            State.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);
            if (turnLogData != null && roomLogData != null)
            {
                roomLogData.EnqueueTurnLogData(turnLogData);
                roomLogData.RoomLogSum.Add(turnLogData.TurnLogSum);
            }

            if (roomLogData != null)
            {
                player.RunLogData.EnqueueRoomLogData(roomLogData);
                player.RunLogData.RunLogSum.Add(roomLogData.RoomLogSum);
            }
        }

        State.OnCombatEnd();
    }

    private void SaveLogState(IRunState runState, CombatState combatState)
    {
        ModSaveUtils.Save(runState, combatState);
    }

    private void CreateNewTurnLog(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        if (State.RunLog is null) return;
        if (!State.RunLog.IsSameRun(combatState.RunState)) return;

        State.TurnLogsData.Clear();

        foreach (var player in State.RunLog.GetAllPlayers())
        {
            SingleTurnLogData newTurnLogData = new SingleTurnLogData();
            State.TurnLogsData.TryAdd(player.PlayerInfo, newTurnLogData);
        }

        State.InvalidateCache();
    }

    private void UpdatePoisonPower(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player) return;

        foreach (var monsterRecord in State.CombatRecord.MonsterRecords)
        {
            foreach (var powerRecord in monsterRecord.Powers)
            {
                if (powerRecord.Key == typeof(PoisonPower))
                {
                    if (powerRecord.Value.Count == 0) continue;

                    powerRecord.Value.Peek().Amount--;

                    if (powerRecord.Value.Peek().Amount <= 0)
                    {
                        powerRecord.Value.Dequeue();
                    }
                }
            }
        }
    }

    private void SaveTurnLog(CombatState combatState, CombatSide side)
    {
        if (side == CombatSide.Player) return;

        if (State.RunLog == null) return;
        if (!State.RunLog.IsSameRun(combatState.RunState)) return;

        foreach (var player in State.RunLog.GetAllPlayers())
        {
            State.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            State.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);

            if (turnLogData != null)
            {
                roomLogData?.EnqueueTurnLogData(turnLogData);
                roomLogData?.RoomLogSum.Add(turnLogData.TurnLogSum);
            }
        }

        State.TurnLogsData.Clear();
        State.InvalidateCache();
    }
}
