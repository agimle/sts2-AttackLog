using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Model;

/// <summary>
/// 战斗记录，用于记录部分信息
/// Power
/// </summary>
public class CombatLog
{
    // 记录怪物
    public Dictionary<Creature,MonsterPowerRecord> MonsterPowerRecords { get; set; } = new();
    
    // 战斗中记录 PoisonPower 和 DoomPower
    
}