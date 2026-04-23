using AttackLog.Service;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AttackLog.Patch;

public static class DoomPowerPatch
{
    public static void DoomKillPrefix(IReadOnlyList<Creature> creatures)
    {
        DoomKillService.HandleDoomKill(creatures);
    }
}