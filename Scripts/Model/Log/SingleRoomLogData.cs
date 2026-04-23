namespace AttackLog.Model;

public class SingleRoomLogData
{
    public Queue<SingleTurnLogData> TurnLogs { get; set; }

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
