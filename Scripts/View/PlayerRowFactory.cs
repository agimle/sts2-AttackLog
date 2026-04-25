using AttackLog.Model;
using AttackLog.State;
using Godot;

namespace AttackLog.View;

public class PlayerRowFactory
{
    private readonly ILogDataProvider _dataProvider;

    public PlayerRowFactory(ILogDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public PanelContainer BuildRow(PlayerData playerData)
    {
        var card = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var baseColor = playerData.PlayerInfo.NameColor;
        card.AddThemeStyleboxOverride("panel", PanelTheme.CreateCardStyle(baseColor));

        var row = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 8);

        var iconTexture = playerData.PlayerInfo.IconTexture;
        if (iconTexture != null)
        {
            var icon = new TextureRect
            {
                Texture = iconTexture,
                CustomMinimumSize = new Vector2(24, 24),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
            };
            row.AddChild(icon);
        }

        var name = playerData.PlayerInfo.DisplayName;
        var nameLabel = PanelTheme.MakeLabel(name, 14, baseColor);
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(nameLabel);

        var stats = _dataProvider.GetPlayerStats(playerData.PlayerInfo) ?? new CachedPlayerStats();

        var percentLabel = PanelTheme.MakeLabel(stats.PercentText, 14, PanelTheme.PercentColor);
        percentLabel.CustomMinimumSize = new Vector2(60, 0);
        percentLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(percentLabel);

        var runDamageLabel = PanelTheme.MakeLabel(stats.RunDamageText, 14, PanelTheme.TotalColor);
        runDamageLabel.CustomMinimumSize = new Vector2(60, 0);
        runDamageLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(runDamageLabel);

        var roomDamageLabel = PanelTheme.MakeLabel(stats.RoomDamageText, 14, PanelTheme.RoomColor);
        roomDamageLabel.CustomMinimumSize = new Vector2(60, 0);
        roomDamageLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(roomDamageLabel);

        var turnDamageLabel = PanelTheme.MakeLabel(stats.TurnDamageText, 14, PanelTheme.TurnColor);
        turnDamageLabel.CustomMinimumSize = new Vector2(60, 0);
        turnDamageLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(turnDamageLabel);

        var wrapper = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        wrapper.AddThemeConstantOverride("separation", 4);
        wrapper.AddChild(row);

        float percent = Mathf.Clamp(stats.DamagePercent / 100f, 0.01f, 1f);

        var barFill = new ColorRect
        {
            Color = new Color(baseColor.R, baseColor.G, baseColor.B, 0.7f),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = percent,
            Name = "BarFill",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        var barEmpty = new Control
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 1f - percent,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        var barTrack = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 4),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        barTrack.AddChild(barFill);
        barTrack.AddChild(barEmpty);
        wrapper.AddChild(barTrack);

        card.AddChild(wrapper);
        return card;
    }

    public void UpdateRow(PanelContainer card, PlayerData playerData)
    {
        var wrapper = card.GetChild(0) as VBoxContainer;
        if (wrapper == null) return;

        var row = wrapper.GetChild(0) as HBoxContainer;
        if (row == null) return;

        var labels = row.GetChildren().OfType<Label>().ToList();
        if (labels.Count < 5) return;

        var stats = _dataProvider.GetPlayerStats(playerData.PlayerInfo);
        if (stats == null) return;

        labels[1].Text = stats.PercentText;
        labels[2].Text = stats.RunDamageText;
        labels[3].Text = stats.RoomDamageText;
        labels[4].Text = stats.TurnDamageText;

        var barTrack = wrapper.GetChild<HBoxContainer>(1);
        if (barTrack == null) return;

        float percent = Mathf.Clamp(stats.DamagePercent / 100f, 0.01f, 1f);

        var barFill = barTrack.GetChild<ColorRect>(0);
        if (barFill != null)
        {
            barFill.SizeFlagsStretchRatio = percent;
        }

        var barEmpty = barTrack.GetChild<Control>(1);
        if (barEmpty != null)
        {
            barEmpty.SizeFlagsStretchRatio = 1f - percent;
        }
    }

    public static HBoxContainer BuildHeaderRow()
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 8);

        var iconPlaceholder = new Control
        {
            CustomMinimumSize = new Vector2(24, 24)
        };
        row.AddChild(iconPlaceholder);

        var playerHeader = PanelTheme.MakeLabel("玩家", 11, PanelTheme.Gray);
        playerHeader.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        playerHeader.HorizontalAlignment = HorizontalAlignment.Left;
        row.AddChild(playerHeader);

        var percentHeader = PanelTheme.MakeLabel("占比", 11, PanelTheme.PercentColor);
        percentHeader.CustomMinimumSize = new Vector2(60, 0);
        percentHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(percentHeader);

        var totalHeader = PanelTheme.MakeLabel("总计", 11, PanelTheme.TotalColor);
        totalHeader.CustomMinimumSize = new Vector2(60, 0);
        totalHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(totalHeader);

        var roomHeader = PanelTheme.MakeLabel("房间", 11, PanelTheme.RoomColor);
        roomHeader.CustomMinimumSize = new Vector2(60, 0);
        roomHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(roomHeader);

        var turnHeader = PanelTheme.MakeLabel("回合", 11, PanelTheme.TurnColor);
        turnHeader.CustomMinimumSize = new Vector2(60, 0);
        turnHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(turnHeader);

        return row;
    }
}
