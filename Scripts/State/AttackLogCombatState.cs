using AttackLog.Model;

namespace AttackLog.State;

public class AttackLogCombatState
{
    public CombatRecord CombatRecord { get; set; } = new();
    public Dictionary<PlayerInfo, SingleRoomLogData> RoomLogsData { get; set; } = new();
    public Dictionary<PlayerInfo, SingleTurnLogData> TurnLogsData { get; set; } = new();

    public void OnCombatStart()
    {
        RoomLogsData.Clear();
        CombatRecord.Clear();
    }

    public void OnCombatEnd()
    {
        TurnLogsData.Clear();
        RoomLogsData.Clear();
    }
}
