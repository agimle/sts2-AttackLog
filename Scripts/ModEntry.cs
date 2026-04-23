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
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog;

[ModInitializer("Init")]
public class ModEntry
{
    /// <summary>
    /// Harmony实例
    /// </summary>
    private static Harmony? _harmony;

    // 初始化函数
    public static void Init()
    {
        // 检查是否已初始化
        if (_harmony != null)
        {
            return;
        }
        // 打patch（即修改游戏代码的功能）用
        // 传入参数随意，只要不和其他人撞车即可
        _harmony = new Harmony("com.agimle.sts2.attack_log");
        
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
        // 使得tscn可以加载自定义脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(ModEntry).Assembly);
        
        LoadPowerRecordFactory();
        
        ModLogger.Clear();
        ModLogger.Log("模组初始化完成");
        Log.Info("Mod initialized!");
    }
    
    private static void LoadPowerRecordFactory()
    {
        PoisonRecord.Register();
        DoomRecord.Register();
    }
}