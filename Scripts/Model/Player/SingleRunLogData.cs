namespace AttackLog.Model;

public class SingleRunLogData
{
    /// <summary>
    /// 全部房间数据
    /// </summary>
    public Queue<SingleRoomLogData> RoomLogs { get; set; }
    
    /// <summary>
    /// 整轮游戏数据总和
    /// </summary>
    public AttackLogModel RunLogSum { get; set; }
    
    public SingleRunLogData()
    {
        RoomLogs = new Queue<SingleRoomLogData>();
        RunLogSum = new AttackLogModel();
    }
    
    public void EnqueueRoomLogData(SingleRoomLogData data)
    {
        RoomLogs.Enqueue(data);
    }
}