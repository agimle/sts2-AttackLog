namespace AttackLog.Model;

public class SingleTurnLogData
{
    /// <summary>
    /// 全部攻击数据
    /// </summary>
    public Queue<AttackLogModel> AttackLogs { get; set; }
    
    /// <summary>
    /// 当前回合全部攻击数据总和
    /// </summary>
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