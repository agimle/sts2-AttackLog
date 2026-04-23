namespace AttackLog.Model;

public class SingleRunLogData
{
    public Queue<SingleRoomLogData> RoomLogs { get; set; }

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
