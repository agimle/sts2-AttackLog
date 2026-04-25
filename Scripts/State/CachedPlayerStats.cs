using AttackLog.Model;

namespace AttackLog.State;

/// <summary>
/// 玩家缓存统计数据，包含各维度的伤害汇总和格式化文本。
/// 文本属性使用脏标记模式，仅在数值变化时重新生成字符串，避免 GC 压力。
/// </summary>
public class CachedPlayerStats
{
    /// <summary>回合级伤害汇总</summary>
    public AttackLogModel? TurnLog { get; set; }

    /// <summary>房间级伤害汇总</summary>
    public AttackLogModel? RoomLog { get; set; }

    /// <summary>Run 级伤害汇总</summary>
    public AttackLogModel? RunLog { get; set; }

    /// <summary>伤害占比百分比</summary>
    public float DamagePercent { get; set; }

    /// <summary>所有玩家总伤害</summary>
    public int TotalDamage { get; set; }

    /// <summary>百分比文本缓存</summary>
    private string? _percentText;

    /// <summary>Run 伤害文本缓存</summary>
    private string? _runDamageText;

    /// <summary>房间伤害文本缓存</summary>
    private string? _roomDamageText;

    /// <summary>回合伤害文本缓存</summary>
    private string? _turnDamageText;

    /// <summary>上次缓存时的百分比值</summary>
    private float _cachedPercent = -1;

    /// <summary>上次缓存时的 Run 伤害值</summary>
    private int _cachedRunDamage = -1;

    /// <summary>上次缓存时的房间伤害值</summary>
    private int _cachedRoomDamage = -1;

    /// <summary>上次缓存时的回合伤害值</summary>
    private int _cachedTurnDamage = -1;

    /// <summary>
    /// 百分比文本，格式为 "xx.x%"，仅在数值变化时重新生成
    /// </summary>
    public string PercentText
    {
        get
        {
            if (_cachedPercent.Equals(DamagePercent) && _percentText != null)
                return _percentText;
            _cachedPercent = DamagePercent;
            return _percentText = $"{DamagePercent:F1}%";
        }
    }

    /// <summary>
    /// Run 伤害文本，仅在数值变化时重新生成
    /// </summary>
    public string RunDamageText
    {
        get
        {
            int damage = RunLog?.RealDamageDealt ?? 0;
            if (_cachedRunDamage == damage && _runDamageText != null)
                return _runDamageText;
            _cachedRunDamage = damage;
            return _runDamageText = damage.ToString();
        }
    }

    /// <summary>
    /// 房间伤害文本，仅在数值变化时重新生成
    /// </summary>
    public string RoomDamageText
    {
        get
        {
            int damage = RoomLog?.RealDamageDealt ?? 0;
            if (_cachedRoomDamage == damage && _roomDamageText != null)
                return _roomDamageText;
            _cachedRoomDamage = damage;
            return _roomDamageText = damage.ToString();
        }
    }

    /// <summary>
    /// 回合伤害文本，仅在数值变化时重新生成
    /// </summary>
    public string TurnDamageText
    {
        get
        {
            int damage = TurnLog?.RealDamageDealt ?? 0;
            if (_cachedTurnDamage == damage && _turnDamageText != null)
                return _turnDamageText;
            _cachedTurnDamage = damage;
            return _turnDamageText = damage.ToString();
        }
    }
}
