namespace AttackLog.Core;

public enum AttackLogEventType
{
    RunStarted,
    BeforeCombatStart,
    BeforeSideTurnStart,
    AfterTurnEnd,
    AfterCombatEnd,
    AfterDamageGiven,
    AfterDeath,
    PowerReceived,
    DoomKill,
    RefreshRequested
}
