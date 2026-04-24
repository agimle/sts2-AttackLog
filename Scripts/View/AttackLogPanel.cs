using Godot;
using AttackLog.Core;
using AttackLog.Model;
using AttackLog.State;
using AttackLog.Utils;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.View;

public sealed partial class AttackLogPanel : CanvasLayer
{
    private const float PanelWidth = 460f;
    private const float RefreshInterval = 0.1f;

    private static readonly Color White = new("FFFFFF");
    private static readonly Color Gray = new("A0A8B4");
    private static readonly Color BgDark = new("000000B0");
    private static readonly Color Border = new("3A3A5C");
    private static readonly Color PercentColor = new("EC4899");
    private static readonly Color TotalColor = new("F59E0B");
    private static readonly Color RoomColor = new("10B981");
    private static readonly Color TurnColor = new("6366F1");

    private static readonly AttackLogEventType[] RefreshOnEvents =
    {
        AttackLogEventType.RunStarted,
        AttackLogEventType.BeforeCombatStart,
        AttackLogEventType.BeforeSideTurnStart,
        AttackLogEventType.AfterTurnEnd,
        AttackLogEventType.AfterCombatEnd,
        AttackLogEventType.AfterDamageGiven,
        AttackLogEventType.AfterDeath,
        AttackLogEventType.PowerReceived,
        AttackLogEventType.DoomKill,
        AttackLogEventType.RefreshRequested
    };

    private static AttackLogPanel? _instance;
    private static ILogDataProvider _dataProvider = LogDataProvider.Default;

    private PanelContainer? _root;
    private VBoxContainer? _playerList;
    private Label? _emptyLabel;

    private Dictionary<PlayerInfo, PanelContainer> _playerRows = new();
    private float _lastRefreshTime = 0f;
    private bool _refreshRequested = false;

    private bool _isDragging = false;
    private Vector2 _dragOffset = Vector2.Zero;

    private enum SortMode { Total, Room }
    private SortMode _sortMode = SortMode.Total;
    private Button? _sortButton;

    public static void SetDataProvider(ILogDataProvider provider)
    {
        _dataProvider = provider ?? LogDataProvider.Default;
    }

    public override void _EnterTree()
    {
        Layer = 100;
        Name = nameof(AttackLogPanel);
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
    }

    public override void _Process(double delta)
    {
        if (_isDragging && _root != null)
        {
            var newPos = _root.GetGlobalMousePosition() - _dragOffset;
            _root.Position = ClampToScreen(newPos, _root.Size);
        }

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

        _root.AddThemeStyleboxOverride("panel", new StyleBoxFlat
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
        });

        var margin = new MarginContainer();
        margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);

        var col = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        col.AddThemeConstantOverride("separation", 6);

        var titleBar = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        titleBar.AddThemeConstantOverride("separation", 6);

        var title = MakeLabel("⚔ 战斗统计", 16, White, true);
        title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        title.HorizontalAlignment = HorizontalAlignment.Left;
        titleBar.AddChild(title);

        _sortButton = new Button
        {
            Text = "▼ 总计",
            Flat = true,
            CustomMinimumSize = new Vector2(60, 28),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        _sortButton.AddThemeFontSizeOverride("font_size", 11);
        _sortButton.AddThemeColorOverride("font_color", TotalColor);
        _sortButton.AddThemeColorOverride("font_hover_color", TotalColor);
        _sortButton.AddThemeColorOverride("font_pressed_color", TotalColor);
        var btnEmptyStyle = new StyleBoxEmpty();
        _sortButton.AddThemeStyleboxOverride("normal", btnEmptyStyle);
        _sortButton.AddThemeStyleboxOverride("hover", btnEmptyStyle);
        _sortButton.AddThemeStyleboxOverride("pressed", btnEmptyStyle);
        _sortButton.AddThemeStyleboxOverride("focus", btnEmptyStyle);
        _sortButton.Pressed += OnSortToggled;
        titleBar.AddChild(_sortButton);

        col.AddChild(titleBar);

        var separator = HLine(1, Border);
        col.AddChild(separator);

        var headerMargin = new MarginContainer();
        headerMargin.AddThemeConstantOverride("margin_left", 8);
        headerMargin.AddThemeConstantOverride("margin_right", 8);
        var headerRow = BuildHeaderRow();
        headerMargin.AddChild(headerRow);
        col.AddChild(headerMargin);

        var separator2 = HLine(1, Border);
        col.AddChild(separator2);

        _playerList = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _playerList.AddThemeConstantOverride("separation", 6);
        col.AddChild(_playerList);

        _emptyLabel = MakeLabel("No players registered", 12, Gray);
        _emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        col.AddChild(_emptyLabel);

        margin.AddChild(col);
        _root.AddChild(margin);
        AddChild(_root);

        _root.GuiInput += OnRootGuiInput;
    }

    private void SetInitialPosition()
    {
        if (_root == null) return;
        var viewport = GetViewport();
        if (viewport == null) return;

        var screenSize = viewport.GetVisibleRect().Size;
        float x = screenSize.X - PanelWidth - 16f;
        float y = screenSize.Y * 0.1f;
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
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    _isDragging = true;
                    _dragOffset = _root!.GetGlobalMousePosition() - _root.Position;
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
    }

    private void OnSortToggled()
    {
        _sortMode = _sortMode == SortMode.Total ? SortMode.Room : SortMode.Total;
        UpdateSortButton();
        ClearAllRows();
        ResetRootSize();
        Refresh();
    }

    private void UpdateSortButton()
    {
        if (_sortButton == null) return;
        _sortButton.Text = _sortMode == SortMode.Total ? "▼ 总计" : "▼ 房间";
        var color = _sortMode == SortMode.Total ? TotalColor : RoomColor;
        _sortButton.AddThemeColorOverride("font_color", color);
        _sortButton.AddThemeColorOverride("font_hover_color", color);
        _sortButton.AddThemeColorOverride("font_pressed_color", color);
    }

    private void Refresh()
    {
        if (_playerList == null || _emptyLabel == null) return;

        ResetRootSize();

        if (!_dataProvider.HasActiveRun)
        {
            _emptyLabel.Visible = true;
            _emptyLabel.Text = "No active run";
            ClearAllRows();
            ResetRootSize();
            return;
        }

        var players = _dataProvider.GetPlayers().ToList();
        if (players.Count == 0)
        {
            _emptyLabel.Visible = true;
            _emptyLabel.Text = "No players registered";
            ClearAllRows();
            ResetRootSize();
            return;
        }

        players = SortPlayers(players);

        _emptyLabel.Visible = false;

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

        int index = 0;
        foreach (var playerData in players)
        {
            if (_playerRows.TryGetValue(playerData.PlayerInfo, out var existingRow))
            {
                UpdatePlayerRow(existingRow, playerData, index);
            }
            else
            {
                var newRow = BuildPlayerRow(playerData, index);
                _playerList.AddChild(newRow);
                _playerRows[playerData.PlayerInfo] = newRow;
            }
            index++;
        }

        ReorderPlayerRows(players);

        ResetRootSize();
    }

    private List<PlayerData> SortPlayers(List<PlayerData> players)
    {
        return players.OrderByDescending(p =>
        {
            var stats = _dataProvider.GetPlayerStats(p.PlayerInfo);
            if (stats == null) return 0f;
            return _sortMode == SortMode.Total
                ? stats.RunLog?.RealDamageDealt ?? 0
                : stats.RoomLog?.RealDamageDealt ?? 0;
        }).ToList();
    }

    private void ReorderPlayerRows(List<PlayerData> sortedPlayers)
    {
        if (_playerList == null) return;
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (_playerRows.TryGetValue(sortedPlayers[i].PlayerInfo, out var row))
            {
                _playerList.MoveChild(row, i);
            }
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

    private static HBoxContainer BuildHeaderRow()
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 8);

        var iconPlaceholder = new Control
        {
            CustomMinimumSize = new Vector2(24, 24)
        };
        row.AddChild(iconPlaceholder);

        var playerHeader = MakeLabel("玩家", 11, Gray);
        playerHeader.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        playerHeader.HorizontalAlignment = HorizontalAlignment.Left;
        row.AddChild(playerHeader);

        var percentHeader = MakeLabel("占比", 11, PercentColor);
        percentHeader.CustomMinimumSize = new Vector2(60, 0);
        percentHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(percentHeader);

        var totalHeader = MakeLabel("总计", 11, TotalColor);
        totalHeader.CustomMinimumSize = new Vector2(60, 0);
        totalHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(totalHeader);

        var roomHeader = MakeLabel("房间", 11, RoomColor);
        roomHeader.CustomMinimumSize = new Vector2(60, 0);
        roomHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(roomHeader);

        var turnHeader = MakeLabel("回合", 11, TurnColor);
        turnHeader.CustomMinimumSize = new Vector2(60, 0);
        turnHeader.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(turnHeader);

        return row;
    }

    private static PanelContainer BuildPlayerRow(PlayerData playerData, int rowIndex)
    {
        var card = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var baseColor = playerData.PlayerInfo.NameColor;
        var borderColor = DarkenColor(baseColor, 0.3f);
        var bgColor = new Color(baseColor.R, baseColor.G, baseColor.B, 0.15f);

        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
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
        });

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

        var name = playerData.PlayerInfo.PlayerName + " [" + playerData.PlayerInfo.Title.GetFormattedText() + "]";
        var nameLabel = MakeLabel(name, 14, baseColor);
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(nameLabel);

        var stats = _dataProvider.GetPlayerStats(playerData.PlayerInfo) ?? new CachedPlayerStats();

        var percentLabel = MakeLabel($"{stats.DamagePercent:F1}%", 14, PercentColor);
        percentLabel.CustomMinimumSize = new Vector2(60, 0);
        percentLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(percentLabel);

        var runDamage = stats.RunLog?.RealDamageDealt ?? 0;
        var runDamageLabel = MakeLabel($"{runDamage}", 14, TotalColor);
        runDamageLabel.CustomMinimumSize = new Vector2(60, 0);
        runDamageLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(runDamageLabel);

        var roomDamage = stats.RoomLog?.RealDamageDealt ?? 0;
        var roomDamageLabel = MakeLabel($"{roomDamage}", 14, RoomColor);
        roomDamageLabel.CustomMinimumSize = new Vector2(60, 0);
        roomDamageLabel.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(roomDamageLabel);

        var turnDamage = stats.TurnLog?.RealDamageDealt ?? 0;
        var turnDamageLabel = MakeLabel($"{turnDamage}", 14, TurnColor);
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

    private static void UpdatePlayerRow(PanelContainer card, PlayerData playerData, int rowIndex)
    {
        var wrapper = card.GetChild(0) as VBoxContainer;
        if (wrapper == null) return;

        var row = wrapper.GetChild(0) as HBoxContainer;
        if (row == null) return;

        var labels = row.GetChildren().OfType<Label>().ToList();
        if (labels.Count < 5) return;

        var stats = _dataProvider.GetPlayerStats(playerData.PlayerInfo);
        if (stats == null) return;

        labels[1].Text = $"{stats.DamagePercent:F1}%";
        labels[2].Text = $"{stats.RunLog?.RealDamageDealt ?? 0}";
        labels[3].Text = $"{stats.RoomLog?.RealDamageDealt ?? 0}";
        labels[4].Text = $"{stats.TurnLog?.RealDamageDealt ?? 0}";

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

    private static Color DarkenColor(Color color, float amount)
    {
        return new Color(
            Math.Max(0, color.R - amount),
            Math.Max(0, color.G - amount),
            Math.Max(0, color.B - amount),
            color.A
        );
    }

    private static Label MakeLabel(string text, int fontSize, Color color, bool bold = false)
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

        _instance = new AttackLogPanel();
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        sceneTree?.Root.AddChild(_instance);
        LogState.Instance.IsLogPanelCreated = true;
    }

    public static void RefreshInstance()
    {
        AttackLogEventBus.Publish(new RefreshRequestedEvent());
    }

    public static void CreatePanel(RunState runState)
    {
        EnsureCreated();
        RefreshInstance();
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
