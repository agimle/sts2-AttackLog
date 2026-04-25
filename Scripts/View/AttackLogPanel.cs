using Godot;
using AttackLog.Core;
using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.View;

/// <summary>
/// 战斗统计面板，以浮动窗口形式显示各玩家的伤害统计。
/// 订阅所有游戏事件并按节流间隔刷新 UI，支持拖拽移动、排序切换和增量更新。
/// </summary>
public sealed partial class AttackLogPanel : CanvasLayer
{
    /// <summary>面板宽度</summary>
    private const float PanelWidth = 460f;

    /// <summary>获取面板宽度</summary>
    public static float GetPanelWidth() => PanelWidth;

    /// <summary>刷新节流间隔（秒）</summary>
    private const float RefreshInterval = 0.1f;

    /// <summary>触发面板刷新的事件类型列表</summary>
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

    /// <summary>面板单例实例</summary>
    private static AttackLogPanel? _instance;

    /// <summary>数据提供者，默认使用 LogDataProvider</summary>
    private static ILogDataProvider _dataProvider = LogDataProvider.Default;

    /// <summary>面板根容器</summary>
    private PanelContainer? _root;

    /// <summary>玩家列表容器</summary>
    private VBoxContainer? _playerList;

    /// <summary>空状态提示标签</summary>
    private Label? _emptyLabel;

    /// <summary>排序切换按钮</summary>
    private Button? _sortButton;

    /// <summary>玩家信息 → 行 UI 的映射</summary>
    private Dictionary<PlayerInfo, PanelContainer> _playerRows = new();

    /// <summary>上次刷新时间戳</summary>
    private float _lastRefreshTime = 0f;

    /// <summary>是否有待刷新请求</summary>
    private bool _refreshRequested = false;

    /// <summary>拖拽处理器</summary>
    private PanelDragHandler _dragHandler = new();

    /// <summary>排序控制器</summary>
    private PanelSortController _sortController;

    /// <summary>玩家行工厂</summary>
    private PlayerRowFactory _rowFactory;

    /// <summary>构造函数，初始化排序控制器和行工厂</summary>
    public AttackLogPanel()
    {
        _sortController = new PanelSortController(LogState.Instance);
        _rowFactory = new PlayerRowFactory(_dataProvider);
    }

    /// <summary>
    /// 替换数据提供者，支持自定义实现（如测试 Mock）
    /// </summary>
    /// <param name="provider">新的数据提供者</param>
    public static void SetDataProvider(ILogDataProvider provider)
    {
        _dataProvider = provider ?? LogDataProvider.Default;
    }

    /// <summary>进入场景树：设置层级、订阅事件</summary>
    public override void _EnterTree()
    {
        Layer = 100;
        Name = nameof(AttackLogPanel);
        _instance = this;
        SubscribeAll();
    }

    /// <summary>退出场景树：取消订阅事件、清理单例引用</summary>
    public override void _ExitTree()
    {
        UnsubscribeAll();
        if (ReferenceEquals(_instance, this))
            _instance = null;
    }

    /// <summary>初始化：构建 UI、设置位置、首次刷新</summary>
    public override void _Ready()
    {
        BuildUi();
        SetInitialPosition();
        Refresh();
        _dragHandler.DragEnded += OnDragEnded;
    }

    /// <summary>拖拽结束回调，保存面板位置</summary>
    private void OnDragEnded()
    {
        if (_root != null)
        {
            ModSaveUtils.SavePanelPosition(true, _root.Position.X, _root.Position.Y);
        }
    }

    /// <summary>每帧处理：节流刷新 UI</summary>
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

    /// <summary>
    /// 游戏事件回调，请求刷新面板
    /// </summary>
    private void OnGameEvent(IAttackLogEvent e)
    {
        RequestRefresh();
    }

    /// <summary>
    /// 请求刷新面板，实现节流机制：超过间隔立即刷新，否则标记待刷新
    /// </summary>
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

    /// <summary>订阅所有触发刷新的事件</summary>
    private void SubscribeAll()
    {
        foreach (var eventType in RefreshOnEvents)
        {
            AttackLogEventBus.Subscribe(eventType, OnGameEvent);
        }
    }

    /// <summary>取消订阅所有事件</summary>
    private void UnsubscribeAll()
    {
        foreach (var eventType in RefreshOnEvents)
        {
            AttackLogEventBus.Unsubscribe(eventType, OnGameEvent);
        }
    }

    /// <summary>
    /// 构建面板 UI 结构：标题栏、表头、玩家列表、空状态提示
    /// </summary>
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
            CustomMinimumSize = new Vector2(65, 28)
        };
        _sortButton.AddThemeFontSizeOverride("font_size", 13);
        _sortButton.Pressed += OnSortToggled;
        ApplySortButtonColors(PanelTheme.TotalColor);
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

    /// <summary>
    /// 设置面板初始位置（屏幕右侧偏上）
    /// </summary>
    private void SetInitialPosition()
    {
        if (_root == null) return;
        var viewport = GetViewport();
        if (viewport == null) return;

        var savedPos = ModSaveUtils.GetPanelPosition(true);
        if (savedPos.HasValue)
        {
            _root.Position = ClampToScreen(savedPos.Value, _root.Size);
            return;
        }

        var screenSize = viewport.GetVisibleRect().Size;
        float x = screenSize.X - PanelWidth - 16f;
        float y = screenSize.Y * 0.1f;
        _root.Position = ClampToScreen(new Vector2(x, y), _root.Size);
    }

    /// <summary>
    /// 将面板位置限制在屏幕范围内
    /// </summary>
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

    /// <summary>
    /// 根节点输入事件处理，转发给拖拽处理器
    /// </summary>
    private void OnRootGuiInput(InputEvent @event)
    {
        if (_root != null)
        {
            _dragHandler.HandleInput(@event, _root);
        }
    }

    /// <summary>
    /// 排序切换回调，切换排序维度并刷新面板
    /// </summary>
    private void OnSortToggled()
    {
        _sortController.Toggle();
        UpdateSortButton();
        ClearAllRows();
        ResetRootSize();
        Refresh();
    }

    /// <summary>更新排序按钮文本和颜色</summary>
    private void UpdateSortButton()
    {
        if (_sortButton == null) return;
        bool isTotal = _sortController.SortByTotal;
        _sortButton.Text = isTotal ? "▼ 总计" : "▼ 房间";
        ApplySortButtonColors(isTotal ? PanelTheme.TotalColor : PanelTheme.RoomColor);
    }

    /// <summary>应用排序按钮的主题颜色</summary>
    private void ApplySortButtonColors(Color color)
    {
        if (_sortButton == null) return;
        _sortButton.AddThemeColorOverride("font_color", color);
        _sortButton.AddThemeColorOverride("font_hover_color", color);
        _sortButton.AddThemeColorOverride("font_pressed_color", color);

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
        _sortButton.AddThemeStyleboxOverride("normal", btnStyle);
        _sortButton.AddThemeStyleboxOverride("hover", btnStyle);
        _sortButton.AddThemeStyleboxOverride("pressed", btnStyle);
        _sortButton.AddThemeStyleboxOverride("focus", btnStyle);
    }

    /// <summary>
    /// 刷新面板内容：获取玩家数据、排序、增量更新行
    /// </summary>
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

    /// <summary>
    /// 按排序后的 PlayerInfo 顺序重排 PlayerData 列表
    /// </summary>
    private List<PlayerData> SortPlayerDataByInfo(List<PlayerData> players, List<KeyValuePair<PlayerInfo, CachedPlayerStats>> sortedInfos)
    {
        var infoOrder = sortedInfos.Select((kv, i) => (kv.Key, i)).ToDictionary(x => x.Key, x => x.i);
        return players.OrderBy(p => infoOrder.TryGetValue(p.PlayerInfo, out var idx) ? idx : int.MaxValue).ToList();
    }

    /// <summary>
    /// 按排序顺序重排玩家行 UI 节点
    /// </summary>
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

    /// <summary>清除所有玩家行并释放资源</summary>
    private void ClearAllRows()
    {
        foreach (var row in _playerRows.Values)
        {
            row.GetParent()?.RemoveChild(row);
            row.QueueFree();
        }
        _playerRows.Clear();
    }

    /// <summary>重置根容器尺寸以适应内容</summary>
    private void ResetRootSize()
    {
        if (_root == null) return;
        _root.ResetSize();
    }

    /// <summary>创建水平分隔线控件</summary>
    private static Control HLine(int height, Color color)
    {
        return new Control
        {
            CustomMinimumSize = new Vector2(0, height),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }

    /// <summary>确保面板已创建并添加到场景树</summary>
    public static void EnsureCreated()
    {
        if (_instance != null) return;

        _instance = new AttackLogPanel();
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        sceneTree?.Root.AddChild(_instance);
        LogState.Instance.IsLogPanelCreated = true;
    }

    /// <summary>请求刷新面板实例</summary>
    public static void RefreshInstance()
    {
        AttackLogEventBus.Publish(new RefreshRequestedEvent());
    }

    /// <summary>创建面板并刷新（外部调用入口）</summary>
    public static void CreatePanel(RunState runState)
    {
        EnsureCreated();
        RefreshInstance();
    }

    /// <summary>订阅 RunStarted 事件以自动创建面板</summary>
    public static void SubscribeCreationEvent()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.RunStarted, OnRunStartedForCreation);
    }

    /// <summary>RunStarted 事件回调，自动创建面板并刷新</summary>
    private static void OnRunStartedForCreation(IAttackLogEvent e)
    {
        EnsureCreated();
        RefreshInstance();
    }
}
