using AttackLog.Model;
using MegaCrit.Sts2.Core.Runs;


namespace AttackLog.Save;

public class ModSave
{
    /// <summary>
    /// 字符串种子，原样保存，方便玩家查看和修改
    /// </summary>
    public string StringSeed { get; set; } = "";

    /// <summary>
    /// uint型种子
    /// </summary>
    public uint Seed { get; set; }

    /// <summary>
    /// 玩家Id -> 玩家数据
    /// </summary>
    public Dictionary<ulong, SingleRunLogData> PlayerDataDict { get; set; } = new();

    /// <summary>
    /// 主面板位置 X
    /// </summary>
    public float MainPanelX { get; set; } = -1f;

    /// <summary>
    /// 主面板位置 Y
    /// </summary>
    public float MainPanelY { get; set; } = -1f;

    /// <summary>
    /// 伤害统计面板位置 X
    /// </summary>
    public float DamageStatsPanelX { get; set; } = -1f;

    /// <summary>
    /// 伤害统计面板位置 Y
    /// </summary>
    public float DamageStatsPanelY { get; set; } = -1f;

    /// <summary>
    /// 无参构造函数（反序列化用）
    /// </summary>
    public ModSave() { }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="runState"></param>
    public ModSave(IRunState runState)
    {
        StringSeed = runState.Rng.StringSeed;
        Seed = runState.Rng.Seed;
        PlayerDataDict = new Dictionary<ulong, SingleRunLogData>();
    }
}
