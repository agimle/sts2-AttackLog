using System.Diagnostics.CodeAnalysis;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace AttackLog.Model;

public class PlayerInfo
{
    public ulong NetId { get; private set; }
    public string PlayerName { get; private set; }
    public Creature Creature { get; set; }
    public LocString Title { get; private set; }
    public Color NameColor { get; private set; }
    public Texture2D IconTexture { get;  private set; }

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