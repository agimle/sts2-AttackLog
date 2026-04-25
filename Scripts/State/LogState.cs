using AttackLog.Model;
using AttackLog.Save;

namespace AttackLog.State;

/// <summary>
/// 全局状态门面（Facade），统一管理 RunState、CombatState 和 ViewCache。
/// 作为框架的核心入口，Service 层通过此类访问所有状态。
/// 采用单例模式，确保全局唯一的状态实例。
/// </summary>
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

    /// <summary>Run 级状态</summary>
    private readonly AttackLogRunState _runState;

    /// <summary>战斗级状态</summary>
    private readonly AttackLogCombatState _combatState;

    /// <summary>视图缓存</summary>
    private readonly ViewCache _viewCache;

    /// <summary>日志面板是否已创建</summary>
    public bool IsLogPanelCreated { get; set; }

    #region RunState 代理属性

    /// <summary>当前 Run 的日志</summary>
    public RunLog? RunLog
    {
        get => _runState.RunLog;
        set => _runState.RunLog = value;
    }

    /// <summary>Mod 存档数据</summary>
    public ModSave? ModSave
    {
        get => _runState.ModSave;
        set => _runState.ModSave = value;
    }

    #endregion

    #region CombatState 代理属性

    /// <summary>玩家 → 房间日志数据映射</summary>
    public Dictionary<PlayerInfo, SingleRoomLogData> RoomLogsData
    {
        get => _combatState.RoomLogsData;
        set => _combatState.RoomLogsData = value;
    }

    /// <summary>玩家 → 回合日志数据映射</summary>
    public Dictionary<PlayerInfo, SingleTurnLogData> TurnLogsData
    {
        get => _combatState.TurnLogsData;
        set => _combatState.TurnLogsData = value;
    }

    /// <summary>当前战斗的怪物记录</summary>
    public CombatRecord CombatRecord
    {
        get => _combatState.CombatRecord;
        set => _combatState.CombatRecord = value;
    }

    #endregion

    #region ViewCache 代理方法

    /// <summary>
    /// 使视图缓存失效，下次访问时自动重建
    /// </summary>
    public void InvalidateCache()
    {
        _viewCache.Invalidate();
    }

    /// <summary>
    /// 获取指定玩家的缓存统计数据
    /// </summary>
    /// <param name="info">玩家信息</param>
    /// <returns>缓存统计，不存在返回 null</returns>
    public CachedPlayerStats? GetCachedStats(PlayerInfo info)
    {
        return _viewCache.GetStats(info);
    }

    /// <summary>
    /// 获取所有玩家的缓存统计数据
    /// </summary>
    /// <returns>玩家信息 → 缓存统计的映射</returns>
    public Dictionary<PlayerInfo, CachedPlayerStats> GetAllCachedStats()
    {
        return _viewCache.GetAllStats();
    }

    #endregion

    #region 子状态访问

    /// <summary>获取 Run 级状态实例</summary>
    public AttackLogRunState GetRunState() => _runState;

    /// <summary>获取战斗级状态实例</summary>
    public AttackLogCombatState GetCombatState() => _combatState;

    /// <summary>获取视图缓存实例</summary>
    public ViewCache GetViewCache() => _viewCache;

    #endregion

    #region 生命周期方法

    /// <summary>
    /// 战斗开始回调，清空战斗状态并使缓存失效
    /// </summary>
    public void OnCombatStart()
    {
        _combatState.OnCombatStart();
        InvalidateCache();
    }

    /// <summary>
    /// 战斗结束回调，清空战斗状态并使缓存失效
    /// </summary>
    public void OnCombatEnd()
    {
        _combatState.OnCombatEnd();
        InvalidateCache();
    }

    #endregion
}
