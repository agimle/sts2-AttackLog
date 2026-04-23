using System.Security.Cryptography;
using AttackLog.Model;
using AttackLog.Save;
using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.State;

/// <summary>
/// 记录
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
        RunLog =  null;
        RoomLogsData = new Dictionary<PlayerInfo,SingleRoomLogData>();
        TurnLogsData = new Dictionary<PlayerInfo,SingleTurnLogData>();
    }
    #endregion
    
    /// <summary>
    /// 模组存档
    /// </summary>
    public ModSave? ModSave;
    
    /// <summary>
    /// 显示窗口是否创建
    /// </summary>
    public bool IsLogPanelCreated  { get; set; }
    
    /// <summary>
    /// 当前游戏
    /// </summary>
    public RunLog? RunLog { get; set; }
    
    /// <summary>
    /// 当前房间所有玩家记录
    /// </summary>
    public Dictionary<PlayerInfo,SingleRoomLogData> RoomLogsData { get; set; }
    /// <summary>
    /// 当前回合所有玩家记录
    /// </summary>
    public Dictionary<PlayerInfo,SingleTurnLogData> TurnLogsData { get; set; }
    
    
    
    /// <summary>
    /// 当前战斗记录
    /// </summary>
    public CombatRecord CombatRecord { get; set; } =  new CombatRecord();
}