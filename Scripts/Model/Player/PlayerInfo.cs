using System.Diagnostics.CodeAnalysis;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace AttackLog.Model;

/// <summary>
/// 玩家元信息，作为 Dictionary 的 Key 使用。
/// 基于 NetId 判断相等性，支持缓存显示名称避免重复字符串分配。
/// </summary>
public class PlayerInfo
{
    /// <summary>网络 ID</summary>
    public ulong NetId { get; private set; }

    /// <summary>玩家名称</summary>
    public string PlayerName { get; private set; }

    /// <summary>关联的 Creature 实例</summary>
    public Creature Creature { get; set; }

    /// <summary>角色称号</summary>
    public LocString Title { get; private set; }

    /// <summary>名称颜色，用于 UI 显示</summary>
    public Color NameColor { get; private set; }

    /// <summary>角色图标纹理</summary>
    public Texture2D IconTexture { get;  private set; }

    /// <summary>
    /// 缓存的显示名称，格式为 "PlayerName [Title]"。
    /// 首次访问时生成并缓存，后续访问直接返回缓存值。
    /// </summary>
    private string? _cachedDisplayName;
    public string DisplayName => _cachedDisplayName ??= $"{PlayerName} [{Title.GetFormattedText()}]";

    public PlayerInfo(ulong netId, string playerName, Creature creature, LocString title, Color nameColor, Texture2D iconTexture)
    {
        NetId = netId;
        PlayerName = playerName;
        Creature = creature;
        Title = title;
        NameColor = nameColor;
        IconTexture = iconTexture;
    }

    /// <summary>
    /// 基于 NetId 判断相等性
    /// </summary>
    public override bool Equals(object? obj)
    {
        if(obj  is PlayerInfo info)
        {
            return NetId == info.NetId;
        }
        return false;
    }

    /// <summary>
    /// 使用 NetId + Title 组合计算哈希值
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(NetId,Title);
    }
}
