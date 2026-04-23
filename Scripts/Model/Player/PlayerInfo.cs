using System.Diagnostics.CodeAnalysis;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace AttackLog.Model;

public class PlayerInfo
{
    /// <summary>
    /// 玩家id
    /// </summary>
    public ulong NetId { get; private set; }
    /// <summary>
    /// 玩家昵称
    /// </summary>
    public string PlayerName { get; private set; }
    /// <summary>
    /// 玩家生物
    /// </summary>
    public Creature Creature { get; set; }
    /// <summary>
    /// 角色名
    /// </summary>
    public LocString Title { get; private set; }
    /// <summary>
    /// 角色名颜色
    /// </summary>
    public Color NameColor { get; private set; }
    /// <summary>
    /// 角色局内图标
    /// </summary>
    public Texture2D IconTexture { get;  private set; }
    
    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="netId"></param>
    /// <param name="playerName"></param>
    /// <param name="creature"></param>
    /// <param name="title"></param>
    /// <param name="nameColor"></param>
    /// <param name="iconTexture"></param>
    public PlayerInfo(ulong netId, string playerName, Creature creature, LocString title, Color nameColor, Texture2D iconTexture)
    {
        NetId = netId;
        PlayerName = playerName;
        Creature = creature;
        Title = title;
        NameColor = nameColor;
        IconTexture = iconTexture;
    }

    public override bool Equals(object? obj)
    {
        if(obj  is PlayerInfo info)
        {
            return NetId == info.NetId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NetId,Title);
    }
}