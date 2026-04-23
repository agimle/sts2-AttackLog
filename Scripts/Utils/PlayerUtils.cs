using System.Globalization;
using MegaCrit.Sts2.Core.Platform;

namespace AttackLog.Utils;

public static class PlayerUtils
{
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
