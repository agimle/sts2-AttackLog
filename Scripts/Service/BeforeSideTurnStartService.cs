using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

/// <summary>
/// 回合开始前
/// </summary>
public static class BeforeSideTurnStartService
{
    /// <summary>
    /// 创建新回合记录
    /// </summary>
    /// <param name="combatState"></param>
    /// <param name="side"></param>
    public static void CreateNewTurnLog(CombatState combatState, CombatSide side)
    {
        if(side != CombatSide.Player) return;
        
        if (LogState.Instance.RunLog is null) return;
        if (!LogState.Instance.RunLog.IsSameRun(combatState.RunState)) return;
        
        // 清空上一个回合的记录
        LogState.Instance.TurnLogsData.Clear();

        // 遍历所有注册过的用户
        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            SingleTurnLogData newTurnLogData = new  SingleTurnLogData();
            LogState.Instance.TurnLogsData.TryAdd(player.PlayerInfo, newTurnLogData);
        }
        
        LogState.Instance.InvalidateCache();
    }      
    

    /// <summary>
    /// 更新PoisonPower的层数
    /// </summary>
    /// <param name="combatState"></param>
    /// <param name="side"></param>
    public static void UpdatePoisonPower(CombatState combatState, CombatSide side)
    {
        if(side != CombatSide.Player) 
        {
            return;
        }
        
        
        foreach (var monsterRecord in LogState.Instance.CombatRecord.MonsterRecords)
        {
            foreach (var powerRecord in monsterRecord.Value.Powers)
            {
                if (powerRecord.Key == typeof(PoisonPower))
                {
                    if(powerRecord.Value.Count == 0) continue;
                    
                    powerRecord.Value.Peek().Amount--;
                    
                    if (powerRecord.Value.Peek().Amount <= 0)
                    {
                        powerRecord.Value.Dequeue();

                    }
                }
            }
        }
    }
}