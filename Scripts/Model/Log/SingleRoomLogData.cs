namespace AttackLog.Model;

/// <summary>
/// 单房间日志数据，记录一个房间内的所有回合记录和汇总。
/// 回合记录上限 100 条，超出时自动淘汰最旧记录。
/// </summary>
public class SingleRoomLogData
{
    /// <summary>回合记录历史上限</summary>
    private const int MaxTurnHistory = 100;

    /// <summary>回合记录队列</summary>
    public Queue<SingleTurnLogData> TurnLogs { get; set; }

    /// <summary>房间汇总数据</summary>
    public AttackLogModel RoomLogSum { get; set; }

    /// <summary>总回合数（不受队列淘汰影响）</summary>
    public int TotalTurnCount { get; private set; }

    public SingleRoomLogData()
    {
        TurnLogs = new Queue<SingleTurnLogData>();
        RoomLogSum = new AttackLogModel();
    }

    /// <summary>
    /// 入队回合记录，超过上限时自动淘汰最旧记录
    /// </summary>
    /// <param name="data">回合日志数据</param>
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
