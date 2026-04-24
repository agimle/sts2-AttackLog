using Godot;

namespace AttackLog.View;

public static class PanelTheme
{
    public static readonly Color White = new("FFFFFF");
    public static readonly Color Gray = new("A0A8B4");
    public static readonly Color BgDark = new("000000B0");
    public static readonly Color Border = new("3A3A5C");
    public static readonly Color PercentColor = new("EC4899");
    public static readonly Color TotalColor = new("F59E0B");
    public static readonly Color RoomColor = new("10B981");
    public static readonly Color TurnColor = new("6366F1");

    public static Color Darken(Color color, float amount)
    {
        return new Color(
            Math.Max(0, color.R - amount),
            Math.Max(0, color.G - amount),
            Math.Max(0, color.B - amount),
            color.A
        );
    }

    public static StyleBoxFlat CreateCardStyle(Color baseColor)
    {
        var borderColor = Darken(baseColor, 0.3f);
        var bgColor = new Color(baseColor.R, baseColor.G, baseColor.B, 0.15f);

        return new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = borderColor,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 8,
            ContentMarginRight = 8,
            ContentMarginTop = 4,
            ContentMarginBottom = 4
        };
    }

    public static StyleBoxFlat CreateRootStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = BgDark,
            BorderColor = Border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };
    }

    public static Label MakeLabel(string text, int fontSize, Color color)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }
}
