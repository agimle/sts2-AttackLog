using AttackLog.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace AttackLog.Patch;

/// <summary>
/// 游戏 Hook 补丁集合，将游戏生命周期事件转发到 EventBus。
/// 每个方法对应一个 Harmony postfix，由 PatchUtils 注册到游戏 Hook 系统
/// </summary>
public static class HookPatches
{
    /// <summary>
    /// Run 启动 Hook 的初始化入口，订阅 RunManager.RunStarted 事件
    /// </summary>
    public static void OnRunStartPostfix()
    {
        RunManager.Instance.RunStarted += OnRunStarted;
    }

    /// <summary>
    /// 战斗开始前回调，发布 BeforeCombatStartEvent
    /// </summary>
    public static void BeforeCombatStartPostfix(IRunState runState, CombatState? combatState)
    {
        if (combatState is null) return;
        AttackLogEventBus.Publish(new BeforeCombatStartEvent
        {
            RunState = runState,
            CombatState = combatState
        });
    }

    /// <summary>
    /// 回合方开始前回调，发布 BeforeSideTurnStartEvent
    /// </summary>
    public static void BeforeSideTurnStartPostfix(CombatState combatState, CombatSide side)
    {
        AttackLogEventBus.Publish(new BeforeSideTurnStartEvent
        {
            CombatState = combatState,
            Side = side
        });
    }

    /// <summary>
    /// 回合结束后回调，发布 AfterTurnEndEvent
    /// </summary>
    public static void AfterTurnEndPostfix(CombatState combatState, CombatSide side)
    {
        AttackLogEventBus.Publish(new AfterTurnEndEvent
        {
            CombatState = combatState,
            Side = side
        });
    }

    /// <summary>
    /// 战斗结束后回调，发布 AfterCombatEndEvent
    /// </summary>
    public static void AfterCombatEndPostfix(IRunState runState, CombatState? combatState, CombatRoom room)
    {
        if (combatState is null) return;
        AttackLogEventBus.Publish(new AfterCombatEndEvent
        {
            RunState = runState,
            CombatState = combatState
        });
    }

    /// <summary>
    /// 伤害结算后回调，发布 AfterDamageGivenEvent
    /// </summary>
    public static void AfterDamageGivenPostfix(PlayerChoiceContext choiceContext, CombatState combatState, Creature? dealer, DamageResult results, ValueProp props, Creature target, CardModel? cardSource)
    {
        AttackLogEventBus.Publish(new AfterDamageGivenEvent
        {
            Dealer = dealer,
            Result = results,
            Target = target,
            CardSource = cardSource
        });
    }

    /// <summary>
    /// 生物死亡后回调，发布 AfterDeathEvent
    /// </summary>
    public static void AfterDeathPostfix(IRunState runState, CombatState? combatState, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        AttackLogEventBus.Publish(new AfterDeathEvent
        {
            Creature = creature
        });
    }

    /// <summary>
    /// Run 启动事件处理器，发布 RunStartedEvent
    /// </summary>
    private static void OnRunStarted(RunState runState)
    {
        AttackLogEventBus.Publish(new RunStartedEvent
        {
            RunState = runState
        });
    }
}
