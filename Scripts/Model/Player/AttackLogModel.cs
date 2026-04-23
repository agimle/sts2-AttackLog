namespace AttackLog.Model;

public class AttackLogModel
{
    #region 攻击部分

    /// <summary>
    /// 打在护盾上的伤害
    /// </summary>
    public int DamageOnBlock { get; set; }

    /// <summary>
    /// 打在血上的伤害
    /// </summary>
    public int DamageOnHp { get; set; }

    /// <summary>
    /// 过量的伤害
    /// </summary>
    public int OverkillDamage { get; set; }

    /// <summary>
    /// 全部的伤害
    /// </summary>
    public int TotalDamageDealt => DamageOnBlock + DamageOnHp + OverkillDamage;

    /// <summary>
    /// 实际造成的伤害
    /// </summary>
    public int RealDamageDealt => DamageOnBlock + DamageOnHp;

    #endregion
    
    // ------------------------------------------------------------
    
    #region 防御部分
    
    /// <summary>
    /// 获得的护盾
    /// </summary>
    public int ShieldGained { get; set; }
    /// <summary>
    /// 有效护盾
    /// </summary>
    public int EffectiveShield { get; set; }
    /// <summary>
    /// 过量护盾
    /// </summary>
    public int OverShield { get; set; }
    
    #endregion
    
    
    
    
    public AttackLogModel()
    {
        DamageOnBlock = 0;
        DamageOnHp = 0;
        OverkillDamage = 0;
        ShieldGained = 0;
        EffectiveShield = 0;
        OverShield = 0;
    }
    
    /// <summary>
    /// 加法
    /// </summary>
    /// <param name="other"></param>
    public void Plus(AttackLogModel other)
    {
        DamageOnBlock += other.DamageOnBlock;
        DamageOnHp += other.DamageOnHp;
        OverkillDamage += other.OverkillDamage;
        ShieldGained += other.ShieldGained;
        EffectiveShield += other.EffectiveShield;
        OverShield += other.OverShield;
    }

    /// <summary>
    /// 统计两个
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static AttackLogModel Sum(AttackLogModel a, AttackLogModel b)
    {
        AttackLogModel result = new AttackLogModel();
        result.Plus(a);
        result.Plus(b);
        return result;
    }
    public static AttackLogModel Sum(AttackLogModel a,AttackLogModel b,AttackLogModel c)
    {
        AttackLogModel result = new AttackLogModel();
        result.Plus(a);
        result.Plus(b);
        result.Plus(c);
        return result;
    }
    public static AttackLogModel Sum(params AttackLogModel[] logs)
    {
        AttackLogModel result = new AttackLogModel();
        foreach (var log in logs)
        {
            result.Plus(log);
        }
        return result;
    }
}