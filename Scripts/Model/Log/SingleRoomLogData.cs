namespace AttackLog.Model;

public class SingleRoomLogData
{
    private const int MaxTurnHistory = 100;

    public Queue<SingleTurnLogData> TurnLogs { get; set; }

    public AttackLogModel RoomLogSum { get; set; }

    public int TotalTurnCount { get; private set; }

    public SingleRoomLogData()
    {
        TurnLogs = new Queue<SingleTurnLogData>();
        RoomLogSum = new AttackLogModel();
    }

    public void EnqueueTurnLogData(SingleTurnLogData data)
    {
        TotalTurnCount++;
        TurnLogs.Enqueue(data);

        while (TurnLogs.Count > MaxTurnHistory)
        {
            TurnLogs.Dequeue();
        }
    }
}
