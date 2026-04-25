using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

/// <summary>
/// 战斗生命周期服务，处理战斗开始/结束、回合开始/结束、
/// 房间日志和回合日志的创建与保存、以及中毒 Power 的回合递减
/// </summary>
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

    /// <summary>
    /// 战斗开始前：为每个玩家创建新的房间日志
    /// </summary>
    private void OnBeforeCombatStart(BeforeCombatStartEvent evt)
    {
        CreateNewRoomLog(evt.RunState, evt.CombatState);
    }

    /// <summary>
    /// 战斗结束后：保存房间日志到 RunLog，持久化存档
    /// </summary>
    private void OnAfterCombatEnd(AfterCombatEndEvent evt)
    {
        SaveRoomLog(evt.RunState, evt.CombatState);
        SaveLogState(evt.RunState, evt.CombatState);
    }

    /// <summary>
    /// 玩家回合开始前：创建新回合日志，递减中毒 Power 层数
    /// </summary>
    private void OnBeforeSideTurnStart(BeforeSideTurnStartEvent evt)
    {
        CreateNewTurnLog(evt.CombatState, evt.Side);
        UpdatePoisonPower(evt.CombatState, evt.Side);
    }

    /// <summary>
    /// 敌方回合结束后：保存回合日志到房间日志
    /// </summary>
    private void OnAfterTurnEnd(AfterTurnEndEvent evt)
    {
        SaveTurnLog(evt.CombatState, evt.Side);
    }

    /// <summary>
    /// 为每个玩家创建新的房间日志数据
    /// </summary>
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

    /// <summary>
    /// 战斗结束时保存房间日志：回合日志 → 房间日志 → Run 日志
    /// </summary>
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
                roomLogData.RoomLogSum.Accumulate(turnLogData.TurnLogSum);
            }

            if (roomLogData != null)
            {
                player.RunLogData.EnqueueRoomLogData(roomLogData);
                player.RunLogData.RunLogSum.Accumulate(roomLogData.RoomLogSum);
            }
        }

        State.OnCombatEnd();
    }

    /// <summary>
    /// 持久化存档数据
    /// </summary>
    private void SaveLogState(IRunState runState, CombatState combatState)
    {
        ModSaveUtils.Save(runState, combatState);
    }

    /// <summary>
    /// 为每个玩家创建新的回合日志数据（仅玩家回合）
    /// </summary>
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

    /// <summary>
    /// 递减所有怪物身上的中毒 Power 层数（玩家回合开始时触发）
    /// </summary>
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

    /// <summary>
    /// 敌方回合结束后保存回合日志到房间日志
    /// </summary>
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
                roomLogData?.RoomLogSum.Accumulate(turnLogData.TurnLogSum);
            }
        }

        State.TurnLogsData.Clear();
        State.InvalidateCache();
    }
}
