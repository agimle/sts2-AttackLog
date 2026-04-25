using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

/// <summary>
/// 伤害数据结构体，从 DamageResult 提取并缓存伤害分布信息。
/// 作为 PowerDamageCalculateUtils 的输入，用于按 Power 施加顺序归因伤害。
/// </summary>
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
    /// 未被阻挡的伤害（打血量）
    /// </summary>
    public int UnblockedDamage { get; set; }

    /// <summary>
    /// 过量伤害（击杀后的溢出伤害）
    /// </summary>
    public int OverkillDamage { get; set; }

    /// <summary>
    /// 总伤害（格挡 + 血量 + 过量）
    /// </summary>
    public int TotalDamage =>  BlockedDamage + UnblockedDamage + OverkillDamage;

    /// <summary>
    /// 实际造成伤害（格挡 + 血量，不含过量）
    /// </summary>
    public int RealDamage => BlockedDamage + UnblockedDamage;

    /// <summary>
    /// 从游戏原生 DamageResult 构造
    /// </summary>
    /// <param name="damageResult">游戏伤害结果</param>
    public DamageGivenData(DamageResult damageResult)
    {
        Receiver = damageResult.Receiver;
        BlockedDamage = damageResult.BlockedDamage;
        UnblockedDamage = damageResult.UnblockedDamage;
        OverkillDamage = damageResult.OverkillDamage;
    }
}
