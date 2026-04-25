using AttackLog.Core;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.Patch;
using AttackLog.View;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog;

/// <summary>
/// Mod 入口类，负责初始化 Harmony 补丁、服务定位器、能力记录工厂和面板创建
/// </summary>
[ModInitializer("Init")]
public class ModEntry
{
    /// <summary>Harmony 实例</summary>
    private static Harmony? _harmony;

    /// <summary>
    /// Mod 初始化入口，由游戏 Mod 加载器调用。
    /// 执行顺序：Harmony 补丁 → 服务初始化 → Hook 注册 → 能力工厂加载 → 面板订阅 → 日志初始化
    /// </summary>
    public static void Init()
    {
        if (_harmony != null)
        {
            return;
        }
        _harmony = new Harmony("com.agimle.sts2.attack_log");

        AttackLogServiceLocator.Initialize();

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

        AttackLogPanel.SubscribeCreationEvent();
        DamageTypeStatsPanel.SubscribeCreationEvent();

        ModLogger.Clear();
        ModLogger.Log("模组初始化完成");
        Log.Info("Mod initialized!");
    }

    /// <summary>
    /// 注册所有能力记录工厂（中毒、末日）
    /// </summary>
    private static void LoadPowerRecordFactory()
    {
        PoisonRecord.Register();
        DoomRecord.Register();
    }
}
