using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;

namespace AttackLog;

/// <summary>
/// 补丁工具类
/// </summary>
public static class PatchUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="harmony"></param>
    /// <param name="hookName"></param>
    /// <param name="postfixName"></param>
    /// <exception cref="MissingMethodException"></exception>
    public static void PatchHook(Harmony harmony,string hookName, string postfixName)
    {
        MethodInfo original = AccessTools.Method(typeof(Hook), hookName)
                              ?? throw new MissingMethodException(typeof(Hook).FullName, hookName);
        MethodInfo postfix = AccessTools.Method(typeof(HookPatches), postfixName)
                             ?? throw new MissingMethodException(typeof(HookPatches).FullName, postfixName);

        harmony!.Patch(original, postfix: new HarmonyMethod(postfix));
    }
}