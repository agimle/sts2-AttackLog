using System.Globalization;
using MegaCrit.Sts2.Core.Platform;

namespace AttackLog.Utils;

/// <summary>
/// 玩家工具类，通过平台 API 获取玩家显示名称。
/// 过滤无效名称（空字符串、默认占位符、纯数字 ID）
/// </summary>
public static class PlayerUtils
{
    /// <summary>
    /// 根据 NetId 获取玩家显示名称
    /// </summary>
    /// <param name="netId">玩家网络 ID</param>
    /// <returns>有效名称返回名称字符串，无效则返回 null</returns>
    public static string? GetPlayerName(ulong netId)
    {
        try
        {
            PlatformType platformType = PlatformUtil.PrimaryPlatform;
            if (platformType == PlatformType.None)
            {
                return null;
            }

            string? playerName = PlatformUtil.GetPlayerName(platformType, netId);

            if (string.IsNullOrWhiteSpace(playerName)
                || string.Equals(playerName, "[玩家]", StringComparison.OrdinalIgnoreCase)

                || string.Equals(playerName, netId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal))
            {
                return null;
            }

            return playerName;
        }
        catch
        {
            return null;
        }
    }
}
