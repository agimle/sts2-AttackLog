# PoisonPower 生命周期文档

## 概述

PoisonPower（中毒）是 Slay the Spire 2 中的一种减益型 Power，属于 Debuff 类型。其核心机制是在持有者的回合开始时造成不可阻挡的伤害，并逐层递减。

## 基本信息

| 属性 | 值 |
|------|-----|
| **类型** | `PowerType.Debuff` |
| **叠加方式** | `PowerStackType.Counter`（数值叠加） |
| **来源文件** | `Core/Models/Powers/PoisonPower.cs` |

## 生命周期流程

```
┌─────────────────────────────────────────────────────────────────┐
│                    PoisonPower 生命周期                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. 施加阶段 (Apply)                                             │
│     ├── 卡片使用                                                 │
│     ├── 药水使用                                                 │
│     ├── 遗物触发                                                 │
│     └── 其他 Power 效果                                          │
│              │                                                  │
│              ▼                                                  │
│  2. Hook 修改阶段                                                │
│     ├── ModifyPowerAmountGiven (施加者修改)                      │
│     └── ModifyPowerAmountReceived (接收者修改)                   │
│              │                                                  │
│              ▼                                                  │
│  3. 回合开始触发 (AfterSideTurnStart)                            │
│     ├── 检查是否为持有者回合                                      │
│     ├── 计算触发次数 (TriggerCount)                              │
│     ├── 造成伤害 (Unblockable | Unpowered)                       │
│     └── 递减层数 (Decrement)                                     │
│              │                                                  │
│              ▼                                                  │
│  4. 过期/移除                                                    │
│     └── 层数降为 0 时自动移除                                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## 核心代码分析

### 1. 施加阶段

PoisonPower 通过 `PowerCmd.Apply<PoisonPower>()` 方法施加：

```csharp
// PowerCmd.cs - Apply 方法核心流程
public static async Task Apply(PowerModel power, Creature target, decimal amount, ...)
{
    // 1. 检查战斗是否结束
    if (CombatManager.Instance.IsEnding) return;
    
    // 2. 检查目标是否可接收 Power
    if (!target.CanReceivePowers) return;
    
    // 3. 触发 Hook 修改层数
    modifiedAmount = Hook.ModifyPowerAmountGiven(...);    // 施加者修改
    modifiedAmount = Hook.ModifyPowerAmountReceived(...); // 接收者修改
    
    // 4. 应用 Power
    power.ApplyInternal(target, modifiedAmount, silent);
    
    // 5. 记录历史
    CombatManager.Instance.History.PowerReceived(combatState, power, modifiedAmount, applier);
    
    // 6. 如果是玩家方获得 Debuff，跳过下一次持续时间衰减
    if (target.Side == CombatSide.Player && power.Type == PowerType.Debuff)
    {
        power.SkipNextDurationTick = true;
    }
}
```

### 2. 回合开始触发

```csharp
// PoisonPower.cs - AfterSideTurnStart
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
{
    // 只在持有者的回合触发
    if (side != base.Owner.Side) return;
    
    int iterations = TriggerCount;  // 计算触发次数
    
    for (int i = 0; i < iterations; i++)
    {
        // 造成不可阻挡、不可被强化的伤害
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(), 
            base.Owner, 
            base.Amount,  // 当前层数作为伤害值
            ValueProp.Unblockable | ValueProp.Unpowered,  // 不可格挡、不可强化
            null, 
            null
        );
        
        // 如果目标存活，递减层数
        if (base.Owner.IsAlive)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            await Cmd.CustomScaledWait(0.1f, 0.25f);
        }
    }
}
```

### 3. 触发次数计算

```csharp
// PoisonPower.cs - TriggerCount 属性
private int TriggerCount
{
    get
    {
        // 获取所有存活的敌方单位
        IEnumerable<Creature> source = from c in base.Owner.CombatState.GetOpponentsOf(base.Owner)
            where c.IsAlive
            select c;
        
        // 触发次数 = min(层数, 1 + 敌方AccelerantPower层数总和)
        return Math.Min(base.Amount, 1 + source.Sum(a => a.GetPowerAmount<AccelerantPower>()));
    }
}
```

### 4. 层数递减

```csharp
// PowerCmd.cs - Decrement 方法
public static async Task Decrement(PowerModel power)
{
    await ModifyAmount(power, -1m, null, null);
}

// ModifyAmount 方法会检查是否需要移除
public static async Task<int> ModifyAmount(PowerModel power, decimal offset, ...)
{
    int newAmount = power.Amount + (int)modifiedOffset;
    power.SetAmount(newAmount, silent);
    
    // 检查是否需要移除
    if (power.ShouldRemoveDueToAmount())
    {
        await Remove(power);
    }
    
    return newAmount;
}
```

## 伤害计算

### 下回合预期伤害

```csharp
// PoisonPower.cs - CalculateTotalDamageNextTurn
public int CalculateTotalDamageNextTurn()
{
    decimal num = default(decimal);
    int num2 = Math.Min(base.Amount, TriggerCount);
    
    for (int i = 0; i < num2; i++)
    {
        decimal damage = base.Amount - i;  // 每次触发后层数减少
        damage = Hook.ModifyDamage(...);   // 经过 Hook 修改
        num += damage;
    }
    return (int)num;
}
```

### 伤害特性

- **不可格挡** (`ValueProp.Unblockable`)：无视护盾
- **不可强化** (`ValueProp.Unpowered`)：不受力量等加成影响
- **递减伤害**：每次触发后层数减 1，伤害递减

## 施加来源

### 卡片

| 卡片 | 能量 | 稀有度 | 目标 | 基础层数 | 升级后 |
|------|------|--------|------|----------|--------|
| [DeadlyPoison](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/DeadlyPoison.cs) | 1 | Common | 单体敌人 | 5 | 7 |
| [PoisonedStab](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/PoisonedStab.cs) | 1 | Common | 单体敌人 | 3 | 4 |
| [Snakebite](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/Snakebite.cs) | 2 | Common | 单体敌人 | 7 | 10 |
| [BouncingFlask](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/BouncingFlask.cs) | 2 | Uncommon | 随机敌人×3 | 3×3 | 3×4 |

### 药水

| 药水 | 稀有度 | 目标 | 层数 |
|------|--------|------|------|
| [PoisonPotion](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Potions/PoisonPotion.cs) | Common | 单体敌人 | 6 |

### 遗物

| 遗物 | 稀有度 | 触发条件 | 效果 |
|------|--------|----------|------|
| [TwistedFunnel](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Relics/TwistedFunnel.cs) | Uncommon | 战斗第一回合开始 | 所有敌人获得 4 层 Poison |
| [SneckoSkull](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Relics/SneckoSkull.cs) | Common | 施加 Poison 时 | 额外 +1 层 |

### 其他 Power 效果

| Power | 触发条件 | 效果 |
|-------|----------|------|
| [EnvenomPower](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/EnvenomPower.cs) | 造成攻击伤害后 | 给目标施加 Poison |
| [NoxiousFumesPower](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/NoxiousFumesPower.cs) | 回合开始 | 所有敌人获得 Poison |
| [CorrosiveWavePower](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/CorrosiveWavePower.cs) | 抽牌时 | 所有敌人获得 Poison |

## 交互机制

### 与 AccelerantPower 的交互

[AccelerantPower](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/AccelerantPower.cs) 可以增加 Poison 的触发次数：

```
触发次数 = min(Poison层数, 1 + 敌方AccelerantPower层数总和)
```

这意味着如果敌人有 AccelerantPower，Poison 会在同一回合内触发多次。

### 与 OutbreakPower 的交互

[OutbreakPower](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/OutbreakPower.cs) 会在施加 Poison 达到阈值时触发：

```csharp
public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, ...)
{
    if (power is PoisonPower && applier == base.Owner && amount > 0)
    {
        data.timesPoisoned++;
        if (data.timesPoisoned >= 3)  // 每 3 次施加 Poison
        {
            // 对所有敌人造成伤害
            await CreatureCmd.Damage(..., base.Amount, ValueProp.Unpowered, ...);
            data.timesPoisoned %= 3;
        }
    }
}
```

### 层数修改 Hook

施加 Poison 时会经过以下 Hook：

1. **ModifyPowerAmountGiven**: 施加者修改层数
   - [SneckoSkull](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Relics/SneckoSkull.cs): +1 层

2. **ModifyPowerAmountReceived**: 接收者修改层数

## 过期与移除

### 自动移除条件

```csharp
// PowerModel.cs - ShouldRemoveDueToAmount
public bool ShouldRemoveDueToAmount()
{
    if (AllowNegative || Amount > 0)
    {
        if (AllowNegative)
        {
            return Amount == 0;  // 允许负数时，0 层移除
        }
        return false;
    }
    return true;  // 不允许负数且层数 <= 0 时移除
}
```

PoisonPower 不允许负数，所以当层数降为 0 或以下时会自动移除。

### 移除流程

```csharp
// PowerCmd.cs - Remove
public static async Task Remove(PowerModel? power)
{
    if (power != null)
    {
        power.RemoveInternal();           // 从 Creature 移除
        await Cmd.CustomScaledWait(0.2f, 0.4f);
        await power.AfterRemoved(power.Owner);  // 触发移除后回调
    }
}
```

## 时序图

```
时间线
  │
  ├─ 玩家打出 DeadlyPoison
  │    │
  │    ├─ PowerCmd.Apply<PoisonPower>(target, 5, player, card)
  │    │    │
  │    │    ├─ Hook.ModifyPowerAmountGiven (SneckoSkull: +1)
  │    │    │    → amount = 6
  │    │    │
  │    │    ├─ Hook.ModifyPowerAmountReceived
  │    │    │    → amount = 6
  │    │    │
  │    │    └─ power.ApplyInternal(target, 6)
  │    │
  │    └─ 敌人获得 6 层 Poison
  │
  ├─ 敌人回合开始
  │    │
  │    ├─ Hook.AfterSideTurnStart(Enemy)
  │    │    │
  │    │    └─ PoisonPower.AfterSideTurnStart(Enemy, combatState)
  │    │         │
  │    │         ├─ TriggerCount = min(6, 1) = 1
  │    │         │
  │    │         ├─ CreatureCmd.Damage(enemy, 6, Unblockable|Unpowered)
  │    │         │    → 敌人受到 6 点伤害（无视护盾）
  │    │         │
  │    │         └─ PowerCmd.Decrement(this)
  │    │              → Poison 层数: 6 → 5
  │    │
  │    └─ 敌人回合继续...
  │
  ├─ 敌人回合结束
  │
  ├─ 玩家回合开始
  │    │
  │    └─ (Poison 不触发，因为不是持有者回合)
  │
  ├─ 敌人回合开始
  │    │
  │    ├─ PoisonPower.AfterSideTurnStart(Enemy, combatState)
  │    │    │
  │    │    ├─ CreatureCmd.Damage(enemy, 5, Unblockable|Unpowered)
  │    │    │    → 敌人受到 5 点伤害
  │    │    │
  │    │    └─ PowerCmd.Decrement(this)
  │    │         → Poison 层数: 5 → 4
  │    │
  │    └─ ...
  │
  └─ 持续直到 Poison 层数降为 0
       → 自动移除
```

## CombatHistory 记录系统

### 相关 Entry 类型

CombatHistory 会记录 PoisonPower 相关的以下事件：

| Entry 类型 | 记录时机 | 关键字段 |
|------------|----------|----------|
| `PowerReceivedEntry` | Power 施加时 | `Power`, `Amount`, `Applier` |
| `DamageReceivedEntry` | 伤害造成时 | `Result`, `Dealer`, `CardSource` |

### 记录流程

```
┌─────────────────────────────────────────────────────────────────┐
│                    CombatHistory 记录时机                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. 施加 Poison                                                 │
│     PowerCmd.Apply() → PowerReceivedEntry 记录                  │
│     ├── Power: PoisonPower 实例                                 │
│     ├── Amount: 施加层数                                        │
│     └── Applier: 施加者 ✓                                       │
│                                                                 │
│  2. Poison 触发伤害                                              │
│     CreatureCmd.Damage() → DamageReceivedEntry 记录             │
│     ├── Result: 伤害结果                                        │
│     ├── Dealer: null ✗ (无法知道是 Poison 触发)                 │
│     └── CardSource: null ✗                                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 代码示例

```csharp
// 获取所有 Poison 施加记录
var poisonApplies = CombatManager.Instance.History.Entries
    .OfType<PowerReceivedEntry>()
    .Where(e => e.Power is PoisonPower);

foreach (var entry in poisonApplies)
{
    Console.WriteLine($"回合 {entry.RoundNumber}: " +
        $"{entry.Applier?.ModelId.Entry ?? "未知"} " +
        $"对 {entry.Actor.ModelId.Entry} " +
        $"施加 {entry.Amount} 层 Poison");
}

// 获取所有伤害记录（包括 Poison 造成的）
var damages = CombatManager.Instance.History.Entries
    .OfType<DamageReceivedEntry>()
    .Where(e => e.Dealer == null);  // Poison 伤害的 dealer 是 null
```

### 局限性分析

| 问题 | 说明 |
|------|------|
| **伤害来源无法识别** | Poison 造成伤害时 `Dealer = null`，无法知道是哪个 Power 触发的 |
| **施加者只能记录一人** | `Power.Applier` 只记录第一个施加者，后续叠加不会更新 |
| **多人施加无法区分** | 多人施加的 Poison 会合并层数，无法区分各自贡献 |

### 多人施加追踪方案

```
场景：玩家A 施加 5 层，玩家B 施加 3 层

CombatHistory 记录：
  ├─ PowerReceivedEntry { Applier: 玩家A, Amount: 5 }  ← 第一次
  └─ PowerReceivedEntry { Applier: 玩家B, Amount: 3 }  ← 第二次

Power.Applier 值：
  └─ 玩家A (只记录第一个)

Poison 触发时：
  └─ DamageReceivedEntry { Dealer: null, Amount: 8 }  ← 无法知道是谁造成的
```

**Mod 开发建议**：需要自己维护施加者追踪字典

```csharp
// 建议的数据结构
Dictionary<Creature, List<PoisonApplierRecord>> poisonTracker;

class PoisonApplierRecord
{
    Creature Applier;    // 施加者
    int Amount;          // 施加层数
    int RoundNumber;     // 施加回合
}
```

### 通过 CombatHistory 辅助追踪

```csharp
// 方案：监听 PowerReceived 事件，自己维护追踪
void OnPowerReceived(PowerReceivedEntry entry)
{
    if (entry.Power is PoisonPower)
    {
        var tracker = GetOrCreateTracker(entry.Actor);
        tracker.Add(new PoisonApplierRecord
        {
            Applier = entry.Applier,
            Amount = (int)entry.Amount,
            RoundNumber = entry.RoundNumber
        });
    }
}

// 当 Poison 造成伤害时，按比例或顺序分配
void OnDamageReceived(DamageReceivedEntry entry)
{
    if (entry.Dealer == null && IsPoisonDamage(entry))
    {
        var tracker = GetTracker(entry.Receiver);
        // 按比例分配伤害归属...
    }
}
```

## 关键代码位置

| 功能 | 文件路径 |
|------|----------|
| PoisonPower 定义 | [Core/Models/Powers/PoisonPower.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/PoisonPower.cs) |
| Power 基类 | [Core/Models/PowerModel.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/PowerModel.cs) |
| Power 命令 | [Core/Commands/PowerCmd.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Commands/PowerCmd.cs) |
| Hook 系统 | [Core/Hooks/Hook.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Hooks/Hook.cs) |
| 伤害命令 | [Core/Commands/CreatureCmd.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Commands/CreatureCmd.cs) |
| 战斗管理器 | [Core/Combat/CombatManager.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/CombatManager.cs) |
| 战斗历史 | [Core/Combat/History/CombatHistory.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/History/CombatHistory.cs) |
| Power 记录 Entry | [Core/Combat/History/Entries/PowerReceivedEntry.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/History/Entries/PowerReceivedEntry.cs) |
| 伤害记录 Entry | [Core/Combat/History/Entries/DamageReceivedEntry.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/History/Entries/DamageReceivedEntry.cs) |
