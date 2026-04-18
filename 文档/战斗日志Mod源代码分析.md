# 杀戮尖塔2 战斗日志Mod - 源代码分析文档

## 目录

1. [战斗系统概述](#战斗系统概述)
2. [核心类和文件](#核心类和文件)
3. [战斗历史系统](#战斗历史系统)
4. [伤害系统](#伤害系统)
5. [护盾系统](#护盾系统)
6. [回合系统](#回合系统)
7. [数据统计接口](#数据统计接口)

---

## 战斗系统概述

杀戮尖塔2的战斗系统采用事件驱动架构，核心由以下组件构成：

- **CombatManager**: 战斗管理器，单例模式，管理整个战斗流程
- **CombatState**: 战斗状态，存储当前战斗的所有数据
- **CombatHistory**: 战斗历史记录系统，记录所有战斗事件
- **Creature**: 生物实体（玩家和敌人的基类）
- **Player**: 玩家实体
- **AttackCommand**: 攻击命令，处理伤害计算和执行

---

## 核心类和文件

### 1. CombatManager (战斗管理器)

**文件路径**: `src/Core/Combat/CombatManager.cs`

**命名空间**: `MegaCrit.Sts2.Core.Combat`

**关键属性**:

```csharp
public class CombatManager
{
    public static CombatManager Instance { get; }
    public CombatHistory History { get; }
    public CombatStateTracker StateTracker { get; }
    public bool IsInProgress { get; private set; }
    public bool IsPlayPhase { get; private set; }
    public bool IsEnemyTurnStarted { get; private set; }
    public int RoundNumber { get; set; }
}
```

**关键事件**:

| 事件名 | 参数类型 | 说明 |
|--------|----------|------|
| CombatSetUp | CombatState | 战斗设置完成 |
| CombatEnded | CombatRoom | 战斗结束 |
| CombatWon | CombatRoom | 战斗胜利 |
| TurnStarted | CombatState | 回合开始 |
| TurnEnded | CombatState | 回合结束 |
| PlayerEndedTurn | Player, bool | 玩家结束回合 |
| CreaturesChanged | CombatState | 生物列表变化 |

### 2. CombatState (战斗状态)

**文件路径**: `src/Core/Combat/CombatState.cs`

**命名空间**: `MegaCrit.Sts2.Core.Combat`

**关键属性**:

```csharp
public class CombatState
{
    public IRunState RunState { get; }
    public IReadOnlyList<Creature> Allies { get; }
    public IReadOnlyList<Creature> Enemies { get; }
    public IReadOnlyList<Creature> Creatures { get; }
    public IReadOnlyList<Player> Players { get; }
    public int RoundNumber { get; set; }
    public CombatSide CurrentSide { get; set; }
    public EncounterModel? Encounter { get; }
    public List<Creature> EscapedCreatures { get; private set; }
}
```

### 3. Creature (生物实体)

**文件路径**: `src/Core/Entities/Creatures/Creature.cs`

**命名空间**: `MegaCrit.Sts2.Core.Entities.Creatures`

**关键属性**:

```csharp
public class Creature
{
    public int Block { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public MonsterModel? Monster { get; }
    public Player? Player { get; }
    public CombatSide Side { get; }
    public CombatState? CombatState { get; set; }
    public IReadOnlyList<PowerModel> Powers { get; }
    public bool IsAlive { get; }
    public bool IsDead { get; }
    public bool IsEnemy { get; }
    public bool IsPlayer { get; }
}
```

**关键事件**:

| 事件名 | 参数类型 | 说明 |
|--------|----------|------|
| BlockChanged | int, int | 护盾变化（旧值，新值） |
| CurrentHpChanged | int, int | 当前HP变化 |
| MaxHpChanged | int, int | 最大HP变化 |
| Died | Creature | 生物死亡 |
| Revived | Creature | 生物复活 |
| PowerApplied | PowerModel | 能力应用 |
| PowerRemoved | PowerModel | 能力移除 |

### 4. Player (玩家实体)

**文件路径**: `src/Core/Entities/Players/Player.cs`

**命名空间**: `MegaCrit.Sts2.Core.Entities.Players`

**关键属性**:

```csharp
public class Player
{
    public CharacterModel Character { get; }
    public Creature Creature { get; }
    public ulong NetId { get; }
    public PlayerCombatState? PlayerCombatState { get; private set; }
    public IReadOnlyList<RelicModel> Relics { get; }
    public IReadOnlyList<PotionModel?> PotionSlots { get; }
    public CardPile Deck { get; }
    public int MaxEnergy { get; set; }
    public int Gold { get; set; }
}
```

### 5. PlayerCombatState (玩家战斗状态)

**文件路径**: `src/Core/Entities/Players/PlayerCombatState.cs`

**命名空间**: `MegaCrit.Sts2.Core.Entities.Players`

**关键属性**:

```csharp
public class PlayerCombatState
{
    public CardPile Hand { get; }
    public CardPile DrawPile { get; }
    public CardPile DiscardPile { get; }
    public CardPile ExhaustPile { get; }
    public CardPile PlayPile { get; }
    public int Energy { get; set; }
    public int MaxEnergy { get; }
    public int Stars { get; set; }
    public OrbQueue OrbQueue { get; }
    public IReadOnlyList<Creature> Pets { get; }
}
```

---

## 战斗历史系统

### CombatHistory (战斗历史)

**文件路径**: `src/Core/Combat/History/CombatHistory.cs`

**命名空间**: `MegaCrit.Sts2.Core.Combat.History`

这是战斗日志Mod最核心的系统，记录所有战斗事件。

**关键属性**:

```csharp
public class CombatHistory
{
    public IEnumerable<CombatHistoryEntry> Entries { get; }
    public IEnumerable<CardPlayStartedEntry> CardPlaysStarted { get; }
    public IEnumerable<CardPlayFinishedEntry> CardPlaysFinished { get; }
    
    public event Action? Changed;
}
```

**记录方法**:

| 方法名 | 参数 | 说明 |
|--------|------|------|
| CardPlayStarted | CombatState, CardPlay | 卡牌打出开始 |
| CardPlayFinished | CombatState, CardPlay | 卡牌打出结束 |
| CardAfflicted | CombatState, CardModel, AfflictionModel | 卡牌获得负面效果 |
| CardDiscarded | CombatState, CardModel | 卡牌弃置 |
| CardDrawn | CombatState, CardModel, bool | 卡牌抽牌 |
| CardExhausted | CombatState, CardModel | 卡牌消耗 |
| CardGenerated | CombatState, CardModel, bool | 卡牌生成 |
| CreatureAttacked | CombatState, Creature, IReadOnlyList<DamageResult> | 生物攻击 |
| DamageReceived | CombatState, Creature, Creature?, DamageResult, CardModel? | 伤害接收 |
| BlockGained | CombatState, Creature, int, ValueProp, CardPlay? | 护盾获得 |
| EnergySpent | CombatState, int, Player | 能量消耗 |
| MonsterPerformedMove | CombatState, MonsterModel, MoveState, IEnumerable<Creature>? | 怪物行动 |
| OrbChanneled | CombatState, OrbModel | 球体引导 |
| PotionUsed | CombatState, PotionModel, Creature? | 药水使用 |
| PowerReceived | CombatState, PowerModel, decimal, Creature? | 能力获得 |
| StarsModified | CombatState, int, Player | 星星修改 |
| Summoned | CombatState, int, Player | 召唤 |

### CombatHistoryEntry (历史条目基类)

**文件路径**: `src/Core/Combat/History/CombatHistoryEntry.cs`

**命名空间**: `MegaCrit.Sts2.Core.Combat.History`

**关键属性**:

```csharp
public abstract class CombatHistoryEntry
{
    public Creature Actor { get; }
    public int RoundNumber { get; }
    public CombatSide CurrentSide { get; }
    public CombatHistory History { get; }
    public string HumanReadableString { get; }
    public abstract string Description { get; }
    
    public bool HappenedThisTurn(CombatState? state);
}
```

### 历史条目类型详解

#### 1. DamageReceivedEntry (伤害接收条目)

**文件路径**: `src/Core/Combat/History/Entries/DamageReceivedEntry.cs`

```csharp
public class DamageReceivedEntry : CombatHistoryEntry
{
    public DamageResult Result { get; }
    public Creature? Dealer { get; }
    public CardModel? CardSource { get; }
    public Creature Receiver { get; }
}
```

#### 2. BlockGainedEntry (护盾获得条目)

**文件路径**: `src/Core/Combat/History/Entries/BlockGainedEntry.cs`

```csharp
public class BlockGainedEntry : CombatHistoryEntry
{
    public int Amount { get; }
    public Creature Receiver { get; }
    public ValueProp Props { get; }
    public CardPlay? CardPlay { get; }
}
```

#### 3. CreatureAttackedEntry (生物攻击条目)

**文件路径**: `src/Core/Combat/History/Entries/CreatureAttackedEntry.cs`

```csharp
public class CreatureAttackedEntry : CombatHistoryEntry
{
    public IReadOnlyList<DamageResult> DamageResults { get; }
}
```

#### 4. CardPlayStartedEntry (卡牌打出开始条目)

**文件路径**: `src/Core/Combat/History/Entries/CardPlayStartedEntry.cs`

```csharp
public class CardPlayStartedEntry : CombatHistoryEntry
{
    public CardPlay CardPlay { get; }
}
```

#### 5. CardPlayFinishedEntry (卡牌打出结束条目)

**文件路径**: `src/Core/Combat/History/Entries/CardPlayFinishedEntry.cs`

```csharp
public class CardPlayFinishedEntry : CombatHistoryEntry
{
    public CardPlay CardPlay { get; }
    public bool WasEthereal { get; }
}
```

#### 6. EnergySpentEntry (能量消耗条目)

**文件路径**: `src/Core/Combat/History/Entries/EnergySpentEntry.cs`

```csharp
public class EnergySpentEntry : CombatHistoryEntry
{
    public int Amount { get; }
}
```

#### 7. CardDrawnEntry (卡牌抽牌条目)

**文件路径**: `src/Core/Combat/History/Entries/CardDrawnEntry.cs`

```csharp
public class CardDrawnEntry : CombatHistoryEntry
{
    public CardModel Card { get; }
    public bool FromHandDraw { get; }
}
```

#### 8. CardDiscardedEntry (卡牌弃置条目)

**文件路径**: `src/Core/Combat/History/Entries/CardDiscardedEntry.cs`

```csharp
public class CardDiscardedEntry : CombatHistoryEntry
{
    public CardModel Card { get; }
}
```

#### 9. CardExhaustedEntry (卡牌消耗条目)

**文件路径**: `src/Core/Combat/History/Entries/CardExhaustedEntry.cs`

```csharp
public class CardExhaustedEntry : CombatHistoryEntry
{
    public CardModel Card { get; }
}
```

#### 10. CardGeneratedEntry (卡牌生成条目)

**文件路径**: `src/Core/Combat/History/Entries/CardGeneratedEntry.cs`

```csharp
public class CardGeneratedEntry : CombatHistoryEntry
{
    public CardModel Card { get; }
    public bool GeneratedByPlayer { get; }
}
```

#### 11. PowerReceivedEntry (能力获得条目)

**文件路径**: `src/Core/Combat/History/Entries/PowerReceivedEntry.cs`

```csharp
public class PowerReceivedEntry : CombatHistoryEntry
{
    public PowerModel Power { get; }
    public decimal Amount { get; }
    public Creature? Applier { get; }
}
```

#### 12. MonsterPerformedMoveEntry (怪物行动条目)

**文件路径**: `src/Core/Combat/History/Entries/MonsterPerformedMoveEntry.cs`

```csharp
public class MonsterPerformedMoveEntry : CombatHistoryEntry
{
    public MonsterModel Monster { get; }
    public MoveState Move { get; }
    public IEnumerable<Creature>? Targets { get; }
}
```

#### 13. PotionUsedEntry (药水使用条目)

**文件路径**: `src/Core/Combat/History/Entries/PotionUsedEntry.cs`

```csharp
public class PotionUsedEntry : CombatHistoryEntry
{
    public PotionModel Potion { get; }
    public Creature? Target { get; }
}
```

#### 14. OrbChanneledEntry (球体引导条目)

**文件路径**: `src/Core/Combat/History/Entries/OrbChanneledEntry.cs`

```csharp
public class OrbChanneledEntry : CombatHistoryEntry
{
    public OrbModel Orb { get; }
}
```

#### 15. StarsModifiedEntry (星星修改条目)

**文件路径**: `src/Core/Combat/History/Entries/StarsModifiedEntry.cs`

```csharp
public class StarsModifiedEntry : CombatHistoryEntry
{
    public int Amount { get; }
}
```

---

## 伤害系统

### DamageResult (伤害结果)

**文件路径**: `src/Core/Entities/Creatures/DamageResult.cs`

**命名空间**: `MegaCrit.Sts2.Core.Entities.Creatures`

```csharp
public class DamageResult
{
    public Creature Receiver { get; }
    public ValueProp Props { get; }
    public int BlockedDamage { get; set; }
    public int UnblockedDamage { get; init; }
    public int OverkillDamage { get; init; }
    public int TotalDamage { get; }
    public bool WasBlockBroken { get; set; }
    public bool WasFullyBlocked { get; set; }
    public bool WasTargetKilled { get; init; }
}
```

### AttackCommand (攻击命令)

**文件路径**: `src/Core/Commands/Builders/AttackCommand.cs`

**命名空间**: `MegaCrit.Sts2.Core.Commands.Builders`

**关键属性**:

```csharp
public class AttackCommand
{
    public Creature? Attacker { get; private set; }
    public AbstractModel? ModelSource { get; private set; }
    public CombatSide TargetSide { get; private set; }
    public ValueProp DamageProps { get; private set; }
    public bool IsSingleTargeted { get; }
    public bool IsMultiTargeted { get; }
    public bool IsRandomlyTargeted { get; }
    public IEnumerable<DamageResult> Results { get; }
    public string? HitSfx { get; private set; }
    public string? HitVfx { get; private set; }
}
```

**关键方法**:

| 方法名 | 返回类型 | 说明 |
|--------|----------|------|
| FromCard | AttackCommand | 从卡牌创建攻击 |
| FromMonster | AttackCommand | 从怪物创建攻击 |
| Targeting | AttackCommand | 设置单体目标 |
| TargetingAllOpponents | AttackCommand | 设置所有对手为目标 |
| TargetingRandomOpponents | AttackCommand | 设置随机对手为目标 |
| WithHitCount | AttackCommand | 设置攻击次数 |
| Execute | Task<AttackCommand> | 执行攻击 |

### DamageCmd (伤害命令)

**文件路径**: `src/Core/Commands/DamageCmd.cs`

**命名空间**: `MegaCrit.Sts2.Core.Commands`

```csharp
public static class DamageCmd
{
    public static AttackCommand Attack(decimal damagePerHit);
    public static AttackCommand Attack(CalculatedDamageVar calculatedDamageVar);
}
```

### ValueProp (伤害属性)

**文件路径**: `src/Core/ValueProps/ValueProp.cs`

**命名空间**: `MegaCrit.Sts2.Core.ValueProps`

```csharp
[Flags]
public enum ValueProp
{
    Unblockable = 2,    // 不可阻挡
    Unpowered = 4,      // 无力量加成
    Move = 8,           // 移动
    SkipHurtAnim = 0x10 // 跳过受伤动画
}
```

### DamageProps (伤害属性常量)

**文件路径**: `src/Core/ValueProps/DamageProps.cs`

```csharp
public static class DamageProps
{
    public const ValueProp card = ValueProp.Move;
    public const ValueProp cardUnpowered = ValueProp.Unpowered | ValueProp.Move;
    public const ValueProp cardHpLoss = ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move;
    public const ValueProp monsterMove = ValueProp.Move;
    public const ValueProp nonCardUnpowered = ValueProp.Unpowered;
    public const ValueProp nonCardHpLoss = ValueProp.Unblockable | ValueProp.Unpowered;
}
```

---

## 护盾系统

### BlockProps (护盾属性常量)

**文件路径**: `src/Core/ValueProps/BlockProps.cs`

```csharp
public static class BlockProps
{
    public const ValueProp card = ValueProp.Move;
    public const ValueProp cardUnpowered = ValueProp.Unpowered | ValueProp.Move;
    public const ValueProp monsterMove = ValueProp.Move;
    public const ValueProp nonCardUnpowered = ValueProp.Unpowered;
}
```

### Creature 护盾相关方法

**文件路径**: `src/Core/Entities/Creatures/Creature.cs`

```csharp
public class Creature
{
    public int Block { get; private set; }
    public event Action<int, int>? BlockChanged;
    
    public void GainBlockInternal(decimal amount);
    public void LoseBlockInternal(decimal amount);
    public decimal DamageBlockInternal(decimal amount, ValueProp props);
}
```

---

## 回合系统

### CombatSide (战斗方)

**文件路径**: `src/Core/Combat/CombatSide.cs`

**命名空间**: `MegaCrit.Sts2.Core.Combat`

```csharp
public enum CombatSide
{
    Player,
    Enemy
}
```

### 回合相关属性

**CombatState**:

```csharp
public int RoundNumber { get; set; }
public CombatSide CurrentSide { get; set; }
```

**CombatManager**:

```csharp
public bool IsPlayPhase { get; private set; }
public bool IsEnemyTurnStarted { get; private set; }
public bool EndingPlayerTurnPhaseOne { get; private set; }
public bool EndingPlayerTurnPhaseTwo { get; private set; }
```

### 回合相关事件

| 事件名 | 说明 |
|--------|------|
| TurnStarted | 回合开始 |
| TurnEnded | 回合结束 |
| PlayerEndedTurn | 玩家结束回合 |
| PlayerUnendedTurn | 玩家取消结束回合 |
| AboutToSwitchToEnemyTurn | 即将切换到敌人回合 |

---

## 数据统计接口

### 获取战斗历史

```csharp
CombatHistory history = CombatManager.Instance.History;
IEnumerable<CombatHistoryEntry> entries = history.Entries;
```

### 统计玩家伤害

```csharp
var damageEntries = CombatManager.Instance.History.Entries
    .OfType<DamageReceivedEntry>()
    .Where(e => e.Dealer != null && e.Dealer.IsPlayer);

int totalDamage = damageEntries.Sum(e => e.Result.TotalDamage);
int totalUnblockedDamage = damageEntries.Sum(e => e.Result.UnblockedDamage);
```

### 统计玩家护盾

```csharp
var blockEntries = CombatManager.Instance.History.Entries
    .OfType<BlockGainedEntry>()
    .Where(e => e.Receiver.IsPlayer);

int totalBlock = blockEntries.Sum(e => e.Amount);
```

### 统计卡牌使用

```csharp
var cardPlays = CombatManager.Instance.History.CardPlaysFinished;
int totalCardsPlayed = cardPlays.Count();

var cardsByType = cardPlays
    .GroupBy(e => e.CardPlay.Card.Id.Entry)
    .Select(g => new { Card = g.Key, Count = g.Count() });
```

### 统计能量消耗

```csharp
var energySpent = CombatManager.Instance.History.Entries
    .OfType<EnergySpentEntry>();

int totalEnergySpent = energySpent.Sum(e => e.Amount);
```

### 获取当前回合数

```csharp
int roundNumber = CombatManager.Instance.StateTracker.State?.RoundNumber ?? 0;
```

### 获取玩家当前状态

```csharp
Player player = CombatManager.Instance.StateTracker.State?.Players.First();
int currentHp = player.Creature.CurrentHp;
int maxHp = player.Creature.MaxHp;
int currentBlock = player.Creature.Block;
int currentEnergy = player.PlayerCombatState?.Energy ?? 0;
```

---

## 文件路径汇总

### 战斗核心

| 文件 | 路径 |
|------|------|
| CombatManager.cs | src/Core/Combat/CombatManager.cs |
| CombatState.cs | src/Core/Combat/CombatState.cs |
| CombatSide.cs | src/Core/Combat/CombatSide.cs |
| CombatStateTracker.cs | src/Core/Combat/CombatStateTracker.cs |

### 战斗历史

| 文件 | 路径 |
|------|------|
| CombatHistory.cs | src/Core/Combat/History/CombatHistory.cs |
| CombatHistoryEntry.cs | src/Core/Combat/History/CombatHistoryEntry.cs |
| DamageReceivedEntry.cs | src/Core/Combat/History/Entries/DamageReceivedEntry.cs |
| BlockGainedEntry.cs | src/Core/Combat/History/Entries/BlockGainedEntry.cs |
| CreatureAttackedEntry.cs | src/Core/Combat/History/Entries/CreatureAttackedEntry.cs |
| CardPlayStartedEntry.cs | src/Core/Combat/History/Entries/CardPlayStartedEntry.cs |
| CardPlayFinishedEntry.cs | src/Core/Combat/History/Entries/CardPlayFinishedEntry.cs |
| CardDrawnEntry.cs | src/Core/Combat/History/Entries/CardDrawnEntry.cs |
| CardDiscardedEntry.cs | src/Core/Combat/History/Entries/CardDiscardedEntry.cs |
| CardExhaustedEntry.cs | src/Core/Combat/History/Entries/CardExhaustedEntry.cs |
| CardGeneratedEntry.cs | src/Core/Combat/History/Entries/CardGeneratedEntry.cs |
| EnergySpentEntry.cs | src/Core/Combat/History/Entries/EnergySpentEntry.cs |
| PowerReceivedEntry.cs | src/Core/Combat/History/Entries/PowerReceivedEntry.cs |
| MonsterPerformedMoveEntry.cs | src/Core/Combat/History/Entries/MonsterPerformedMoveEntry.cs |
| PotionUsedEntry.cs | src/Core/Combat/History/Entries/PotionUsedEntry.cs |
| OrbChanneledEntry.cs | src/Core/Combat/History/Entries/OrbChanneledEntry.cs |
| StarsModifiedEntry.cs | src/Core/Combat/History/Entries/StarsModifiedEntry.cs |

### 命令系统

| 文件 | 路径 |
|------|------|
| DamageCmd.cs | src/Core/Commands/DamageCmd.cs |
| AttackCommand.cs | src/Core/Commands/Builders/AttackCommand.cs |
| AttackContext.cs | src/Core/Commands/Builders/AttackContext.cs |

### 实体

| 文件 | 路径 |
|------|------|
| Creature.cs | src/Core/Entities/Creatures/Creature.cs |
| DamageResult.cs | src/Core/Entities/Creatures/DamageResult.cs |
| Player.cs | src/Core/Entities/Players/Player.cs |
| PlayerCombatState.cs | src/Core/Entities/Players/PlayerCombatState.cs |

### 数值属性

| 文件 | 路径 |
|------|------|
| ValueProp.cs | src/Core/ValueProps/ValueProp.cs |
| DamageProps.cs | src/Core/ValueProps/DamageProps.cs |
| BlockProps.cs | src/Core/ValueProps/BlockProps.cs |

---

## Mod开发建议

### 1. 订阅战斗事件

```csharp
public class AttackLogMod : Mod
{
    public override void OnLoad()
    {
        CombatManager.Instance.CombatSetUp += OnCombatSetUp;
        CombatManager.Instance.TurnStarted += OnTurnStarted;
        CombatManager.Instance.CombatEnded += OnCombatEnded;
    }
    
    private void OnCombatSetUp(CombatState state)
    {
        CombatManager.Instance.History.Changed += OnHistoryChanged;
    }
    
    private void OnHistoryChanged()
    {
        var entries = CombatManager.Instance.History.Entries;
        // 处理历史记录变化
    }
}
```

### 2. 创建统计数据类

```csharp
public class CombatStatistics
{
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public int TotalBlockGained { get; set; }
    public int CardsPlayed { get; set; }
    public int EnergySpent { get; set; }
    public int RoundsPlayed { get; set; }
}
```

### 3. 使用Hook系统

游戏使用Hook系统进行事件拦截，可以通过Hook来获取更详细的数据。

**文件路径**: `src/Core/Hooks/Hook.cs`

---

*文档生成日期: 2026-04-18*
*源代码版本: Slay the Spire 2*
