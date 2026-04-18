# Hook系统接口文档

## 概述

Hook系统是杀戮尖塔2的核心事件系统，允许Mod在游戏的关键时刻插入自定义逻辑。Hook系统通过 `Hook` 静态类提供接口，所有Hook方法都定义在 `AbstractModel` 基类中作为虚方法。

***

## 命名空间

```csharp
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
```

***

## 文件路径

| 文件                      | 路径                                     |
| ----------------------- | -------------------------------------- |
| Hook.cs                 | src/Core/Hooks/Hook.cs                 |
| AbstractModel.cs        | src/Core/Models/AbstractModel.cs       |
| ModifyDamageHookType.cs | src/Core/Hooks/ModifyDamageHookType.cs |

***

## Hook系统架构

### Hook静态类

`Hook` 类是一个静态类，包含所有Hook触发方法。游戏核心代码在关键时刻调用这些方法。

```csharp
public static class Hook
{
    // 所有Hook方法都是静态的
}
```

### AbstractModel基类

所有可以响应Hook的模型类都继承自 `AbstractModel`，其中定义了所有Hook响应方法作为虚方法。

```csharp
public abstract class AbstractModel
{
    // Hook响应方法都是虚方法，默认返回Task.CompletedTask或默认值
    public virtual Task BeforeAttack(AttackCommand command)
    {
        return Task.CompletedTask;
    }
}
```

### 可响应Hook的模型类型

以下模型类型可以响应Hook：

- **RelicModel** - 遗物
- **PowerModel** - 能力
- **CardModel** - 卡牌
- **OrbModel** - 球体
- **MonsterModel** - 怪物
- **PotionModel** - 药水
- **AfflictionModel** - 负面效果
- **EnchantmentModel** - 附魔
- **ModifierModel** - 修改器

***

## Hook方法分类

### 1. 战斗相关Hook

#### BeforeCombatStart / BeforeCombatStartLate

战斗开始前触发。

```csharp
public static async Task BeforeCombatStart(IRunState runState, CombatState? combatState)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeCombatStart()
public virtual Task BeforeCombatStartLate()
```

***

#### AfterCombatEnd

战斗结束时触发。

```csharp
public static async Task AfterCombatEnd(IRunState runState, CombatState? combatState, CombatRoom room)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCombatEnd(CombatRoom room)
```

***

#### AfterCombatVictory / AfterCombatVictoryEarly

战斗胜利时触发。

```csharp
public static async Task AfterCombatVictory(IRunState runState, CombatState? combatState, CombatRoom room)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCombatVictoryEarly(CombatRoom room)
public virtual Task AfterCombatVictory(CombatRoom room)
```

***

### 2. 攻击相关Hook

#### BeforeAttack

攻击执行前触发。

```csharp
public static async Task BeforeAttack(CombatState combatState, AttackCommand command)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeAttack(AttackCommand command)
```

**参数**:

- `command` - 攻击命令对象，包含攻击者、目标、伤害等信息

***

#### AfterAttack

攻击执行后触发。

```csharp
public static async Task AfterAttack(CombatState combatState, AttackCommand command)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterAttack(AttackCommand command)
```

***

### 3. 伤害相关Hook

#### BeforeDamageReceived

生物受到伤害前触发。

```csharp
public static async Task BeforeDamageReceived(
    PlayerChoiceContext choiceContext, 
    IRunState runState, 
    CombatState? combatState, 
    Creature target, 
    decimal amount, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeDamageReceived(
    PlayerChoiceContext choiceContext, 
    Creature target, 
    decimal amount, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)
```

***

#### AfterDamageReceived / AfterDamageReceivedLate

生物受到伤害后触发。

```csharp
public static async Task AfterDamageReceived(
    PlayerChoiceContext choiceContext, 
    IRunState runState, 
    CombatState? combatState, 
    Creature target, 
    DamageResult result, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterDamageReceived(
    PlayerChoiceContext choiceContext, 
    Creature target, 
    DamageResult result, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)

public virtual Task AfterDamageReceivedLate(
    PlayerChoiceContext choiceContext, 
    Creature target, 
    DamageResult result, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)
```

***

#### AfterDamageGiven

生物造成伤害后触发。

```csharp
public static async Task AfterDamageGiven(
    PlayerChoiceContext choiceContext, 
    CombatState combatState, 
    Creature? dealer, 
    DamageResult results, 
    ValueProp props, 
    Creature target, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterDamageGiven(
    PlayerChoiceContext choiceContext, 
    Creature? dealer, 
    DamageResult result, 
    ValueProp props, 
    Creature target, 
    CardModel? cardSource)
```

***

#### ModifyDamage

修改伤害值。

```csharp
public static decimal ModifyDamage(
    IRunState runState, 
    CombatState? combatState, 
    Creature? target, 
    Creature? dealer, 
    decimal damage, 
    ValueProp props, 
    CardModel? cardSource, 
    ModifyDamageHookType modifyDamageHookType, 
    CardPreviewMode previewMode, 
    out IEnumerable<AbstractModel> modifiers)
```

**AbstractModel响应方法**:

```csharp
public virtual decimal ModifyDamageAdditive(
    Creature? target, 
    decimal amount, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)

public virtual decimal ModifyDamageMultiplicative(
    Creature? target, 
    decimal amount, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)

public virtual decimal ModifyDamageCap(
    Creature? target, 
    ValueProp props, 
    Creature? dealer, 
    CardModel? cardSource)
```

**ModifyDamageHookType枚举**:

```csharp
[Flags]
public enum ModifyDamageHookType
{
    None = 0,
    Additive = 2,        // 加法修改
    Multiplicative = 4,  // 乘法修改
    All = 6
}
```

***

### 4. 护盾相关Hook

#### BeforeBlockGained

获得护盾前触发。

```csharp
public static async Task BeforeBlockGained(
    CombatState combatState, 
    Creature creature, 
    decimal amount, 
    ValueProp props, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeBlockGained(
    Creature creature, 
    decimal amount, 
    ValueProp props, 
    CardModel? cardSource)
```

***

#### AfterBlockGained

获得护盾后触发。

```csharp
public static async Task AfterBlockGained(
    CombatState combatState, 
    Creature creature, 
    decimal amount, 
    ValueProp props, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterBlockGained(
    Creature creature, 
    decimal amount, 
    ValueProp props, 
    CardModel? cardSource)
```

***

#### AfterBlockBroken

护盾被打破后触发。

```csharp
public static async Task AfterBlockBroken(CombatState combatState, Creature creature)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterBlockBroken(Creature creature)
```

***

#### AfterBlockCleared

护盾被清除后触发。

```csharp
public static async Task AfterBlockCleared(CombatState combatState, Creature creature)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterBlockCleared(Creature creature)
```

***

#### ModifyBlock

修改护盾值。

```csharp
public static decimal ModifyBlock(
    CombatState combatState, 
    Creature target, 
    decimal block, 
    ValueProp props, 
    CardModel? cardSource, 
    CardPlay? cardPlay, 
    out IEnumerable<AbstractModel> modifiers)
```

**AbstractModel响应方法**:

```csharp
public virtual decimal ModifyBlockAdditive(
    Creature target, 
    decimal block, 
    ValueProp props, 
    CardModel? cardSource, 
    CardPlay? cardPlay)

public virtual decimal ModifyBlockMultiplicative(
    Creature target, 
    decimal block, 
    ValueProp props, 
    CardModel? cardSource, 
    CardPlay? cardPlay)
```

***

### 5. 卡牌相关Hook

#### BeforeCardPlayed

卡牌打出前触发。

```csharp
public static async Task BeforeCardPlayed(CombatState combatState, CardPlay cardPlay)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeCardPlayed(CardPlay cardPlay)
```

***

#### AfterCardPlayed / AfterCardPlayedLate

卡牌打出后触发。

```csharp
public static async Task AfterCardPlayed(
    CombatState combatState, 
    PlayerChoiceContext choiceContext, 
    CardPlay cardPlay)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
public virtual Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
```

***

#### AfterCardDrawn / AfterCardDrawnEarly

卡牌抽到后触发。

```csharp
public static async Task AfterCardDrawn(
    CombatState combatState, 
    PlayerChoiceContext choiceContext, 
    CardModel card, 
    bool fromHandDraw)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCardDrawnEarly(
    PlayerChoiceContext choiceContext, 
    CardModel card, 
    bool fromHandDraw)

public virtual Task AfterCardDrawn(
    PlayerChoiceContext choiceContext, 
    CardModel card, 
    bool fromHandDraw)
```

***

#### AfterCardDiscarded

卡牌弃置后触发。

```csharp
public static async Task AfterCardDiscarded(
    CombatState combatState, 
    PlayerChoiceContext choiceContext, 
    CardModel card)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
```

***

#### AfterCardExhausted

卡牌消耗后触发。

```csharp
public static async Task AfterCardExhausted(
    CombatState combatState, 
    PlayerChoiceContext choiceContext, 
    CardModel card, 
    bool causedByEthereal)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCardExhausted(
    PlayerChoiceContext choiceContext, 
    CardModel card, 
    bool causedByEthereal)
```

***

#### AfterCardRetained

卡牌保留后触发。

```csharp
public static async Task AfterCardRetained(CombatState combatState, CardModel card)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCardRetained(CardModel card)
```

***

#### AfterCardChangedPiles / AfterCardChangedPilesLate

卡牌改变牌堆后触发。

```csharp
public static async Task AfterCardChangedPiles(
    IRunState runState, 
    CombatState? combatState, 
    CardModel card, 
    PileType oldPile, 
    AbstractModel? source)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterCardChangedPiles(
    CardModel card, 
    PileType oldPileType, 
    AbstractModel? source)

public virtual Task AfterCardChangedPilesLate(
    CardModel card, 
    PileType oldPileType, 
    AbstractModel? source)
```

***

#### ModifyCardPlayCount

修改卡牌打出次数。

```csharp
public static int ModifyCardPlayCount(
    CombatState combatState, 
    CardModel card, 
    int playCount, 
    Creature? target, 
    out List<AbstractModel> modifyingModels)
```

**AbstractModel响应方法**:

```csharp
public virtual int ModifyCardPlayCount(
    CardModel card, 
    Creature? target, 
    int playCount)
```

***

### 6. 回合相关Hook

#### BeforeSideTurnStart

回合开始前触发（按战斗方）。

```csharp
public static async Task BeforeSideTurnStart(CombatState combatState, CombatSide side)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeSideTurnStart(
    PlayerChoiceContext choiceContext, 
    CombatSide side, 
    CombatState combatState)
```

***

#### AfterSideTurnStart / AfterSideTurnStartLate

回合开始后触发（按战斗方）。

```csharp
public static async Task AfterSideTurnStart(CombatState combatState, CombatSide side)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterSideTurnStart(CombatSide side, CombatState combatState)
public virtual Task AfterSideTurnStartLate(CombatSide side, CombatState combatState)
```

***

#### AfterPlayerTurnStart / AfterPlayerTurnStartEarly / AfterPlayerTurnStartLate

玩家回合开始后触发。

```csharp
public static async Task AfterPlayerTurnStart(
    CombatState combatState, 
    PlayerChoiceContext choiceContext, 
    Player player)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
public virtual Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
public virtual Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
```

***

#### BeforeTurnEnd / BeforeTurnEndEarly / BeforeTurnEndVeryEarly

回合结束前触发。

```csharp
public static async Task BeforeTurnEnd(CombatState combatState, CombatSide side)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side)
public virtual Task BeforeTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side)
public virtual Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
```

***

#### AfterTurnEnd / AfterTurnEndLate

回合结束后触发。

```csharp
public static async Task AfterTurnEnd(CombatState combatState, CombatSide side)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
public virtual Task AfterTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side)
```

***

### 7. 能量相关Hook

#### AfterEnergyReset / AfterEnergyResetLate

能量重置后触发。

```csharp
public static async Task AfterEnergyReset(CombatState combatState, Player player)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterEnergyReset(Player player)
public virtual Task AfterEnergyResetLate(Player player)
```

***

#### AfterEnergySpent

能量消耗后触发。

```csharp
public static async Task AfterEnergySpent(CombatState combatState, CardModel card, int amount)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterEnergySpent(CardModel card, int amount)
```

***

#### ModifyEnergyGain

修改能量获取。

```csharp
public static decimal ModifyEnergyGain(
    CombatState combatState, 
    Player player, 
    decimal originalAmount, 
    out IEnumerable<AbstractModel> modifiers)
```

**AbstractModel响应方法**:

```csharp
public virtual decimal ModifyEnergyGain(Player player, decimal amount)
```

***

#### ModifyMaxEnergy

修改最大能量。

```csharp
public static decimal ModifyMaxEnergy(CombatState combatState, Player player, decimal amount)
```

**AbstractModel响应方法**:

```csharp
public virtual decimal ModifyMaxEnergy(Player player, decimal amount)
```

***

### 8. 能力(Power)相关Hook

#### BeforePowerAmountChanged

能力层数改变前触发。

```csharp
public static async Task BeforePowerAmountChanged(
    CombatState combatState, 
    PowerModel power, 
    decimal amount, 
    Creature target, 
    Creature? applier, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforePowerAmountChanged(
    PowerModel power, 
    decimal amount, 
    Creature target, 
    Creature? applier, 
    CardModel? cardSource)
```

***

#### AfterPowerAmountChanged

能力层数改变后触发。

```csharp
public static async Task AfterPowerAmountChanged(
    CombatState combatState, 
    PowerModel power, 
    decimal amount, 
    Creature? applier, 
    CardModel? cardSource)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterPowerAmountChanged(
    PowerModel power, 
    decimal amount, 
    Creature? applier, 
    CardModel? cardSource)
```

***

#### ModifyPowerAmountGiven

修改给予的能力层数。

```csharp
public static decimal ModifyPowerAmountGiven(
    CombatState combatState, 
    PowerModel power, 
    Creature giver, 
    decimal amount, 
    Creature? target, 
    CardModel? cardSource, 
    out IEnumerable<AbstractModel> modifiers)
```

**AbstractModel响应方法**:

```csharp
public virtual decimal ModifyPowerAmountGiven(
    PowerModel power, 
    Creature giver, 
    decimal amount, 
    Creature? target, 
    CardModel? cardSource)
```

***

#### ModifyPowerAmountReceived

修改接收的能力层数。

```csharp
public static decimal ModifyPowerAmountReceived(
    CombatState combatState, 
    PowerModel canonicalPower, 
    Creature target, 
    decimal amount, 
    Creature? giver, 
    out IEnumerable<AbstractModel> modifiers)
```

**AbstractModel响应方法**:

```csharp
public virtual decimal ModifyPowerAmountReceived(
    PowerModel power, 
    Creature target, 
    decimal amount, 
    Creature? giver)
```

***

### 9. 球体(Orb)相关Hook

#### AfterOrbChanneled

球体引导后触发。

```csharp
public static async Task AfterOrbChanneled(
    CombatState combatState, 
    PlayerChoiceContext choiceContext, 
    Player player, 
    OrbModel orb)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterOrbChanneled(
    PlayerChoiceContext choiceContext, 
    Player player, 
    OrbModel orb)
```

***

#### AfterOrbEvoked

球体激发后触发。

```csharp
public static async Task AfterOrbEvoked(
    PlayerChoiceContext choiceContext, 
    CombatState combatState, 
    OrbModel orb, 
    IEnumerable<Creature> targets)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterOrbEvoked(
    PlayerChoiceContext choiceContext, 
    OrbModel orb, 
    IEnumerable<Creature> targets)
```

***

#### ModifyOrbPassiveTriggerCount

修改球体被动触发次数。

```csharp
public static int ModifyOrbPassiveTriggerCount(
    CombatState combatState, 
    OrbModel orb, 
    int triggerCount, 
    out List<AbstractModel> modifyingModels)
```

**AbstractModel响应方法**:

```csharp
public virtual int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
```

***

### 10. 药水相关Hook

#### BeforePotionUsed

药水使用前触发。

```csharp
public static async Task BeforePotionUsed(
    IRunState runState, 
    CombatState? combatState, 
    PotionModel potion, 
    Creature? target)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforePotionUsed(PotionModel potion, Creature? target)
```

***

#### AfterPotionUsed

药水使用后触发。

```csharp
public static async Task AfterPotionUsed(
    IRunState runState, 
    CombatState? combatState, 
    PotionModel potion, 
    Creature? target)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterPotionUsed(PotionModel potion, Creature? target)
```

***

#### AfterPotionDiscarded

药水丢弃后触发。

```csharp
public static async Task AfterPotionDiscarded(
    IRunState runState, 
    CombatState? combatState, 
    PotionModel potion)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterPotionDiscarded(PotionModel potion)
```

***

#### AfterPotionProcured

药水获得后触发。

```csharp
public static async Task AfterPotionProcured(
    IRunState runState, 
    CombatState? combatState, 
    PotionModel potion)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterPotionProcured(PotionModel potion)
```

***

### 11. 死亡相关Hook

#### BeforeDeath

生物死亡前触发。

```csharp
public static async Task BeforeDeath(IRunState runState, CombatState? combatState, Creature creature)
```

**AbstractModel响应方法**:

```csharp
public virtual Task BeforeDeath(Creature creature)
```

***

#### AfterDeath

生物死亡后触发。

```csharp
public static async Task AfterDeath(
    IRunState runState, 
    CombatState? combatState, 
    Creature creature, 
    bool wasRemovalPrevented, 
    float deathAnimLength)
```

**AbstractModel响应方法**:

```csharp
public virtual Task AfterDeath(
    PlayerChoiceContext choiceContext, 
    Creature creature, 
    bool wasRemovalPrevented, 
    float deathAnimLength)
```

***

### 12. 条件判断Hook (Should系列)

这些Hook返回布尔值，用于判断是否允许某些行为。

#### ShouldAllowHitting

是否允许攻击某生物。

```csharp
public static bool ShouldAllowHitting(CombatState combatState, Creature creature)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldAllowHitting(Creature creature)
```

***

#### ShouldAllowTargeting

是否允许选择某生物作为目标。

```csharp
public static bool ShouldAllowTargeting(
    CombatState combatState, 
    Creature target, 
    out AbstractModel? preventer)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldAllowTargeting(Creature target)
```

***

#### ShouldClearBlock

是否清除护盾。

```csharp
public static bool ShouldClearBlock(
    CombatState combatState, 
    Creature creature, 
    out AbstractModel? preventer)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldClearBlock(Creature creature)
```

***

#### ShouldDie

是否死亡。

```csharp
public static bool ShouldDie(
    IRunState runState, 
    CombatState? combatState, 
    Creature creature, 
    out AbstractModel? preventer)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldDie(Creature creature)
```

***

#### ShouldDraw

是否抽牌。

```csharp
public static bool ShouldDraw(
    CombatState combatState, 
    Player player, 
    bool fromHandDraw, 
    out AbstractModel? modifier)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldDraw(Player player, bool fromHandDraw)
```

***

#### ShouldPlay

是否可以打出卡牌。

```csharp
public static bool ShouldPlay(
    CombatState combatState, 
    CardModel card, 
    out AbstractModel? preventer, 
    AutoPlayType autoPlayType)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
```

***

#### ShouldStopCombatFromEnding

是否阻止战斗结束。

```csharp
public static bool ShouldStopCombatFromEnding(CombatState combatState)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldStopCombatFromEnding()
```

***

#### ShouldTakeExtraTurn

是否获得额外回合。

```csharp
public static bool ShouldTakeExtraTurn(CombatState combatState, Player player)
```

**AbstractModel响应方法**:

```csharp
public virtual bool ShouldTakeExtraTurn(Player player)
```

***

## 完整Hook方法列表

### 事件类Hook (返回Task)

| Hook方法                      | 说明        |
| --------------------------- | --------- |
| AfterActEntered             | 进入新Act后   |
| BeforeAttack                | 攻击前       |
| AfterAttack                 | 攻击后       |
| AfterBlockBroken            | 护盾打破后     |
| AfterBlockCleared           | 护盾清除后     |
| BeforeBlockGained           | 获得护盾前     |
| AfterBlockGained            | 获得护盾后     |
| BeforeCardAutoPlayed        | 卡牌自动打出前   |
| AfterCardChangedPiles       | 卡牌改变牌堆后   |
| AfterCardDiscarded          | 卡牌弃置后     |
| AfterCardDrawn              | 卡牌抽到后     |
| AfterCardEnteredCombat      | 卡牌进入战斗后   |
| AfterCardExhausted          | 卡牌消耗后     |
| AfterCardGeneratedForCombat | 卡牌在战斗中生成后 |
| BeforeCardPlayed            | 卡牌打出前     |
| AfterCardPlayed             | 卡牌打出后     |
| BeforeCardRemoved           | 卡牌移除前     |
| AfterCardRetained           | 卡牌保留后     |
| BeforeCombatStart           | 战斗开始前     |
| AfterCombatEnd              | 战斗结束后     |
| AfterCombatVictory          | 战斗胜利后     |
| AfterCreatureAddedToCombat  | 生物加入战斗后   |
| AfterCurrentHpChanged       | 当前HP改变后   |
| AfterDamageGiven            | 造成伤害后     |
| BeforeDamageReceived        | 受到伤害前     |
| AfterDamageReceived         | 受到伤害后     |
| BeforeDeath                 | 死亡前       |
| AfterDeath                  | 死亡后       |
| AfterGoldGained             | 获得金币后     |
| AfterDiedToDoom             | 因末日死亡后    |
| AfterEnergyReset            | 能量重置后     |
| AfterEnergySpent            | 能量消耗后     |
| BeforeFlush                 | 清空手牌前     |
| AfterForge                  | 锻造后       |
| BeforeHandDraw              | 抽牌前       |
| AfterHandEmptied            | 手牌清空后     |
| AfterItemPurchased          | 购买物品后     |
| AfterMapGenerated           | 地图生成后     |
| AfterOrbChanneled           | 球体引导后     |
| AfterOrbEvoked              | 球体激发后     |
| AfterOstyRevived            | Osty复活后   |
| AfterPlayerTurnStart        | 玩家回合开始后   |
| BeforePlayPhaseStart        | 出牌阶段开始前   |
| AfterPotionDiscarded        | 药水丢弃后     |
| AfterPotionProcured         | 药水获得后     |
| BeforePotionUsed            | 药水使用前     |
| AfterPotionUsed             | 药水使用后     |
| BeforePowerAmountChanged    | 能力层数改变前   |
| AfterPowerAmountChanged     | 能力层数改变后   |
| AfterPreventingBlockClear   | 阻止护盾清除后   |
| AfterPreventingDeath        | 阻止死亡后     |
| AfterPreventingDraw         | 阻止抽牌后     |
| AfterRestSiteHeal           | 休息点治疗后    |
| AfterRestSiteSmith          | 休息点锻造后    |
| BeforeRewardsOffered        | 奖励展示前     |
| AfterRewardTaken            | 奖励获取后     |
| BeforeRoomEntered           | 进入房间前     |
| AfterRoomEntered            | 进入房间后     |
| AfterShuffle                | 洗牌后       |
| BeforeSideTurnStart         | 回合开始前     |
| AfterSideTurnStart          | 回合开始后     |
| AfterStarsGained            | 获得星星后     |
| AfterStarsSpent             | 消耗星星后     |
| AfterSummon                 | 召唤后       |
| AfterTakingExtraTurn        | 获得额外回合后   |
| BeforeTurnEnd               | 回合结束前     |
| AfterTurnEnd                | 回合结束后     |

### 修改类Hook (返回修改后的值)

| Hook方法                       | 返回类型    | 说明            |
| ---------------------------- | ------- | ------------- |
| ModifyAttackHitCount         | decimal | 修改攻击次数        |
| ModifyBlock                  | decimal | 修改护盾值         |
| ModifyCardPlayCount          | int     | 修改卡牌打出次数      |
| ModifyDamage                 | decimal | 修改伤害值         |
| ModifyEnergyCostInCombat     | decimal | 修改战斗中能量消耗     |
| ModifyEnergyGain             | decimal | 修改能量获取        |
| ModifyHandDraw               | decimal | 修改抽牌数         |
| ModifyHpLostBeforeOsty       | decimal | 修改HP损失(Osty前) |
| ModifyHpLostAfterOsty        | decimal | 修改HP损失(Osty后) |
| ModifyMaxEnergy              | decimal | 修改最大能量        |
| ModifyMerchantPrice          | decimal | 修改商人价格        |
| ModifyOrbPassiveTriggerCount | int     | 修改球体被动触发次数    |
| ModifyOrbValue               | decimal | 修改球体值         |
| ModifyPowerAmountGiven       | decimal | 修改给予的能力层数     |
| ModifyPowerAmountReceived    | decimal | 修改接收的能力层数     |
| ModifyRestSiteHealAmount     | decimal | 修改休息点治疗量      |
| ModifyStarCost               | decimal | 修改星星消耗        |
| ModifySummonAmount           | decimal | 修改召唤数量        |
| ModifyXValue                 | int     | 修改X值          |

### 条件类Hook (返回bool)

| Hook方法                                      | 说明            |
| ------------------------------------------- | ------------- |
| ShouldAddToDeck                             | 是否添加到牌组       |
| ShouldAfflict                               | 是否施加负面效果      |
| ShouldAllowAncient                          | 是否允许远古事件      |
| ShouldAllowHitting                          | 是否允许攻击        |
| ShouldAllowMerchantCardRemoval              | 是否允许商人移除卡牌    |
| ShouldAllowSelectingMoreCardRewards         | 是否允许选择更多卡牌奖励  |
| ShouldAllowTargeting                        | 是否允许选择目标      |
| ShouldClearBlock                            | 是否清除护盾        |
| ShouldCreatureBeRemovedFromCombatAfterDeath | 死亡后是否从战斗中移除   |
| ShouldDie                                   | 是否死亡          |
| ShouldDisableRemainingRestSiteOptions       | 是否禁用剩余休息点选项   |
| ShouldDraw                                  | 是否抽牌          |
| ShouldEtherealTrigger                       | 是否触发虚无        |
| ShouldFlush                                 | 是否清空手牌        |
| ShouldGainGold                              | 是否获得金币        |
| ShouldGenerateTreasure                      | 是否生成宝藏        |
| ShouldGainStars                             | 是否获得星星        |
| ShouldPayExcessEnergyCostWithStars          | 是否用星星支付多余能量消耗 |
| ShouldPlay                                  | 是否可以打出卡牌      |
| ShouldPlayerResetEnergy                     | 是否重置能量        |
| ShouldProceedToNextMapPoint                 | 是否前往下一个地图点    |
| ShouldProcurePotion                         | 是否获得药水        |
| ShouldRefillMerchantEntry                   | 是否补充商人条目      |
| ShouldStopCombatFromEnding                  | 是否阻止战斗结束      |
| ShouldTakeExtraTurn                         | 是否获得额外回合      |
| ShouldForcePotionReward                     | 是否强制药水奖励      |
| ShouldAllowFreeTravel                       | 是否允许免费旅行      |
| ShouldPowerBeRemovedOnDeath                 | 死亡时是否移除能力     |

***

## 使用示例

### 创建自定义遗物响应Hook

```csharp
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

public class MyRelic : RelicModel
{
    public override bool ShouldReceiveCombatHooks => true;
    
    public override Task AfterAttack(AttackCommand command)
    {
        if (command.Attacker.IsPlayer)
        {
            int totalDamage = command.Results.Sum(r => r.UnblockedDamage);
            Console.WriteLine($"玩家造成了 {totalDamage} 点伤害");
        }
        return Task.CompletedTask;
    }
    
    public override decimal ModifyDamageAdditive(
        Creature? target, 
        decimal amount, 
        ValueProp props, 
        Creature? dealer, 
        CardModel? cardSource)
    {
        if (dealer != null && dealer.IsPlayer)
        {
            return 2m;
        }
        return 0m;
    }
}
```

### 创建自定义能力响应Hook

```csharp
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;

public class MyPower : PowerModel
{
    public override Task AfterBlockGained(
        Creature creature, 
        decimal amount, 
        ValueProp props, 
        CardModel? cardSource)
    {
        if (creature == Owner)
        {
            Console.WriteLine($"获得了 {amount} 点护盾");
        }
        return Task.CompletedTask;
    }
    
    public override decimal ModifyBlockAdditive(
        Creature target, 
        decimal block, 
        ValueProp props, 
        CardModel? cardSource, 
        CardPlay? cardPlay)
    {
        if (target == Owner)
        {
            return 1m;
        }
        return 0m;
    }
}
```

### 在Mod中订阅Hook事件

```csharp
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Combat;

public class MyMod : Mod
{
    public override void OnLoad()
    {
        CombatManager.Instance.CombatSetUp += OnCombatSetUp;
    }
    
    private void OnCombatSetUp(CombatState state)
    {
        // 战斗设置完成，可以在这里初始化数据收集
    }
}
```

***

## 注意事项

1. **异步方法**: 大多数Hook方法返回`Task`，可以使用`async/await`进行异步操作。
2. **执行顺序**: Hook按照模型在`IterateHookListeners()`中的顺序执行。带有`Late`后缀的方法在普通方法之后执行。
3. **修改器追踪**: 修改类Hook通常会输出`modifiers`参数，记录哪些模型参与了修改。
4. **性能考虑**: Hook方法会被频繁调用，应避免在其中进行耗时操作。
5. **线程安全**: Hook可能在不同的上下文中调用，注意线程安全。

***

*文档生成日期: 2026-04-18*
*源代码版本: Slay the Spire 2*
