using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace AttackLog;

public class ModEntry
{
    // 初始化函数
    public static void Init()
    {
        // 打patch（即修改游戏代码的功能）用
        // 传入参数随意，只要不和其他人撞车即可
        var harmony = new Harmony(".luting4u.sts2.attacklog");
        harmony.PatchAll();
        // 使得tscn可以加载自定义脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(ModEntry).Assembly);
        Log.Info("Mod initialized!");
    }
}