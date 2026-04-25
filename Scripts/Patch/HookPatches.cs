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

public static class HookPatches
{
    public static void OnRunStartPostfix()
    {
        RunManager.Instance.RunStarted += OnRunStarted;
    }

    public static void BeforeCombatStartPostfix(IRunState runState, CombatState? combatState)
    {
        if (combatState is null) return;
        AttackLogEventBus.Publish(new BeforeCombatStartEvent
        {
            RunState = runState,
            CombatState = combatState
        });
    }

    public static void BeforeSideTurnStartPostfix(CombatState combatState, CombatSide side)
    {
        AttackLogEventBus.Publish(new BeforeSideTurnStartEvent
        {
            CombatState = combatState,
            Side = side
        });
    }

    public static void AfterTurnEndPostfix(CombatState combatState, CombatSide side)
    {
        AttackLogEventBus.Publish(new AfterTurnEndEvent
        {
            CombatState = combatState,
            Side = side
        });
    }

    public static void AfterCombatEndPostfix(IRunState runState, CombatState? combatState, CombatRoom room)
    {
        if (combatState is null) return;
        AttackLogEventBus.Publish(new AfterCombatEndEvent
        {
            RunState = runState,
            CombatState = combatState
        });
    }

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

    public static void AfterDeathPostfix(IRunState runState, CombatState? combatState, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        AttackLogEventBus.Publish(new AfterDeathEvent
        {
            Creature = creature
        });
    }

    private static void OnRunStarted(RunState runState)
    {
        AttackLogEventBus.Publish(new RunStartedEvent
        {
            RunState = runState
        });
    }
}
