using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.Patch;
using AttackLog.Service;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog;

[ModInitializer("Init")]
public class ModEntry
{
    private static Harmony? _harmony;

    public static void Init()
    {
        if (_harmony != null)
        {
            return;
        }
        _harmony = new Harmony("com.agimle.sts2.attack_log");

        SubscribeAllServices();

        HookPatches.OnRunStartPostfix();

        PatchUtils.PatchHook(_harmony, nameof(Hook.BeforeCombatStart), nameof(HookPatches.BeforeCombatStartPostfix));

        PatchUtils.PatchHook(_harmony, nameof(Hook.BeforeSideTurnStart), nameof(HookPatches.BeforeSideTurnStartPostfix));

        PatchUtils.PatchHook(_harmony, nameof(Hook.AfterTurnEnd), nameof(HookPatches.AfterTurnEndPostfix));

        PatchUtils.PatchHook(_harmony, nameof(Hook.AfterCombatEnd), nameof(HookPatches.AfterCombatEndPostfix));

        PatchUtils.PatchHook(_harmony, nameof(Hook.AfterDamageGiven), nameof(HookPatches.AfterDamageGivenPostfix));

        PatchUtils.PatchHook(_harmony, nameof(Hook.AfterDeath), nameof(HookPatches.AfterDeathPostfix));

        PatchUtils.PatchMethod(_harmony, typeof(CombatHistory), nameof(CombatHistory.PowerReceived), typeof(CombatHistoryPatch), postfixName: nameof(CombatHistoryPatch.PowerReceivedPostfix));

        PatchUtils.PatchMethod(_harmony, typeof(DoomPower), nameof(DoomPower.DoomKill), typeof(DoomPowerPatch), prefixName: nameof(DoomPowerPatch.DoomKillPrefix));

        _harmony.PatchAll();
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(ModEntry).Assembly);

        LoadPowerRecordFactory();

        ModLogger.Clear();
        ModLogger.Log("模组初始化完成");
        Log.Info("Mod initialized!");
    }

    private static void SubscribeAllServices()
    {
        OnRunStartedService.Subscribe();
        BeforeCombatStartService.Subscribe();
        BeforeSideTurnStartService.Subscribe();
        AfterTurnEndService.Subscribe();
        AfterCombatEndService.Subscribe();
        AfterDamageGivenService.Subscribe();
        AfterDeathService.Subscribe();
        PowerReceivedService.Subscribe();
        DoomKillService.Subscribe();
    }

    private static void LoadPowerRecordFactory()
    {
        PoisonRecord.Register();
        DoomRecord.Register();
    }
}
