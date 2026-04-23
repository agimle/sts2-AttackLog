using System.Reflection;
using AttackLog.Patch;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;

namespace AttackLog;

/// <summary>
/// 补丁工具类
/// </summary>
public static class PatchUtils
{
    /// <summary>
    /// 针对Hook的补丁
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

        harmony.Patch(original, postfix: new HarmonyMethod(postfix));
    }

    /// <summary>
    /// 通用补丁
    /// </summary>
    /// <param name="harmony"></param>
    /// <param name="originalClassName"></param>
    /// <param name="methodName"></param>
    /// <param name="patchClass"></param>
    /// <param name="prefixName"></param>
    /// <param name="postfixName"></param>
    /// <exception cref="MissingMethodException"></exception>
    public static void PatchMethod(Harmony harmony,
        Type originalClassName, string methodName, Type patchClass, string? prefixName = null, string? postfixName = null)
    {
        MethodInfo original = AccessTools.Method(originalClassName, methodName)
                              ?? throw new MissingMethodException(originalClassName.FullName, methodName);
        HarmonyMethod? prefix = prefixName != null
            ? new HarmonyMethod(AccessTools.Method(patchClass, prefixName)
                                ?? throw new MissingMethodException(patchClass.FullName, prefixName))
            : null;
        HarmonyMethod? postfix = postfixName != null
            ? new HarmonyMethod(AccessTools.Method(patchClass, postfixName)
                                ?? throw new MissingMethodException(patchClass.FullName, postfixName))
            : null;

        harmony.Patch(original, prefix: prefix, postfix: postfix);
    }
}