using AttackLog.Model;
using AttackLog.Save;
using AttackLog.State;
using AttackLog.View;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

/// <summary>
/// 进入游戏时
/// </summary>
public static class OnRunStartedService
{
    public static void OnRunStarted(IRunState runState)
    {
        CreateNewRun(runState);
        RegisterPlayers(runState);
        
        LoadLogState(runState);
        
        // 创建面板
        if (!LogState.Instance.IsLogPanelCreated)
        {
            AttackLogPanel.EnsureCreated();
        }
        // 刷新面板
        AttackLogPanel.RefreshInstance();
    }
    
    /// <summary>
    /// 创建新一轮游戏记录
    /// </summary>
    /// <param name="runState"></param>
    private static void CreateNewRun(IRunState runState)
    {
        if (LogState.Instance.RunLog is null || !LogState.Instance.RunLog.IsSameRun(runState))
        {
            LogState.Instance.RunLog = new RunLog(runState);
            LogState.Instance.InvalidateCache();
        }
    }
    
    /// <summary>
    /// 向RunLog里注册玩家
    /// </summary>
    /// <param name="runState"></param>
    private static void RegisterPlayers(IRunState runState)
    {
        if (LogState.Instance.RunLog is null || !LogState.Instance.RunLog.IsSameRun(runState))
        {
            CreateNewRun(runState);
        }
        
        foreach (var player in runState.Players)
        {
            LogState.Instance.RunLog?.RegisterPlayer(player);
        }
        
        LogState.Instance.InvalidateCache();
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="runState"></param>
    private static void LoadLogState(IRunState runState)
    {
        ModSaveUtils.Load(runState);
    }
}