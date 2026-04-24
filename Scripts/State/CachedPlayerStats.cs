using AttackLog.Model;

namespace AttackLog.State;

public class CachedPlayerStats
{
    public AttackLogModel? TurnLog { get; set; }
    public AttackLogModel? RoomLog { get; set; }
    public AttackLogModel? RunLog { get; set; }
    public float DamagePercent { get; set; }
    public int TotalDamage { get; set; }
}
