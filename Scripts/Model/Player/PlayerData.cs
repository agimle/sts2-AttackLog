using AttackLog.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace AttackLog.Model;

public class PlayerData
{
    public PlayerInfo  PlayerInfo  { get; set; }
    public SingleRunLogData RunLogData  { get; set; }

    public PlayerData(Player player)
    {
        string playerName = PlayerUtils.GetPlayerName(player.NetId) ?? $"Unknown";
        
        PlayerInfo = new PlayerInfo(player.NetId, playerName, player.Creature, player.Character.Title, player.Character.NameColor, player.Character.IconTexture);
        RunLogData = new SingleRunLogData();
    }
}