using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

public class AttackLogCombatService : IAttackLogService
{
    public void Subscribe()
    {
        AttackLogEventBus.Subscribe<BeforeCombatStartEvent>(OnBeforeCombatStart);
        AttackLogEventBus.Subscribe<AfterCombatEndEvent>(OnAfterCombatEnd);
        AttackLogEventBus.Subscribe<BeforeSideTurnStartEvent>(OnBeforeSideTurnStart);
        AttackLogEventBus.Subscribe<AfterTurnEndEvent>(OnAfterTurnEnd);
    }

    public void Unsubscribe()
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
        if (LogState.Instance.RunLog is null) return;
        if (!LogState.Instance.RunLog.IsSameRun(runState)) return;

        LogState.Instance.OnCombatStart();

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            SingleRoomLogData newRoomLogData = new SingleRoomLogData();
            LogState.Instance.RoomLogsData.TryAdd(player.PlayerInfo, newRoomLogData);
        }

        LogState.Instance.InvalidateCache();
    }

    private void SaveRoomLog(IRunState runState, CombatState combatState)
    {
        if (LogState.Instance.RunLog == null) return;
        if (!LogState.Instance.RunLog.IsSameRun(runState)) return;

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            LogState.Instance.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);
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

        LogState.Instance.OnCombatEnd();
    }

    private void SaveLogState(IRunState runState, CombatState combatState)
    {
        ModSaveUtils.Save(runState, combatState);
    }

    private void CreateNewTurnLog(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        if (LogState.Instance.RunLog is null) return;
        if (!LogState.Instance.RunLog.IsSameRun(combatState.RunState)) return;

        LogState.Instance.TurnLogsData.Clear();

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            SingleTurnLogData newTurnLogData = new SingleTurnLogData();
            LogState.Instance.TurnLogsData.TryAdd(player.PlayerInfo, newTurnLogData);
        }

        LogState.Instance.InvalidateCache();
    }

    private void UpdatePoisonPower(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player) return;

        foreach (var monsterRecord in LogState.Instance.CombatRecord.MonsterRecords)
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

        if (LogState.Instance.RunLog == null) return;
        if (!LogState.Instance.RunLog.IsSameRun(combatState.RunState)) return;

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            LogState.Instance.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);

            if (turnLogData != null)
            {
                roomLogData?.EnqueueTurnLogData(turnLogData);
                roomLogData?.RoomLogSum.Add(turnLogData.TurnLogSum);
            }
        }

        LogState.Instance.TurnLogsData.Clear();
        LogState.Instance.InvalidateCache();
    }
}
