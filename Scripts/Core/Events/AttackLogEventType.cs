namespace AttackLog.Core;

/// <summary>
/// 事件类型枚举，定义框架中所有可用的事件类型
/// </summary>
public enum AttackLogEventType
{
    /// <summary>新 Run 开始</summary>
    RunStarted,
    /// <summary>战斗开始前</summary>
    BeforeCombatStart,
    /// <summary>回合开始前（区分敌我方）</summary>
    BeforeSideTurnStart,
    /// <summary>回合结束后</summary>
    AfterTurnEnd,
    /// <summary>战斗结束后</summary>
    AfterCombatEnd,
    /// <summary>伤害结算后</summary>
    AfterDamageGiven,
    /// <summary>生物死亡后</summary>
    AfterDeath,
    /// <summary>Power 被施加后</summary>
    PowerReceived,
    /// <summary>末日击杀</summary>
    DoomKill,
    /// <summary>请求刷新 UI</summary>
    RefreshRequested
}
