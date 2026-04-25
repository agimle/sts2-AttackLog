namespace AttackLog.Model;

/// <summary>
/// 核心统计模型，记录单次攻击/防御的伤害数据。
/// 支持 Accumulate 累加和 Sum 求和操作，用于回合/房间/Run 级别的数据汇总。
/// </summary>
public class AttackLogModel
{
    #region Attack

    /// <summary>打在护盾上的伤害</summary>
    public int DamageOnBlock { get; set; }

    /// <summary>打在血量上的伤害</summary>
    public int DamageOnHp { get; set; }

    /// <summary>过量击杀伤害</summary>
    public int OverkillDamage { get; set; }

    /// <summary>
    /// 毒造成的伤害
    /// </summary>
    public int PoisonDamage { get; set; }
    
    /// <summary>
    /// 灾厄造成的伤害
    /// </summary>
    public int DoomDamage { get; set; }
    
    /// <summary>
    /// 普通伤害
    /// </summary>
    public int CommonDamage => DamageOnHp - PoisonDamage - DoomDamage;
    
    /// <summary>总伤害（格挡 + 血量 + 过量）</summary>
    public int TotalDamageDealt => DamageOnBlock + DamageOnHp + OverkillDamage;

    /// <summary>实际伤害（格挡 + 血量，不含过量）</summary>
    public int RealDamageDealt => DamageOnBlock + DamageOnHp;

    #endregion

    #region Defense

    /// <summary>获得的护盾值</summary>
    public int ShieldGained { get; set; }

    /// <summary>有效护盾值</summary>
    public int EffectiveShield { get; set; }

    /// <summary>超额护盾值</summary>
    public int OverShield { get; set; }

    #endregion

    public AttackLogModel()
    {
        DamageOnBlock = 0;
        DamageOnHp = 0;
        OverkillDamage = 0;
        PoisonDamage = 0;
        DoomDamage = 0;
        ShieldGained = 0;
        EffectiveShield = 0;
        OverShield = 0;
    }

    /// <summary>
    /// 将另一个模型的数值累加到当前实例
    /// </summary>
    /// <param name="other">要累加的源模型</param>
    public void Accumulate(AttackLogModel other)
    {
        DamageOnBlock += other.DamageOnBlock;
        DamageOnHp += other.DamageOnHp;
        OverkillDamage += other.OverkillDamage;
        PoisonDamage += other.PoisonDamage;
        DoomDamage += other.DoomDamage;
        ShieldGained += other.ShieldGained;
        EffectiveShield += other.EffectiveShield;
        OverShield += other.OverShield;
    }

    /// <summary>
    /// 计算两个模型的和，返回新实例
    /// </summary>
    public static AttackLogModel Sum(AttackLogModel a, AttackLogModel b)
    {
        AttackLogModel result = new AttackLogModel();
        result.Accumulate(a);
        result.Accumulate(b);
        return result;
    }

    /// <summary>
    /// 计算三个模型的和，返回新实例
    /// </summary>
    public static AttackLogModel Sum(AttackLogModel a, AttackLogModel b, AttackLogModel c)
    {
        AttackLogModel result = new AttackLogModel();
        result.Accumulate(a);
        result.Accumulate(b);
        result.Accumulate(c);
        return result;
    }

    /// <summary>
    /// 计算多个模型的和，返回新实例
    /// </summary>
    public static AttackLogModel Sum(params AttackLogModel[] logs)
    {
        AttackLogModel result = new AttackLogModel();
        foreach (var log in logs)
        {
            result.Accumulate(log);
        }
        return result;
    }
}
