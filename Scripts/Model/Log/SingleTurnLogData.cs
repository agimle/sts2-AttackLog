namespace AttackLog.Model;

public class SingleTurnLogData
{
    public Queue<AttackLogModel> AttackLogs { get; set; }

    public AttackLogModel TurnLogSum { get; set; }

    public SingleTurnLogData()
    {
        AttackLogs = new Queue<AttackLogModel>();
        TurnLogSum = new AttackLogModel();
    }

    public void EnqueueAttackLogData(AttackLogModel model)
    {
        AttackLogs.Enqueue(model);
    }
}
