using Godot;
using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;

namespace AttackLog.View;

public sealed partial class DamageTypeStatsPanel : CanvasLayer
{
    private const float PanelWidth = 380f;
    private const float RefreshInterval = 0.1f;

    private static readonly AttackLogEventType[] RefreshOnEvents =
    {
        AttackLogEventType.RunStarted,
        AttackLogEventType.BeforeCombatStart,
        AttackLogEventType.AfterTurnEnd,
        AttackLogEventType.AfterCombatEnd,
        AttackLogEventType.AfterDamageGiven,
        AttackLogEventType.DoomKill,
        AttackLogEventType.RefreshRequested
    };

    private static DamageTypeStatsPanel? _instance;

    private PanelContainer? _root;
    private VBoxContainer? _contentContainer;
    private VBoxContainer? _playerList;
    private Button? _toggleButton;

    private bool _isGlobal = true;
    private float _lastRefreshTime = 0f;
    private bool _refreshRequested = false;
    private PanelDragHandler _dragHandler = new();
    private Dictionary<PlayerInfo, PanelContainer> _playerRows = new();

    private static readonly Color DoomColor = new(0.6f, 0.6f, 0.6f, 0.15f);
    private static readonly Color PoisonColor = new(0.65f, 0.4f, 0.95f, 0.15f);
    private static readonly Color BlockColor = new(0.35f, 0.8f, 0.95f, 0.15f);

    private static readonly Color DoomColorSolid = new(0.6f, 0.6f, 0.6f);
    private static readonly Color PoisonColorSolid = new(0.65f, 0.4f, 0.95f);
    private static readonly Color BlockColorSolid = new(0.35f, 0.8f, 0.95f);
    private static readonly Color PinkColor = new(0.95f, 0.55f, 0.8f);

    public DamageTypeStatsPanel()
    {
    }

    public override void _EnterTree()
    {
        Layer = 100;
        Name = nameof(DamageTypeStatsPanel);
        _instance = this;
        SubscribeAll();
    }

    public override void _ExitTree()
    {
        UnsubscribeAll();
        if (ReferenceEquals(_instance, this))
            _instance = null;
    }

    public override void _Ready()
    {
        BuildUi();
        SetInitialPosition();
        Refresh();
        _dragHandler.DragEnded += OnDragEnded;
    }

    private void OnDragEnded()
    {
        if (_root != null)
        {
            ModSaveUtils.SavePanelPosition(false, _root.Position.X, _root.Position.Y);
        }
    }

    public override void _Process(double delta)
    {
        if (_refreshRequested)
        {
            float currentTime = Time.GetTicksMsec() / 1000f;
            if (currentTime - _lastRefreshTime >= RefreshInterval)
            {
                Refresh();
                _lastRefreshTime = currentTime;
                _refreshRequested = false;
            }
        }
    }

    private void OnGameEvent(IAttackLogEvent e)
    {
        RequestRefresh();
    }

    private void RequestRefresh()
    {
        float currentTime = Time.GetTicksMsec() / 1000f;
        if (currentTime - _lastRefreshTime >= RefreshInterval)
        {
            Refresh();
            _lastRefreshTime = currentTime;
        }
        else
        {
            _refreshRequested = true;
        }
    }

    private void SubscribeAll()
    {
        foreach (var eventType in RefreshOnEvents)
        {
            AttackLogEventBus.Subscribe(eventType, OnGameEvent);
        }
    }

    private void UnsubscribeAll()
    {
        foreach (var eventType in RefreshOnEvents)
        {
            AttackLogEventBus.Unsubscribe(eventType, OnGameEvent);
        }
    }

    private void BuildUi()
    {
        _root = new PanelContainer
        {
            Name = "Root",
            MouseFilter = Control.MouseFilterEnum.Stop,
            CustomMinimumSize = new Vector2(PanelWidth, 0),
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
        };

        _root.AddThemeStyleboxOverride("panel", PanelTheme.CreateRootStyle());

        var margin = new MarginContainer();
        margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);

        _contentContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _contentContainer.AddThemeConstantOverride("separation", 6);

        var titleBar = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        titleBar.AddThemeConstantOverride("separation", 8);

        var title = PanelTheme.MakeLabel("伤害统计", 14, PanelTheme.White);
        title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        title.HorizontalAlignment = HorizontalAlignment.Left;
        titleBar.AddChild(title);

        _toggleButton = new Button
        {
            Text = "全局",
            CustomMinimumSize = new Vector2(52, 26)
        };
        _toggleButton.AddThemeFontSizeOverride("font_size", 13);
        var btnStyle = new StyleBoxFlat
        {
            BgColor = new Color(PanelTheme.TotalColor.R, PanelTheme.TotalColor.G, PanelTheme.TotalColor.B, 0.25f),
            BorderColor = new Color(PanelTheme.TotalColor.R, PanelTheme.TotalColor.G, PanelTheme.TotalColor.B, 0.6f),
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
            ContentMarginTop = 2,
            ContentMarginBottom = 2
        };
        _toggleButton.AddThemeStyleboxOverride("normal", btnStyle);
        _toggleButton.AddThemeStyleboxOverride("hover", btnStyle);
        _toggleButton.AddThemeStyleboxOverride("pressed", btnStyle);
        _toggleButton.AddThemeStyleboxOverride("focus", btnStyle);
        ApplyButtonColor(PanelTheme.TotalColor);
        _toggleButton.Pressed += OnTogglePressed;
        titleBar.AddChild(_toggleButton);

        _contentContainer.AddChild(titleBar);

        var separator = HLine(1, PanelTheme.Border);
        _contentContainer.AddChild(separator);

        _playerList = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _playerList.AddThemeConstantOverride("separation", 4);
        _contentContainer.AddChild(_playerList);

        var separator2 = HLine(1, PanelTheme.Border);
        _contentContainer.AddChild(separator2);

        var legendRow = BuildLegendRow();
        _contentContainer.AddChild(legendRow);

        margin.AddChild(_contentContainer);
        _root.AddChild(margin);
        AddChild(_root);

        _root.GuiInput += OnRootGuiInput;
    }

    private void ApplyButtonColor(Color color)
    {
        if (_toggleButton == null) return;
        _toggleButton.AddThemeColorOverride("font_color", color);
        _toggleButton.AddThemeColorOverride("font_hover_color", color);
        _toggleButton.AddThemeColorOverride("font_pressed_color", color);
        _toggleButton.AddThemeColorOverride("font_focus_color", color);

        var btnStyle = new StyleBoxFlat
        {
            BgColor = new Color(color.R, color.G, color.B, 0.25f),
            BorderColor = new Color(color.R, color.G, color.B, 0.6f),
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
            ContentMarginTop = 2,
            ContentMarginBottom = 2
        };
        _toggleButton.AddThemeStyleboxOverride("normal", btnStyle);
        _toggleButton.AddThemeStyleboxOverride("hover", btnStyle);
        _toggleButton.AddThemeStyleboxOverride("pressed", btnStyle);
        _toggleButton.AddThemeStyleboxOverride("focus", btnStyle);
    }

    private HBoxContainer BuildLegendRow()
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 12);

        var spacer = new Control { CustomMinimumSize = new Vector2(60, 0) };
        row.AddChild(spacer);

        var doomLegend = CreateLegendItem("灾厄", DoomColorSolid);
        row.AddChild(doomLegend);

        var poisonLegend = CreateLegendItem("毒伤", PoisonColorSolid);
        row.AddChild(poisonLegend);

        var blockLegend = CreateLegendItem("护盾", BlockColorSolid);
        row.AddChild(blockLegend);

        return row;
    }

    private HBoxContainer CreateLegendItem(string name, Color color)
    {
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 4);

        var colorBox = new ColorRect
        {
            Color = color,
            CustomMinimumSize = new Vector2(10, 10)
        };
        container.AddChild(colorBox);

        var label = PanelTheme.MakeLabel(name, 10, color);
        container.AddChild(label);

        return container;
    }

    private void SetInitialPosition()
    {
        if (_root == null) return;
        var viewport = GetViewport();
        if (viewport == null) return;

        var savedPos = ModSaveUtils.GetPanelPosition(false);
        if (savedPos.HasValue)
        {
            _root.Position = ClampToScreen(savedPos.Value, _root.Size);
            return;
        }

        var mainPanelPos = ModSaveUtils.GetPanelPosition(true);
        var screenSize = viewport.GetVisibleRect().Size;

        float x, y;
        if (mainPanelPos.HasValue)
        {
            x = mainPanelPos.Value.X - PanelWidth - 16f;
            y = mainPanelPos.Value.Y;
        }
        else
        {
            x = screenSize.X - PanelWidth - AttackLogPanel.GetPanelWidth() - 32f;
            y = screenSize.Y * 0.1f;
        }

        _root.Position = ClampToScreen(new Vector2(x, y), _root.Size);
    }

    private Vector2 ClampToScreen(Vector2 pos, Vector2 panelSize)
    {
        var viewport = GetViewport();
        if (viewport == null) return pos;

        var screenSize = viewport.GetVisibleRect().Size;
        float minX = 0f;
        float minY = 0f;
        float maxX = Math.Max(minX, screenSize.X - panelSize.X);
        float maxY = Math.Max(minY, screenSize.Y - panelSize.Y);

        return new Vector2(
            Mathf.Clamp(pos.X, minX, maxX),
            Mathf.Clamp(pos.Y, minY, maxY)
        );
    }

    private void OnRootGuiInput(InputEvent @event)
    {
        if (_root != null)
        {
            _dragHandler.HandleInput(@event, _root);
        }
    }

    private void OnTogglePressed()
    {
        _isGlobal = !_isGlobal;
        if (_toggleButton != null)
        {
            _toggleButton.Text = _isGlobal ? "全局" : "房间";
            ApplyButtonColor(_isGlobal ? PanelTheme.TotalColor : PanelTheme.RoomColor);
        }
        Refresh();
    }

    private void Refresh()
    {
        if (_playerList == null) return;

        ResetRootSize();

        var runLog = LogState.Instance.RunLog;
        if (runLog == null)
        {
            ClearAllRows();
            ResetRootSize();
            return;
        }

        var players = runLog.GetAllPlayers().ToList();
        if (players.Count == 0)
        {
            ClearAllRows();
            ResetRootSize();
            return;
        }

        int allPlayersTotal = 0;
        var playerDamages = new List<(PlayerData player, int doom, int common, int poison, int block, int total)>();

        foreach (var player in players)
        {
            var stats = LogState.Instance.GetCachedStats(player.PlayerInfo);
            if (stats == null) continue;

            AttackLogModel? model = _isGlobal ? stats.RunLog : stats.RoomLog;
            if (model == null) continue;

            int doom = model.DoomDamage;
            int common = model.CommonDamage;
            int poison = model.PoisonDamage;
            int block = model.DamageOnBlock;
            int total = doom + common + poison + block;

            allPlayersTotal += total;
            playerDamages.Add((player, doom, common, poison, block, total));
        }

        playerDamages = playerDamages.OrderByDescending(p => p.total).ToList();

        var currentPlayerInfos = new HashSet<PlayerInfo>(players.Select(p => p.PlayerInfo));

        var toRemove = _playerRows.Keys
            .Where(info => !currentPlayerInfos.Contains(info))
            .ToList();

        foreach (var info in toRemove)
        {
            if (_playerRows.TryGetValue(info, out var row))
            {
                row.GetParent()?.RemoveChild(row);
                row.QueueFree();
                _playerRows.Remove(info);
            }
        }

        bool showPercent = players.Count > 1;

        for (int i = 0; i < playerDamages.Count; i++)
        {
            var (player, doom, common, poison, block, total) = playerDamages[i];
            int rank = i + 1;
            float percent = allPlayersTotal > 0 ? (float)total / allPlayersTotal * 100f : 0f;

            if (_playerRows.TryGetValue(player.PlayerInfo, out var existingRow))
            {
                UpdatePlayerRow(existingRow, player, doom, common, poison, block, total, percent, showPercent, rank);
            }
            else
            {
                var newRow = BuildPlayerRow(player, doom, common, poison, block, total, percent, showPercent, rank);
                _playerList.AddChild(newRow);
                _playerRows[player.PlayerInfo] = newRow;
            }
        }

        ResetRootSize();
    }

    private PanelContainer BuildPlayerRow(PlayerData player, int doom, int common, int poison, int block, int total, float percent, bool showPercent, int rank)
    {
        var baseColor = player.PlayerInfo.NameColor;
        var commonColorLight = new Color(baseColor.R, baseColor.G, baseColor.B, 1.0f);

        var card = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 32)
        };

        var barStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.08f, 0.1f, 0.75f),
            BorderColor = new Color(0.2f, 0.2f, 0.25f, 0.5f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4
        };
        card.AddThemeStyleboxOverride("panel", barStyle);

        var barContainer = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        barContainer.Name = "BarContainer";
        barContainer.AddThemeConstantOverride("separation", 0);
        UpdateBarContainer(barContainer, doom, common, poison, block, baseColor);
        card.AddChild(barContainer);

        var overlay = new Control
        {
            Name = "Overlay",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var contentRow = new HBoxContainer
        {
            Name = "ContentRow",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        contentRow.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        contentRow.AddThemeConstantOverride("separation", 6);

        var leftPadding = new Control { CustomMinimumSize = new Vector2(10, 0) };
        contentRow.AddChild(leftPadding);

        var rankLabel = PanelTheme.MakeLabel($"{rank}.", 13, PanelTheme.White);
        rankLabel.Name = "RankLabel";
        contentRow.AddChild(rankLabel);

        var nameLabel = PanelTheme.MakeLabel(player.PlayerInfo.DisplayName, 13, PanelTheme.White);
        nameLabel.Name = "NameLabel";
        contentRow.AddChild(nameLabel);

        var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        contentRow.AddChild(spacer);

        var valueContainer = BuildValueContainer(doom, common, poison, block, total, percent, showPercent, commonColorLight);
        valueContainer.Name = "ValueContainer";
        contentRow.AddChild(valueContainer);

        var rightPadding = new Control { CustomMinimumSize = new Vector2(10, 0) };
        contentRow.AddChild(rightPadding);

        overlay.AddChild(contentRow);
        card.AddChild(overlay);

        return card;
    }

    private void UpdatePlayerRow(PanelContainer card, PlayerData player, int doom, int common, int poison, int block, int total, float percent, bool showPercent, int rank)
    {
        var baseColor = player.PlayerInfo.NameColor;
        var commonColorLight = new Color(baseColor.R, baseColor.G, baseColor.B, 1.0f);

        var barContainer = card.GetNodeOrNull<HBoxContainer>("BarContainer");
        if (barContainer != null)
        {
            UpdateBarContainer(barContainer, doom, common, poison, block, baseColor);
        }

        var valueContainer = card.GetNodeOrNull<HBoxContainer>("Overlay/ContentRow/ValueContainer");
        if (valueContainer != null)
        {
            UpdateValueContainer(valueContainer, doom, common, poison, block, total, percent, showPercent, rank, commonColorLight);
        }

        var rankLabel = card.GetNodeOrNull<Label>("Overlay/ContentRow/RankLabel");
        if (rankLabel != null)
        {
            rankLabel.Text = $"{rank}.";
        }
    }

    private void UpdateBarContainer(HBoxContainer container, int doom, int common, int poison, int block, Color baseColor)
    {
        foreach (var child in container.GetChildren())
        {
            child.QueueFree();
        }
        container.GetChildren().Clear();

        int total = doom + common + poison + block;

        if (total == 0)
        {
            var emptyBar = new ColorRect
            {
                Color = new Color(0.2f, 0.2f, 0.2f, 0.3f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsStretchRatio = 1f,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            container.AddChild(emptyBar);
            return;
        }

        float doomRatio = (float)doom / total;
        float commonRatio = (float)common / total;
        float poisonRatio = (float)poison / total;
        float blockRatio = (float)block / total;

        const float minRatio = 0.02f;
        if (doomRatio > 0 && doomRatio < minRatio) doomRatio = minRatio;
        if (commonRatio > 0 && commonRatio < minRatio) commonRatio = minRatio;
        if (poisonRatio > 0 && poisonRatio < minRatio) poisonRatio = minRatio;
        if (blockRatio > 0 && blockRatio < minRatio) blockRatio = minRatio;

        float totalRatio = doomRatio + commonRatio + poisonRatio + blockRatio;
        doomRatio /= totalRatio;
        commonRatio /= totalRatio;
        poisonRatio /= totalRatio;
        blockRatio /= totalRatio;

        var commonColor = new Color(baseColor.R, baseColor.G, baseColor.B, 0.15f);

        if (common > 0)
        {
            container.AddChild(CreateBarSegment(commonColor, commonRatio));
        }

        if (doom > 0)
        {
            container.AddChild(CreateBarSegment(DoomColor, doomRatio));
        }

        if (poison > 0)
        {
            container.AddChild(CreateBarSegment(PoisonColor, poisonRatio));
        }

        if (block > 0)
        {
            container.AddChild(CreateBarSegment(BlockColor, blockRatio));
        }
    }

    private ColorRect CreateBarSegment(Color color, float ratio)
    {
        return new ColorRect
        {
            Color = color,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = ratio,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }

    private HBoxContainer BuildValueContainer(int doom, int common, int poison, int block, int total, float percent, bool showPercent, Color commonColor)
    {
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 2);

        if (showPercent)
        {
            var percentLabel = PanelTheme.MakeLabel($"({percent:F0}%)", 14, PanelTheme.White);
            percentLabel.Name = "PercentLabel";
            container.AddChild(percentLabel);

            var totalLabel = PanelTheme.MakeLabel(total.ToString(), 14, PanelTheme.White);
            totalLabel.Name = "TotalLabel";
            totalLabel.HorizontalAlignment = HorizontalAlignment.Right;
            container.AddChild(totalLabel);
        }
        else
        {
            var totalLabel = PanelTheme.MakeLabel(total.ToString(), 14, PanelTheme.White);
            totalLabel.Name = "TotalLabel";
            totalLabel.HorizontalAlignment = HorizontalAlignment.Right;
            container.AddChild(totalLabel);
        }

        var equalLabel = PanelTheme.MakeLabel("=", 14, PinkColor);
        container.AddChild(equalLabel);

        var commonLabel = PanelTheme.MakeLabel(common.ToString(), 14, commonColor);
        commonLabel.Name = "CommonLabel";
        container.AddChild(commonLabel);

        var plus1 = PanelTheme.MakeLabel("+", 14, PinkColor);
        container.AddChild(plus1);

        var doomLabel = PanelTheme.MakeLabel(doom.ToString(), 14, DoomColorSolid);
        doomLabel.Name = "DoomLabel";
        container.AddChild(doomLabel);

        var plus2 = PanelTheme.MakeLabel("+", 14, PinkColor);
        container.AddChild(plus2);

        var poisonLabel = PanelTheme.MakeLabel(poison.ToString(), 14, PoisonColorSolid);
        poisonLabel.Name = "PoisonLabel";
        container.AddChild(poisonLabel);

        var plus3 = PanelTheme.MakeLabel("+", 14, PinkColor);
        container.AddChild(plus3);

        var blockLabel = PanelTheme.MakeLabel(block.ToString(), 14, BlockColorSolid);
        blockLabel.Name = "BlockLabel";
        container.AddChild(blockLabel);

        return container;
    }

    private void UpdateValueContainer(HBoxContainer container, int doom, int common, int poison, int block, int total, float percent, bool showPercent, int rank, Color commonColor)
    {
        if (showPercent)
        {
            if (container.GetNodeOrNull<Label>("PercentLabel") is { } percentLabel)
            {
                percentLabel.Text = $"({percent:F0}%)";
            }

            if (container.GetNodeOrNull<Label>("TotalLabel") is { } totalLabel)
            {
                totalLabel.Text = total.ToString();
            }
        }
        else
        {
            if (container.GetNodeOrNull<Label>("TotalLabel") is { } totalLabel)
            {
                totalLabel.Text = total.ToString();
            }
        }

        if (container.GetNodeOrNull<Label>("CommonLabel") is { } commonLabel)
        {
            commonLabel.Text = common.ToString();
            commonLabel.AddThemeColorOverride("font_color", commonColor);
        }

        if (container.GetNodeOrNull<Label>("DoomLabel") is { } doomLabel)
        {
            doomLabel.Text = doom.ToString();
        }

        if (container.GetNodeOrNull<Label>("PoisonLabel") is { } poisonLabel)
        {
            poisonLabel.Text = poison.ToString();
        }

        if (container.GetNodeOrNull<Label>("BlockLabel") is { } blockLabel)
        {
            blockLabel.Text = block.ToString();
        }
    }

    private void ClearAllRows()
    {
        foreach (var row in _playerRows.Values)
        {
            row.GetParent()?.RemoveChild(row);
            row.QueueFree();
        }
        _playerRows.Clear();
    }

    private void ResetRootSize()
    {
        if (_root == null) return;
        _root.ResetSize();
    }

    private static Control HLine(int height, Color color)
    {
        return new Control
        {
            CustomMinimumSize = new Vector2(0, height),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }

    public static void EnsureCreated()
    {
        if (_instance != null) return;

        _instance = new DamageTypeStatsPanel();
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        sceneTree?.Root.AddChild(_instance);
    }

    public static void RefreshInstance()
    {
        AttackLogEventBus.Publish(new RefreshRequestedEvent());
    }

    public static void SubscribeCreationEvent()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.RunStarted, OnRunStartedForCreation);
    }

    private static void OnRunStartedForCreation(IAttackLogEvent e)
    {
        EnsureCreated();
        RefreshInstance();
    }
}
