using Godot;

namespace AttackLog.View;

/// <summary>
/// 面板主题配置，定义所有颜色常量和样式工厂方法
/// </summary>
public static class PanelTheme
{
    /// <summary>白色</summary>
    public static readonly Color White = new("FFFFFF");
    /// <summary>灰色（表头、空状态文本）</summary>
    public static readonly Color Gray = new("A0A8B4");
    /// <summary>深色背景</summary>
    public static readonly Color BgDark = new("000000B0");
    /// <summary>边框颜色</summary>
    public static readonly Color Border = new("3A3A5C");
    /// <summary>占比列颜色（粉色）</summary>
    public static readonly Color PercentColor = new("EC4899");
    /// <summary>总计列颜色（琥珀色）</summary>
    public static readonly Color TotalColor = new("F59E0B");
    /// <summary>房间列颜色（绿色）</summary>
    public static readonly Color RoomColor = new("10B981");
    /// <summary>回合列颜色（紫色）</summary>
    public static readonly Color TurnColor = new("6366F1");

    /// <summary>
    /// 将颜色变暗指定量
    /// </summary>
    /// <param name="color">原始颜色</param>
    /// <param name="amount">变暗量</param>
    /// <returns>变暗后的颜色</returns>
    public static Color Darken(Color color, float amount)
    {
        return new Color(
            Math.Max(0, color.R - amount),
            Math.Max(0, color.G - amount),
            Math.Max(0, color.B - amount),
            color.A
        );
    }

    /// <summary>
    /// 创建玩家卡片样式，基于玩家颜色生成半透明背景和边框
    /// </summary>
    /// <param name="baseColor">玩家基础颜色</param>
    /// <returns>卡片样式</returns>
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

    /// <summary>
    /// 创建面板根容器样式
    /// </summary>
    /// <returns>根容器样式</returns>
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

    /// <summary>
    /// 创建带主题颜色的 Label
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="color">字体颜色</param>
    /// <returns>配置好的 Label</returns>
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
