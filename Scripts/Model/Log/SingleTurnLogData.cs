namespace AttackLog.Model;

/// <summary>
/// 单回合日志数据，记录一个回合内的所有攻击记录和汇总。
/// 攻击记录上限 200 条，超出时自动淘汰最旧记录。
/// </summary>
public class SingleTurnLogData
{
    /// <summary>攻击记录历史上限</summary>
    private const int MaxAttackHistory = 200;

    /// <summary>攻击记录队列</summary>
    public Queue<AttackLogModel> AttackLogs { get; set; }

    /// <summary>回合汇总数据</summary>
    public AttackLogModel TurnLogSum { get; set; }

    /// <summary>总攻击次数（不受队列淘汰影响）</summary>
    public int TotalAttackCount { get; private set; }

    public SingleTurnLogData()
    {
        AttackLogs = new Queue<AttackLogModel>();
        TurnLogSum = new AttackLogModel();
    }

    /// <summary>
    /// 入队攻击记录，超过上限时自动淘汰最旧记录
    /// </summary>
    /// <param name="model">攻击日志模型</param>
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
