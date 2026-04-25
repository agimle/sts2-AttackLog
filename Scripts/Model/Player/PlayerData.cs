using AttackLog.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace AttackLog.Model;

/// <summary>
/// 玩家数据容器，关联玩家元信息和 Run 级日志数据
/// </summary>
public class PlayerData
{
    /// <summary>玩家元信息（名称、颜色、图标等）</summary>
    public PlayerInfo  PlayerInfo  { get; set; }

    /// <summary>玩家 Run 级日志数据</summary>
    public SingleRunLogData RunLogData  { get; set; }

    /// <summary>
    /// 从游戏原生 Player 对象构造，自动提取 PlayerInfo
    /// </summary>
    /// <param name="player">游戏原生 Player 对象</param>
    public PlayerData(Player player)
    {
        string playerName = PlayerUtils.GetPlayerName(player.NetId) ?? $"Unknown";

        PlayerInfo = new PlayerInfo(player.NetId, playerName, player.Creature, player.Character.Title, player.Character.NameColor, player.Character.IconTexture);
        RunLogData = new SingleRunLogData();
    }
}
