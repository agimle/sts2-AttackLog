using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

public static class AttackLogViewUtils
{
    /// <summary>
    /// 获取玩家当前回合实时数据
    /// </summary>
    /// <param name="playerInfo"></param>
    /// <returns></returns>
    public static AttackLogModel? GetPlayerTurnLog(PlayerInfo playerInfo)
    {
        if (LogState.Instance.RunLog == null) return null;

        return LogState.Instance.TurnLogsData.TryGetValue(playerInfo, out var turnLogData)
            ? turnLogData.TurnLogSum
            : new AttackLogModel();
    }
    
    /// <summary>
    /// 获取玩家当前房间实时数据
    /// </summary>
    /// <param name="playerInfo"></param>
    /// <returns></returns>
    public static AttackLogModel? GetPlayerRoomLog(PlayerInfo playerInfo)
    {
        if (LogState.Instance.RunLog == null) return null;

        AttackLogModel? turnLog = GetPlayerTurnLog(playerInfo);
        if (turnLog == null) turnLog = new AttackLogModel();
        
        return LogState.Instance.RoomLogsData.TryGetValue(playerInfo, out var roomLogData)
            ? AttackLogModel.Sum(roomLogData.RoomLogSum, turnLog)
            : new AttackLogModel();
    }

    /// <summary>
    /// 获取玩家当前游戏实时数据
    /// </summary>
    /// <param name="playerInfo"></param>
    /// <returns></returns>
    public static AttackLogModel? GetPlayerRunLog(PlayerInfo playerInfo)
    {
        if(LogState.Instance.RunLog == null) return null;
        
        AttackLogModel? roomLog = GetPlayerRoomLog(playerInfo);
        if (roomLog == null) roomLog = new AttackLogModel();
        
        PlayerData? playerData = LogState.Instance.RunLog.GetPlayerByCreature(playerInfo.Creature);
        if(playerData == null) return new AttackLogModel();
        
        return AttackLogModel.Sum(playerData.RunLogData.RunLogSum, roomLog);
    }
    
    /// <summary>
    /// 获取全部数据
    /// </summary>
    /// <param name="playerInfo"></param>
    /// <param name="turnLog"></param>
    /// <param name="roomLog"></param>
    /// <param name="runLog"></param>
    public static void GetPlayerLog(PlayerInfo playerInfo, out AttackLogModel? turnLog, out AttackLogModel? roomLog, out AttackLogModel? runLog)
    {
        turnLog = GetPlayerTurnLog(playerInfo);
        roomLog = GetPlayerRoomLog(playerInfo);
        runLog = GetPlayerRunLog(playerInfo);
    }


    /// <summary>
    /// 获取伤害百分比
    /// </summary>
    /// <param name="playerInfo"></param>
    /// <param name="damagePercent"></param>
    public static void GetDamagePercent(PlayerInfo playerInfo, out float damagePercent)
    {
        damagePercent = 0f;
        
        var runLog = LogState.Instance.RunLog;
        if (runLog == null) return;
        
        var players = runLog.GetAllPlayers();
        if (players.Count == 0) return;
        
        int totalDamage = 0;
        int playerDamage = 0;
        
        foreach (var player in players)
        {
            GetPlayerLog(player.PlayerInfo, out _, out _, out var runLogData);
            int damage = runLogData?.RealDamageDealt ?? 0;
            totalDamage += damage;
            
            if (player.PlayerInfo.Equals(playerInfo))
            {
                playerDamage = damage;
            }
        }
        
        if (totalDamage == 0) return;
        
        damagePercent = (float)playerDamage / totalDamage * 100f;
    }
    
    /// <summary>
    /// 获取所有玩家的总伤害
    /// </summary>
    /// <returns></returns>
    public static int GetTotalDamage()
    {
        var runLog = LogState.Instance.RunLog;
        if (runLog == null) return 0;
        
        var players = runLog.GetAllPlayers();
        int totalDamage = 0;
        
        foreach (var player in players)
        {
            GetPlayerLog(player.PlayerInfo, out _, out _, out var runLogData);
            totalDamage += runLogData?.RealDamageDealt ?? 0;
        }
        
        return totalDamage;
    }
}