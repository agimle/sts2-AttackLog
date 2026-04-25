namespace AttackLog.Model;

/// <summary>
/// 单次 Run 日志数据，记录整个 Run 的所有房间记录和汇总。
/// 房间记录上限 50 条，超出时自动淘汰最旧记录。
/// 此类同时用于存档序列化（通过 NetId 索引）。
/// </summary>
public class SingleRunLogData
{
    /// <summary>房间记录历史上限</summary>
    private const int MaxRoomHistory = 50;

    /// <summary>房间记录队列</summary>
    public Queue<SingleRoomLogData> RoomLogs { get; set; }

    /// <summary>Run 汇总数据</summary>
    public AttackLogModel RunLogSum { get; set; }

    /// <summary>总房间数（不受队列淘汰影响）</summary>
    public int TotalRoomCount { get; private set; }

    public SingleRunLogData()
    {
        RoomLogs = new Queue<SingleRoomLogData>();
        RunLogSum = new AttackLogModel();
    }

    /// <summary>
    /// 入队房间记录，超过上限时自动淘汰最旧记录
    /// </summary>
    /// <param name="data">房间日志数据</param>
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
