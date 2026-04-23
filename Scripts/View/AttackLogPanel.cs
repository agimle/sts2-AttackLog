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
    private const float MinPanelHeight = 200f;
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

    public static void SetDataProvider(ILogDataProvider provider)
    {
        _dataProvider = provider ?? LogDataProvider.Default;
    }

    public override void _EnterTree()
    {
        Layer = 100;
        Name = nameof(AttackLogPanel);
    }

    public override void _ExitTree()
    {
        if (ReferenceEquals(_instance, this)) _instance = null;
        UnsubscribeAll();
    }

    public override void _Ready()
    {
        BuildUi();
        Refresh();
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
            Position = new Vector2(16, 16),
            CustomMinimumSize = new Vector2(PanelWidth, MinPanelHeight)
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

        var title = MakeLabel("⚔ 战斗统计", 16, White, true);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        col.AddChild(title);

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
    }

    private void Refresh()
    {
        if (_playerList == null || _emptyLabel == null) return;

        if (!_dataProvider.HasActiveRun)
        {
            _emptyLabel.Visible = true;
            _emptyLabel.Text = "No active run";
            ClearAllRows();
            return;
        }

        var players = _dataProvider.GetPlayers();
        if (players.Count == 0)
        {
            _emptyLabel.Visible = true;
            _emptyLabel.Text = "No players registered";
            ClearAllRows();
            return;
        }

        _emptyLabel.Visible = false;

        var currentPlayerInfos = new HashSet<PlayerInfo>(players.Select(p => p.PlayerInfo));

        var toRemove = _playerRows.Keys
            .Where(info => !currentPlayerInfos.Contains(info))
            .ToList();

        foreach (var info in toRemove)
        {
            if (_playerRows.TryGetValue(info, out var row))
            {
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
    }

    private void ClearAllRows()
    {
        foreach (var row in _playerRows.Values)
        {
            row.QueueFree();
        }
        _playerRows.Clear();
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

        card.AddChild(row);
        return card;
    }

    private static void UpdatePlayerRow(PanelContainer card, PlayerData playerData, int rowIndex)
    {
        var row = card.GetChild(0) as HBoxContainer;
        if (row == null) return;

        var labels = row.GetChildren().OfType<Label>().ToList();
        if (labels.Count < 5) return;

        var stats = _dataProvider.GetPlayerStats(playerData.PlayerInfo);
        if (stats == null) return;

        labels[1].Text = $"{stats.DamagePercent:F1}%";
        labels[2].Text = $"{stats.RunLog?.RealDamageDealt ?? 0}";
        labels[3].Text = $"{stats.RoomLog?.RealDamageDealt ?? 0}";
        labels[4].Text = $"{stats.TurnLog?.RealDamageDealt ?? 0}";
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

        _instance.SubscribeAll();
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
