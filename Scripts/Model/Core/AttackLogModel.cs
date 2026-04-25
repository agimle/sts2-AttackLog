namespace AttackLog.Model;

public class AttackLogModel
{
    #region Attack

    public int DamageOnBlock { get; set; }

    public int DamageOnHp { get; set; }

    public int OverkillDamage { get; set; }

    public int TotalDamageDealt => DamageOnBlock + DamageOnHp + OverkillDamage;

    public int RealDamageDealt => DamageOnBlock + DamageOnHp;

    #endregion

    #region Defense

    public int ShieldGained { get; set; }
    public int EffectiveShield { get; set; }
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

    public void Accumulate(AttackLogModel other)
    {
        DamageOnBlock += other.DamageOnBlock;
        DamageOnHp += other.DamageOnHp;
        OverkillDamage += other.OverkillDamage;
        ShieldGained += other.ShieldGained;
        EffectiveShield += other.EffectiveShield;
        OverShield += other.OverShield;
    }

    public static AttackLogModel Sum(AttackLogModel a, AttackLogModel b)
    {
        AttackLogModel result = new AttackLogModel();
        result.Accumulate(a);
        result.Accumulate(b);
        return result;
    }

    public static AttackLogModel Sum(AttackLogModel a, AttackLogModel b, AttackLogModel c)
    {
        AttackLogModel result = new AttackLogModel();
        result.Accumulate(a);
        result.Accumulate(b);
        result.Accumulate(c);
        return result;
    }

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
