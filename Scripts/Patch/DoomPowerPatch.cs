using AttackLog.Core;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Patch;

/// <summary>
/// 末日之力（DoomPower）补丁，在 DoomKill 执行前发布事件。
/// 使用 Prefix 以确保在击杀逻辑之前捕获目标生物列表
/// </summary>
public static class DoomPowerPatch
{
    /// <summary>
    /// DoomKill 前置回调，发布 DoomKillEvent
    /// </summary>
    /// <param name="creatures">即将被末日之力击杀的生物列表</param>
    public static void DoomKillPrefix(IReadOnlyList<Creature> creatures)
    {
        AttackLogEventBus.Publish(new DoomKillEvent
        {
            Creatures = creatures
        });
    }
}
