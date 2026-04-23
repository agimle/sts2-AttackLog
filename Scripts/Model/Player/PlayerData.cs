using MegaCrit.Sts2.Core.Entities.Players;

namespace AttackLog.Model;

/// <summary>
/// 玩家数据存档
/// </summary>
public class PlayerData
{
    /// <summary>
    /// 玩家基本信息
    /// </summary>
    public PlayerInfo  PlayerInfo  { get; set; }
    /// <summary>
    /// 玩家整场战斗数据
    /// </summary>
    public SingleRunLogData RunLogData  { get; set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="player"></param>
    public PlayerData(Player player)
    {
        string playerName = PlayerUtils.GetPlayerName(player.NetId) ?? $"Unknown";
        
        PlayerInfo = new PlayerInfo(player.NetId, playerName, player.Creature, player.Character.Title, player.Character.NameColor, player.Character.IconTexture);
        RunLogData = new SingleRunLogData();
    }
}