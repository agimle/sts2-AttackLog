using Godot;
using AttackLog.Core;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.View;

public sealed partial class AttackLogPanel : CanvasLayer
{
    private const float PanelWidth = 460f;
    private const float RefreshInterval = 0.1f;

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
    private Button? _sortButton;

    private Dictionary<PlayerInfo, PanelContainer> _playerRows = new();
    private float _lastRefreshTime = 0f;
    private bool _refreshRequested = false;

    private PanelDragHandler _dragHandler = new();
    private PanelSortController _sortController;
    private PlayerRowFactory _rowFactory;

    public AttackLogPanel()
    {
        _sortController = new PanelSortController(LogState.Instance);
        _rowFactory = new PlayerRowFactory(_dataProvider);
    }

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
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);

        var col = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        col.AddThemeConstantOverride("separation", 6);

        var titleBar = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        titleBar.AddThemeConstantOverride("separation", 6);

        var title = PanelTheme.MakeLabel("⚔ 战斗统计", 16, PanelTheme.White);
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
        ApplySortButtonColors(PanelTheme.TotalColor);
        var btnEmptyStyle = new StyleBoxEmpty();
        _sortButton.AddThemeStyleboxOverride("normal", btnEmptyStyle);
        _sortButton.AddThemeStyleboxOverride("hover", btnEmptyStyle);
        _sortButton.AddThemeStyleboxOverride("pressed", btnEmptyStyle);
        _sortButton.AddThemeStyleboxOverride("focus", btnEmptyStyle);
        _sortButton.Pressed += OnSortToggled;
        titleBar.AddChild(_sortButton);

        col.AddChild(titleBar);

        var separator = HLine(1, PanelTheme.Border);
        col.AddChild(separator);

        var headerMargin = new MarginContainer();
        headerMargin.AddThemeConstantOverride("margin_left", 8);
        headerMargin.AddThemeConstantOverride("margin_right", 8);
        var headerRow = PlayerRowFactory.BuildHeaderRow();
        headerMargin.AddChild(headerRow);
        col.AddChild(headerMargin);

        var separator2 = HLine(1, PanelTheme.Border);
        col.AddChild(separator2);

        _playerList = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _playerList.AddThemeConstantOverride("separation", 6);
        col.AddChild(_playerList);

        _emptyLabel = PanelTheme.MakeLabel("No players registered", 12, PanelTheme.Gray);
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
        if (_root != null)
        {
            _dragHandler.HandleInput(@event, _root);
        }
    }

    private void OnSortToggled()
    {
        _sortController.Toggle();
        UpdateSortButton();
        ClearAllRows();
        ResetRootSize();
        Refresh();
    }

    private void UpdateSortButton()
    {
        if (_sortButton == null) return;
        bool isTotal = _sortController.SortByTotal;
        _sortButton.Text = isTotal ? "▼ 总计" : "▼ 房间";
        ApplySortButtonColors(isTotal ? PanelTheme.TotalColor : PanelTheme.RoomColor);
    }

    private void ApplySortButtonColors(Color color)
    {
        if (_sortButton == null) return;
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

        var sorted = _sortController.GetSortedPlayers();
        var sortedPlayerData = SortPlayerDataByInfo(players, sorted);

        _emptyLabel.Visible = false;

        var currentPlayerInfos = new HashSet<PlayerInfo>(sortedPlayerData.Select(p => p.PlayerInfo));

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
        foreach (var playerData in sortedPlayerData)
        {
            if (_playerRows.TryGetValue(playerData.PlayerInfo, out var existingRow))
            {
                _rowFactory.UpdateRow(existingRow, playerData);
            }
            else
            {
                var newRow = _rowFactory.BuildRow(playerData);
                _playerList.AddChild(newRow);
                _playerRows[playerData.PlayerInfo] = newRow;
            }
            index++;
        }

        ReorderPlayerRows(sortedPlayerData);

        ResetRootSize();
    }

    private List<PlayerData> SortPlayerDataByInfo(List<PlayerData> players, List<KeyValuePair<PlayerInfo, CachedPlayerStats>> sortedInfos)
    {
        var infoOrder = sortedInfos.Select((kv, i) => (kv.Key, i)).ToDictionary(x => x.Key, x => x.i);
        return players.OrderBy(p => infoOrder.TryGetValue(p.PlayerInfo, out var idx) ? idx : int.MaxValue).ToList();
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
