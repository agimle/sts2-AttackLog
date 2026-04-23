namespace AttackLog.Model;

public class SingleRunLogData
{
    private const int MaxRoomHistory = 50;

    public Queue<SingleRoomLogData> RoomLogs { get; set; }

    public AttackLogModel RunLogSum { get; set; }

    public int TotalRoomCount { get; private set; }

    public SingleRunLogData()
    {
        RoomLogs = new Queue<SingleRoomLogData>();
        RunLogSum = new AttackLogModel();
    }

    public void EnqueueRoomLogData(SingleRoomLogData data)
    {
        TotalRoomCount++;
        RoomLogs.Enqueue(data);

        while (RoomLogs.Count > MaxRoomHistory)
        {
            RoomLogs.Dequeue();
        }
    }
}
