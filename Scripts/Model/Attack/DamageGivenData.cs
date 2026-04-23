using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

public struct DamageGivenData
{
    /// <summary>
    /// 受到伤害的角色
    /// </summary>
    public Creature Receiver { get; set; }
    
    /// <summary>
    /// 被格挡的伤害
    /// </summary>
    public int BlockedDamage { get; set; }
    
    /// <summary>
    /// 未被阻挡的伤害
    /// </summary>
    public int UnblockedDamage { get; set; }
    
    /// <summary>
    /// 过量伤害
    /// </summary>
    public int OverkillDamage { get; set; }
    
    /// <summary>
    /// 总伤害
    /// </summary>
    public int TotalDamage =>  BlockedDamage + UnblockedDamage + OverkillDamage;
    
    /// <summary>
    /// 实际造成伤害
    /// </summary>
    public int RealDamage => BlockedDamage + UnblockedDamage;
    
    public DamageGivenData(DamageResult damageResult)
    {
        Receiver = damageResult.Receiver;
        BlockedDamage = damageResult.BlockedDamage;
        UnblockedDamage = damageResult.UnblockedDamage;
        OverkillDamage = damageResult.OverkillDamage;
    }
}