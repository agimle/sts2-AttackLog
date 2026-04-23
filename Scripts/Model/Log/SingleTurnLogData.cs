namespace AttackLog.Model;

public class SingleTurnLogData
{
    private const int MaxAttackHistory = 200;

    public Queue<AttackLogModel> AttackLogs { get; set; }

    public AttackLogModel TurnLogSum { get; set; }

    public int TotalAttackCount { get; private set; }

    public SingleTurnLogData()
    {
        AttackLogs = new Queue<AttackLogModel>();
        TurnLogSum = new AttackLogModel();
    }

    public void EnqueueAttackLogData(AttackLogModel model)
    {
        TotalAttackCount++;
        AttackLogs.Enqueue(model);

        while (AttackLogs.Count > MaxAttackHistory)
        {
            AttackLogs.Dequeue();
        }
    }
}
