using System.Security.Cryptography;
using AttackLog.Model;
using AttackLog.Save;
using MegaCrit.Sts2.Core.Combat;

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
        RunLog =  null;
        RoomLogsData = new Dictionary<PlayerInfo,SingleRoomLogData>();
        TurnLogsData = new Dictionary<PlayerInfo,SingleTurnLogData>();
        _viewCache = new ViewCache();
    }
    #endregion
    
    public ModSave? ModSave;
    
    public bool IsLogPanelCreated  { get; set; }
    
    public RunLog? RunLog { get; set; }
    
    public Dictionary<PlayerInfo,SingleRoomLogData> RoomLogsData { get; set; }
    public Dictionary<PlayerInfo,SingleTurnLogData> TurnLogsData { get; set; }
    
    public CombatRecord CombatRecord { get; set; } =  new CombatRecord();

    private ViewCache _viewCache;

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
}