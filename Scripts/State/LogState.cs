using AttackLog.Model;
using AttackLog.Save;

namespace AttackLog.State;

public class LogState
{
    #region 单例模式

    private static LogState? _instance;
    public static LogState Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LogState();
            }
            return _instance;
        }
    }
    private LogState()
    {
        IsLogPanelCreated = false;
        _runState = new AttackLogRunState();
        _combatState = new AttackLogCombatState();
        _viewCache = new ViewCache(
            () => _runState.RunLog,
            () => _combatState.TurnLogsData,
            () => _combatState.RoomLogsData
        );
    }
    #endregion

    private readonly AttackLogRunState _runState;
    private readonly AttackLogCombatState _combatState;
    private readonly ViewCache _viewCache;

    public bool IsLogPanelCreated { get; set; }

    #region RunState 代理属性

    public RunLog? RunLog
    {
        get => _runState.RunLog;
        set => _runState.RunLog = value;
    }

    public ModSave? ModSave
    {
        get => _runState.ModSave;
        set => _runState.ModSave = value;
    }

    #endregion

    #region CombatState 代理属性

    public Dictionary<PlayerInfo, SingleRoomLogData> RoomLogsData
    {
        get => _combatState.RoomLogsData;
        set => _combatState.RoomLogsData = value;
    }

    public Dictionary<PlayerInfo, SingleTurnLogData> TurnLogsData
    {
        get => _combatState.TurnLogsData;
        set => _combatState.TurnLogsData = value;
    }

    public CombatRecord CombatRecord
    {
        get => _combatState.CombatRecord;
        set => _combatState.CombatRecord = value;
    }

    #endregion

    #region ViewCache 代理方法

    public void InvalidateCache()
    {
        _viewCache.Invalidate();
    }

    public CachedPlayerStats? GetCachedStats(PlayerInfo info)
    {
        return _viewCache.GetStats(info);
    }

    public Dictionary<PlayerInfo, CachedPlayerStats> GetAllCachedStats()
    {
        return _viewCache.GetAllStats();
    }

    #endregion

    #region 子状态访问

    public AttackLogRunState GetRunState() => _runState;
    public AttackLogCombatState GetCombatState() => _combatState;
    public ViewCache GetViewCache() => _viewCache;

    #endregion

    #region 生命周期方法

    public void OnCombatStart()
    {
        _combatState.OnCombatStart();
        InvalidateCache();
    }

    public void OnCombatEnd()
    {
        _combatState.OnCombatEnd();
        InvalidateCache();
    }

    #endregion
}
