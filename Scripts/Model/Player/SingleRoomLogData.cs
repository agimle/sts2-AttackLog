namespace AttackLog.Model;

public class SingleRoomLogData
{
    /// <summary>
    /// 全部回合数据
    /// </summary>
    public Queue<SingleTurnLogData> TurnLogs { get; set; }
    
    /// <summary>
    /// 当前房间全部回合数据总和
    /// </summary>
    public AttackLogModel RoomLogSum { get; set; }
    
    public SingleRoomLogData()
    {
        TurnLogs = new Queue<SingleTurnLogData>();
        RoomLogSum = new AttackLogModel();
    }
    
    public void EnqueueTurnLogData(SingleTurnLogData data)
    {
        TurnLogs.Enqueue(data);
    }
}