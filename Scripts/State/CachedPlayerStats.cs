using AttackLog.Model;

namespace AttackLog.State;

public class CachedPlayerStats
{
    public AttackLogModel? TurnLog { get; set; }
    public AttackLogModel? RoomLog { get; set; }
    public AttackLogModel? RunLog { get; set; }
    public float DamagePercent { get; set; }
    public int TotalDamage { get; set; }

    private string? _percentText;
    private string? _runDamageText;
    private string? _roomDamageText;
    private string? _turnDamageText;
    private float _cachedPercent = -1;
    private int _cachedRunDamage = -1;
    private int _cachedRoomDamage = -1;
    private int _cachedTurnDamage = -1;

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
