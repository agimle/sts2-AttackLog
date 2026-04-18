# 战斗历史系统 API 参考

## 概述

战斗历史系统（CombatHistory）是杀戮尖塔2中记录所有战斗事件的核心系统。本文档详细介绍如何使用该系统获取战斗数据。

---

## CombatHistory 类

### 命名空间
`MegaCrit.Sts2.Core.Combat.History`

### 文件路径
`src/Core/Combat/History/CombatHistory.cs`

### 获取实例

```csharp
CombatHistory history = CombatManager.Instance.History;
```

---

## 属性

### Entries

获取所有历史条目。

```csharp
public IEnumerable<CombatHistoryEntry> Entries { get; }
```

**示例**:
```csharp
foreach (var entry in CombatManager.Instance.History.Entries)
{
    Console.WriteLine(entry.HumanReadableString);
}
```

### CardPlaysStarted

获取所有卡牌打出开始的条目。

```csharp
public IEnumerable<CardPlayStartedEntry> CardPlaysStarted { get; }
```

### CardPlaysFinished

获取所有卡牌打出结束的条目。

```csharp
public IEnumerable<CardPlayFinishedEntry> CardPlaysFinished { get; }
```

---

## 事件

### Changed

当历史记录发生变化时触发。

```csharp
public event Action? Changed;
```

**订阅示例**:
```csharp
CombatManager.Instance.History.Changed += OnHistoryChanged;

private void OnHistoryChanged()
{
    // 处理历史变化
}
```

---

## 方法

### Clear

清除所有历史记录。

```csharp
public void Clear()
```

---

## 历史条目类型

### 伤害相关

#### DamageReceivedEntry

当生物受到伤害时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Result | DamageResult | 伤害结果详情 |
| Dealer | Creature? | 伤害来源（可能为null） |
| CardSource | CardModel? | 卡牌来源（如果有的话） |
| Receiver | Creature | 伤害接收者 |
| RoundNumber | int | 回合数 |
| CurrentSide | CombatSide | 当前战斗方 |

**DamageResult 属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| TotalDamage | int | 总伤害 |
| UnblockedDamage | int | 未被阻挡的伤害 |
| BlockedDamage | int | 被阻挡的伤害 |
| OverkillDamage | int | 过量伤害 |
| WasBlockBroken | bool | 是否打破护盾 |
| WasFullyBlocked | bool | 是否完全被阻挡 |
| WasTargetKilled | bool | 是否击杀目标 |

**使用示例**:
```csharp
var damageEntries = history.Entries.OfType<DamageReceivedEntry>();

foreach (var entry in damageEntries)
{
    Console.WriteLine($"回合{entry.RoundNumber}: {entry.Dealer?.Name} -> {entry.Receiver.Name}");
    Console.WriteLine($"  总伤害: {entry.Result.TotalDamage}");
    Console.WriteLine($"  实际伤害: {entry.Result.UnblockedDamage}");
    Console.WriteLine($"  被阻挡: {entry.Result.BlockedDamage}");
}
```

#### CreatureAttackedEntry

当生物发起攻击时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| DamageResults | IReadOnlyList<DamageResult> | 所有伤害结果 |

**使用示例**:
```csharp
var attackEntries = history.Entries.OfType<CreatureAttackedEntry>();

foreach (var entry in attackEntries)
{
    int totalDamage = entry.DamageResults.Sum(r => r.TotalDamage);
    Console.WriteLine($"{entry.Actor.Name} 攻击，造成 {totalDamage} 总伤害");
}
```

---

### 护盾相关

#### BlockGainedEntry

当生物获得护盾时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Amount | int | 获得的护盾量 |
| Receiver | Creature | 护盾接收者 |
| Props | ValueProp | 属性标记 |
| CardPlay | CardPlay? | 关联的卡牌打出（如果有） |

**使用示例**:
```csharp
var blockEntries = history.Entries.OfType<BlockGainedEntry>();

foreach (var entry in blockEntries)
{
    Console.WriteLine($"回合{entry.RoundNumber}: {entry.Receiver.Name} 获得 {entry.Amount} 护盾");
}
```

---

### 卡牌相关

#### CardPlayStartedEntry

当卡牌开始打出时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| CardPlay | CardPlay | 卡牌打出信息 |

**CardPlay 属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Card | CardModel | 打出的卡牌 |
| Target | Creature? | 目标生物 |

#### CardPlayFinishedEntry

当卡牌打出完成时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| CardPlay | CardPlay | 卡牌打出信息 |
| WasEthereal | bool | 是否为虚无卡牌 |

**使用示例**:
```csharp
var cardPlays = history.CardPlaysFinished;

var cardCounts = cardPlays
    .GroupBy(e => e.CardPlay.Card.Id.Entry)
    .Select(g => new { CardName = g.Key, Count = g.Count() });

foreach (var item in cardCounts)
{
    Console.WriteLine($"{item.CardName}: 打出 {item.Count} 次");
}
```

#### CardDrawnEntry

当卡牌被抽到时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Card | CardModel | 抽到的卡牌 |
| FromHandDraw | bool | 是否为回合开始抽牌 |

#### CardDiscardedEntry

当卡牌被弃置时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Card | CardModel | 弃置的卡牌 |

#### CardExhaustedEntry

当卡牌被消耗时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Card | CardModel | 消耗的卡牌 |

#### CardGeneratedEntry

当卡牌在战斗中生成时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Card | CardModel | 生成的卡牌 |
| GeneratedByPlayer | bool | 是否由玩家生成 |

---

### 资源相关

#### EnergySpentEntry

当能量被消耗时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Amount | int | 消耗的能量 |

**使用示例**:
```csharp
var energyEntries = history.Entries.OfType<EnergySpentEntry>();
int totalEnergy = energyEntries.Sum(e => e.Amount);
Console.WriteLine($"总共消耗能量: {totalEnergy}");
```

#### StarsModifiedEntry

当星星数量变化时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Amount | int | 变化的星星数量（正数为获得，负数为失去） |

---

### 能力相关

#### PowerReceivedEntry

当生物获得能力时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Power | PowerModel | 获得的能力 |
| Amount | decimal | 能力层数 |
| Applier | Creature? | 能力施加者 |

**使用示例**:
```csharp
var powerEntries = history.Entries.OfType<PowerReceivedEntry>();

foreach (var entry in powerEntries)
{
    Console.WriteLine($"{entry.Actor.Name} 获得 {entry.Amount} 层 {entry.Power.Id.Entry}");
}
```

---

### 怪物相关

#### MonsterPerformedMoveEntry

当怪物执行行动时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Monster | MonsterModel | 执行行动的怪物 |
| Move | MoveState | 行动状态 |
| Targets | IEnumerable<Creature>? | 目标列表 |

---

### 其他

#### PotionUsedEntry

当药水被使用时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Potion | PotionModel | 使用的药水 |
| Target | Creature? | 目标生物 |

#### OrbChanneledEntry

当球体被引导时记录。

**属性**:
| 名称 | 类型 | 说明 |
|------|------|------|
| Orb | OrbModel | 引导的球体 |

---

## 实用统计方法

### 统计玩家造成的总伤害

```csharp
public static int GetTotalPlayerDamage(CombatHistory history)
{
    return history.Entries
        .OfType<DamageReceivedEntry>()
        .Where(e => e.Dealer != null && e.Dealer.IsPlayer)
        .Sum(e => e.Result.UnblockedDamage);
}
```

### 统计玩家受到的总伤害

```csharp
public static int GetTotalDamageTaken(CombatHistory history)
{
    return history.Entries
        .OfType<DamageReceivedEntry>()
        .Where(e => e.Receiver.IsPlayer)
        .Sum(e => e.Result.UnblockedDamage);
}
```

### 统计玩家获得的总护盾

```csharp
public static int GetTotalBlockGained(CombatHistory history)
{
    return history.Entries
        .OfType<BlockGainedEntry>()
        .Where(e => e.Receiver.IsPlayer)
        .Sum(e => e.Amount);
}
```

### 统计每种卡牌的打出次数

```csharp
public static Dictionary<string, int> GetCardPlayCounts(CombatHistory history)
{
    return history.CardPlaysFinished
        .GroupBy(e => e.CardPlay.Card.Id.Entry)
        .ToDictionary(g => g.Key, g => g.Count());
}
```

### 获取每回合的伤害统计

```csharp
public static Dictionary<int, int> GetDamageByRound(CombatHistory history)
{
    return history.Entries
        .OfType<DamageReceivedEntry>()
        .Where(e => e.Dealer != null && e.Dealer.IsPlayer)
        .GroupBy(e => e.RoundNumber)
        .ToDictionary(g => g.Key, g => g.Sum(e => e.Result.UnblockedDamage));
}
```

### 获取击杀统计

```csharp
public static int GetKillCount(CombatHistory history)
{
    return history.Entries
        .OfType<DamageReceivedEntry>()
        .Count(e => e.Result.WasTargetKilled && e.Dealer != null && e.Dealer.IsPlayer);
}
```

---

## 完整示例：战斗统计收集器

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;

public class CombatStatsCollector
{
    public int TotalDamageDealt { get; private set; }
    public int TotalDamageTaken { get; private set; }
    public int TotalBlockGained { get; private set; }
    public int CardsPlayed { get; private set; }
    public int EnergySpent { get; private set; }
    public int EnemiesKilled { get; private set; }
    public int RoundsPlayed { get; private set; }
    
    public void Subscribe()
    {
        CombatManager.Instance.History.Changed += OnHistoryChanged;
        CombatManager.Instance.TurnStarted += OnTurnStarted;
    }
    
    public void Unsubscribe()
    {
        CombatManager.Instance.History.Changed -= OnHistoryChanged;
        CombatManager.Instance.TurnStarted -= OnTurnStarted;
    }
    
    private void OnHistoryChanged()
    {
        var history = CombatManager.Instance.History;
        
        TotalDamageDealt = history.Entries
            .OfType<DamageReceivedEntry>()
            .Where(e => e.Dealer != null && e.Dealer.IsPlayer)
            .Sum(e => e.Result.UnblockedDamage);
            
        TotalDamageTaken = history.Entries
            .OfType<DamageReceivedEntry>()
            .Where(e => e.Receiver.IsPlayer)
            .Sum(e => e.Result.UnblockedDamage);
            
        TotalBlockGained = history.Entries
            .OfType<BlockGainedEntry>()
            .Where(e => e.Receiver.IsPlayer)
            .Sum(e => e.Amount);
            
        CardsPlayed = history.CardPlaysFinished.Count();
        
        EnergySpent = history.Entries
            .OfType<EnergySpentEntry>()
            .Sum(e => e.Amount);
            
        EnemiesKilled = history.Entries
            .OfType<DamageReceivedEntry>()
            .Count(e => e.Result.WasTargetKilled && 
                        e.Dealer != null && 
                        e.Dealer.IsPlayer && 
                        e.Receiver.IsEnemy);
    }
    
    private void OnTurnStarted(CombatState state)
    {
        RoundsPlayed = state.RoundNumber;
    }
    
    public void Reset()
    {
        TotalDamageDealt = 0;
        TotalDamageTaken = 0;
        TotalBlockGained = 0;
        CardsPlayed = 0;
        EnergySpent = 0;
        EnemiesKilled = 0;
        RoundsPlayed = 0;
    }
}
```

---

*文档生成日期: 2026-04-18*
