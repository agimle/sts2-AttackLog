using AttackLog.Service;
using AttackLog.State;
using AttackLog.View;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace AttackLog.Patch;

#region 战斗相关

public static partial class HookPatches
{
    /// <summary>
    /// 开始游戏时（不是进入游戏，是进入存档）
    /// 这个是添加进事件的，不是 Hook 里的
    /// </summary>
    public static void OnRunStartPostfix()
    {
        RunManager.Instance.RunStarted += OnRunStartedService.OnRunStarted;
    }
}


/// <summary>
/// 战斗相关的部分
/// </summary>
public static partial class HookPatches
{
    /// <summary>
    /// 战斗开始前
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    public static void BeforeCombatStartPostfix(IRunState? runState, CombatState? combatState)
    {
        // 创建新房间log
        BeforeCombatStartService.CreateNewRoomLog(runState, combatState);
        
        // 刷新面板
        AttackLogPanel.RefreshInstance();
    }
    
    /// <summary>
    /// 回合开始前
    /// </summary>
    /// <param name="combatState"></param>
    /// <param name="side"></param>
    public static void BeforeSideTurnStartPostfix(CombatState combatState, CombatSide side)
    {
        // 创建新回合log
        BeforeSideTurnStartService.CreateNewTurnLog(combatState, side);
        
        // 更新毒
        BeforeSideTurnStartService.UpdatePoisonPower(combatState, side);
        
        // 刷新面板
        AttackLogPanel.RefreshInstance();
    }

    /// <summary>
    /// 回合结束后
    /// </summary>
    /// <param name="combatState"></param>
    /// <param name="side"></param>
    public static void AfterTurnEndPostfix(CombatState combatState, CombatSide side)
    {
        // 保存回合
        AfterTurnEndService.SaveTurnLog(combatState, side);
        
        // 刷新面板
        AttackLogPanel.RefreshInstance();
    }
    
    /// <summary>
    /// 结束战斗后
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    public static void AfterCombatEndPostfix(IRunState? runState, CombatState? combatState)
    {
        // 保存房间log
        AfterCombatEndService.SaveRoomLog(runState, combatState);
        
        // 刷新面板
        AttackLogPanel.RefreshInstance();
        
        // 保存存档
        AfterCombatEndService.SaveLogState(runState, combatState);
    }
}
#endregion

#region 攻击相关
/// <summary>
/// 攻击相关的部分
/// </summary>
public static partial class HookPatches
{
    /// <summary>
    /// 造成伤害
    /// </summary>
    /// <param name="choiceContext"></param>
    /// <param name="dealer"></param>
    /// <param name="results"></param>
    /// <param name="props"></param>
    /// <param name="target"></param>
    /// <param name="cardSource"></param>
    public static void AfterDamageGivenPostfix(
        PlayerChoiceContext choiceContext, 
        Creature? dealer, 
        DamageResult results,
        ValueProp props, 
        Creature target, 
        CardModel? cardSource)
    {
        // 处理伤害统计
        AfterDamageGivenService.HandleDamageGiven(dealer, results, target, cardSource);
        
        
        // 刷新面板
        AttackLogPanel.RefreshInstance();
    }

    /// <summary>
    /// 生物死亡
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    /// <param name="creature"></param>
    /// <param name="wasRemovalPrevented"></param>
    /// <param name="deathAnimLength"></param>
    public static void AfterDeathPostfix(
        IRunState runState,
        CombatState? combatState, 
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength
    )
    {
        // 处理生物死亡
        AfterDeathService.ClearMonsterPower(creature);
        
        // 刷新面板
        AttackLogPanel.RefreshInstance();
    }
}
#endregion
