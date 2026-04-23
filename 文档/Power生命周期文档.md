# Power 生命周期文档

## 概述

本文档详细描述了杀戮尖塔2（Slay the Spire 2）中 Power（能力/状态效果）的完整生命周期，包括施加、修改、移除、以及与怪物复活相关的特殊处理。

---

## Power 生命周期总览

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Power 生命周期                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐                  │
│  │  Power 施加   │───▶│  Power 存在   │───▶│  Power 移除   │                  │
│  │ (Apply)      │    │  (Active)    │    │  (Remove)    │                  │
│  └──────────────┘    └──────────────┘    └──────────────┘                  │
│         │                   │                   │                          │
│         │                   │                   │                          │
│         ▼                   ▼                   ▼                          │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐                  │
│  │PowerReceived │    │ 层数变化      │    │PowerRemoved  │                  │
│  │   Entry      │    │ 事件触发      │    │   事件       │                  │
│  └──────────────┘    └──────────────┘    └──────────────┘                  │
│                                                                             │
│  特殊场景：                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐                  │
│  │  死亡清空     │    │  复活保留     │    │  逃跑清空     │                  │
│  └──────────────┘    └──────────────┘    └──────────────┘                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 一、Power 施加阶段

### 1.1 施加入口

Power 通过 `PowerCmd.Apply<T>()` 方法施加：

```csharp
// PowerCmd.cs
public static async Task Apply<T>(Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel
{
    await Apply(ModelDb.Power<T>(), target, amount, applier, cardSource, silent);
}

public static async Task Apply(PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
{
    // 1. 检查战斗是否结束
    if (CombatManager.Instance.IsEnding) return;
    
    // 2. 检查目标是否可接收 Power
    if (!target.CanReceivePowers) return;
    
    // 3. 触发 Hook 修改层数
    amount = Hook.ModifyPowerAmountGiven(combatState, power, target, amount, applier, cardSource);
    amount = Hook.ModifyPowerAmountReceived(combatState, power, target, amount, applier, cardSource);
    
    // 4. 应用 Power
    power.ApplyInternal(target, amount, silent);
    
    // 5. 记录历史
    CombatManager.Instance.History.PowerReceived(combatState, power, amount, applier);
    
    // 6. 触发 AfterApplied
    await power.AfterApplied(applier, cardSource);
}
```

### 1.2 施加流程图

```
PowerCmd.Apply<T>(target, amount, applier, cardSource)
│
├── 1. 检查战斗状态
│   └── if (CombatManager.Instance.IsEnding) return
│
├── 2. 检查目标是否可接收 Power
│   └── if (!target.CanReceivePowers) return
│       └── CanReceivePowers 检查:
│           ├── CombatState != null
│           └── Hook.ShouldAllowHitting() == true
│
├── 3. 触发 Hook 修改层数
│   ├── Hook.ModifyPowerAmountGiven()    // 施加者修改
│   └── Hook.ModifyPowerAmountReceived() // 接收者修改
│
├── 4. 应用 Power
│   └── PowerModel.ApplyInternal(target, amount, silent)
│       ├── 设置 Owner = target
│       ├── 设置 Amount = amount
│       └── Creature.ApplyPowerInternal(power)
│           ├── 检查是否已存在同类 Power
│           ├── _powers.Add(power)
│           └── ★ 触发 PowerApplied 事件
│
├── 5. 记录历史
│   └── CombatHistory.PowerReceived(combatState, power, amount, applier)
│       └── 创建 PowerReceivedEntry
│
└── 6. 触发 AfterApplied
    └── PowerModel.AfterApplied(applier, cardSource)
```

### 1.3 关键事件

| 事件 | 触发时机 | 参数 |
|------|----------|------|
| `Creature.PowerApplied` | Power 添加到列表后 | `PowerModel power` |
| `Creature.PowerIncreased` | 层数增加时 | `PowerModel power, int change, bool silent` |
| `CombatHistory.PowerReceived` | 记录到历史 | `PowerModel power, decimal amount, Creature? applier` |

---

## 二、Power 层数修改

### 2.1 修改方式

```csharp
// PowerCmd.cs
public static async Task<int> ModifyAmount(PowerModel power, decimal offset, Creature? applier, CardModel? cardSource)
{
    int newAmount = power.Amount + (int)offset;
    power.SetAmount(newAmount, silent);
    
    // 检查是否需要移除
    if (power.ShouldRemoveDueToAmount())
    {
        await Remove(power);
    }
    
    return newAmount;
}

public static async Task Decrement(PowerModel power)
{
    await ModifyAmount(power, -1m, null, null);
}
```

### 2.2 层数变化事件

```csharp
// Creature.cs
public void InvokePowerModified(PowerModel power, int change, bool silent)
{
    if (change > 0)
    {
        this.PowerIncreased?.Invoke(power, change, silent);
    }
    else if (power.StackType.Equals(PowerStackType.Counter) && power.AllowNegative && change < 0)
    {
        this.PowerIncreased?.Invoke(power, change, silent);  // 负数层数也算增加
    }
    else
    {
        this.PowerDecreased?.Invoke(power, silent);
    }
}
```

### 2.3 自动移除条件

```csharp
// PowerModel.cs
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

---

## 三、Power 移除阶段

### 3.1 移除方式

**方式一：主动移除**
```csharp
// PowerCmd.cs
public static async Task Remove(PowerModel? power)
{
    if (power != null)
    {
        power.RemoveInternal();           // 从 Creature 移除
        await Cmd.CustomScaledWait(0.2f, 0.4f);
        await power.AfterRemoved(power.Owner);  // 触发移除后回调
    }
}

public static async Task Remove<T>(Creature creature) where T : PowerModel
{
    await Remove(creature.GetPower<T>());
}
```

**方式二：批量移除**
```csharp
// Creature.cs
public IEnumerable<PowerModel> RemoveAllPowersInternalExcept(IEnumerable<PowerModel>? except = null)
{
    List<PowerModel> list = _powers.Except(except ?? Array.Empty<PowerModel>()).ToList();
    foreach (PowerModel item in list)
    {
        item.RemoveInternal();
    }
    return list;
}
```

### 3.2 移除流程图

```
PowerModel.RemoveInternal()
│
├── 1. 触发 Removed 事件
│   └── this.Removed?.Invoke()
│
├── 2. 从 Creature 移除
│   └── Owner.RemovePowerInternal(this)
│       ├── _powers.Remove(power)
│       └── ★ 触发 PowerRemoved 事件
│
└── 3. 异步回调
    └── PowerModel.AfterRemoved(oldOwner)
```

### 3.3 关键事件

| 事件 | 触发时机 | 参数 |
|------|----------|------|
| `PowerModel.Removed` | 移除开始时 | 无 |
| `Creature.PowerRemoved` | 从列表移除后 | `PowerModel power` |

---

## 四、Power 清空场景

### 4.1 场景总览

| 场景 | 方法 | 触发时机 | 特殊处理 |
|------|------|----------|----------|
| **死亡** | `RemoveAllPowersAfterDeath()` | 生物死亡时 | 检查 `ShouldPowerBeRemovedAfterOwnerDeath()` |
| **复活** | 部分保留 | 死亡但复活时 | 只移除 Debuff 类型 |
| **逃跑** | `RemoveAllPowersInternalExcept()` | 怪物逃跑时 | 全部移除 |
| **战斗结束** | `Reset()` | 战斗结束后 | 全部移除 |

### 4.2 死亡时 Power 清空

```csharp
// Creature.cs
public IEnumerable<PowerModel> RemoveAllPowersAfterDeath()
{
    return RemoveAllPowersInternalExcept(
        _powers.Where(p => 
            !p.ShouldPowerBeRemovedAfterOwnerDeath() || 
            !Hook.ShouldPowerBeRemovedOnDeath(p)
        )
    );
}
```

**关键检查点**：

```csharp
// PowerModel.cs
public virtual bool ShouldPowerBeRemovedAfterOwnerDeath()
{
    return true;  // 默认移除
}

// Hook.cs
public static bool ShouldPowerBeRemovedOnDeath(PowerModel power)
{
    foreach (AbstractModel item in power.Owner.CombatState.IterateHookListeners())
    {
        if (!item.ShouldPowerBeRemovedOnDeath(power))
        {
            return false;  // 有模型阻止移除
        }
    }
    return true;
}
```

### 4.3 死亡流程中的 Power 处理

```
CreatureCmd.KillWithoutCheckingWinCondition(creature, force, recursion)
│
├── 1. Hook.BeforeDeath()
│
├── 2. 检查是否应该死亡
│   └── force || MaxHp <= 0 || Hook.ShouldDie()
│
├── 3. 如果应该死亡
│   ├── InvokeDiedEvent()
│   ├── 检查是否从战斗移除
│   │   └── Hook.ShouldCreatureBeRemovedFromCombatAfterDeath()
│   ├── 播放死亡动画
│   ├── ★ Hook.AfterDeath(wasRemovalPrevented: false)
│   ├── 从战斗移除（如果需要）
│   └── ★ 移除 Power
│       └── RemoveAllPowersAfterDeath()
│           ├── 检查 ShouldPowerBeRemovedAfterOwnerDeath()
│           ├── 检查 Hook.ShouldPowerBeRemovedOnDeath()
│           └── 只移除符合条件的 Power
│
└── 4. 如果死亡被阻止（复活）
    ├── ★ Hook.AfterDeath(wasRemovalPrevented: true)
    ├── Hook.AfterPreventingDeath()
    └── 如果仍然死亡，递归调用
```

---

## 五、怪物复活与 Power 处理

### 5.1 复活机制概述

怪物复活通过以下机制实现：

1. **阻止从战斗移除**：`ShouldCreatureBeRemovedFromCombatAfterDeath()` 返回 `false`
2. **保留特定 Power**：`ShouldPowerBeRemovedOnDeath()` 控制哪些 Power 被移除
3. **复活后恢复**：通过 `AfterDeath` Hook 触发复活逻辑

### 5.2 复活 Power 示例

#### AdaptablePower（TestSubject 专属）

```csharp
// AdaptablePower.cs
public sealed class AdaptablePower : PowerModel
{
    // 阻止从战斗移除
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        if (creature != base.Owner) return true;
        return false;  // 阻止移除
    }
    
    // Power 不随死亡移除
    public override bool ShouldPowerBeRemovedAfterOwnerDeath()
    {
        return false;
    }
    
    // 死亡时触发复活状态
    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, 
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented && creature == base.Owner)
        {
            GetInternalData<Data>().isReviving = true;
            await testSubject.TriggerDeadState();  // 触发复活流程
        }
    }
}
```

#### IllusionPower（Parafright 专属）

```csharp
// IllusionPower.cs
public sealed class IllusionPower : PowerModel
{
    // 只移除 Debuff 类型的 Power
    public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
    {
        return power.Type == PowerType.Debuff;
    }
    
    // 阻止从战斗移除
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        if (creature != base.Owner) return true;
        return false;
    }
    
    // 死亡时触发复活
    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, 
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented && creature == base.Owner)
        {
            // 设置复活状态
            GetInternalData<Data>().isReviving = true;
            // 设置复活动作
            base.Owner.Monster.SetMoveImmediate(reviveState);
        }
    }
}
```

### 5.3 复活时 Power 处理流程

```
怪物死亡（带复活 Power）
│
├── 1. Hook.BeforeDeath()
│
├── 2. Hook.ShouldDie() → true
│
├── 3. InvokeDiedEvent()
│
├── 4. Hook.ShouldCreatureBeRemovedFromCombatAfterDeath()
│   └── AdaptablePower/IllusionPower 返回 false
│   └── 怪物不从战斗中移除 ✓
│
├── 5. Hook.AfterDeath(wasRemovalPrevented: false)
│   └── AdaptablePower/IllusionPower.AfterDeath()
│       └── 设置复活状态
│
├── 6. 移除 Power
│   └── RemoveAllPowersAfterDeath()
│       ├── AdaptablePower.ShouldPowerBeRemovedAfterOwnerDeath() = false
│       │   └── AdaptablePower 保留 ✓
│       ├── IllusionPower.ShouldPowerBeRemovedOnDeath(power)
│       │   └── 只移除 Debuff 类型
│       │   └── Poison、Vulnerable 等被移除 ✓
│       │   └── Buff 类型保留 ✓
│       └── 其他 Power 被移除
│
└── 7. 复活流程
    ├── 怪物执行复活动作
    ├── 恢复 HP
    └── 可能添加新 Power
```

### 5.4 复活时 Power 的保留与移除

| Power 类型 | AdaptablePower | IllusionPower |
|------------|----------------|---------------|
| 自身 | ✅ 保留 | ✅ 保留 |
| Buff 类型 | ❌ 移除 | ✅ 保留 |
| Debuff 类型 | ❌ 移除 | ❌ 移除 |
| Poison | ❌ 移除 | ❌ 移除 |
| Vulnerable | ❌ 移除 | ❌ 移除 |
| Strength | ❌ 移除 | ✅ 保留 |

---

## 六、Power 清空的完整场景

### 6.1 场景一：普通死亡

```
普通怪物死亡
│
├── ShouldCreatureBeRemovedFromCombatAfterDeath() = true
│   └── 从战斗移除
│
├── ShouldPowerBeRemovedAfterOwnerDeath() = true (默认)
│   └── 所有 Power 移除
│
└── Hook.ShouldPowerBeRemovedOnDeath() = true (默认)
    └── 所有 Power 移除
```

### 6.2 场景二：带复活 Power 的死亡

```
带 AdaptablePower/IllusionPower 的怪物死亡
│
├── ShouldCreatureBeRemovedFromCombatAfterDeath() = false
│   └── 保留在战斗中
│
├── 移除 Power 时
│   ├── AdaptablePower: ShouldPowerBeRemovedAfterOwnerDeath() = false
│   │   └── 自身保留
│   ├── IllusionPower: ShouldPowerBeRemovedOnDeath(power)
│   │   └── Debuff 移除，Buff 保留
│   └── 其他 Power 根据规则移除
│
└── AfterDeath Hook 触发复活
```

### 6.3 场景三：逃跑

```
CreatureCmd.Escape(creature)
│
├── RemoveAllPowersInternalExcept()
│   └── 所有 Power 移除（无例外）
│
├── RemoveCreatureNode()
│
├── CombatManager.RemoveCreature()
│
└── CombatState.CreatureEscaped()
```

### 6.4 场景四：战斗结束

```
战斗结束
│
├── Creature.Reset()
│   ├── RemoveAllPowersInternalExcept()
│   │   └── 所有 Power 移除
│   └── Block = 0
│
└── 清理战斗状态
```

---

## 七、Mod 开发注意事项

### 7.1 追踪 Power 清空的时机

对于需要追踪 Power 的 Mod，需要监听以下事件：

```csharp
// 1. Power 施加
creature.PowerApplied += (power) =>
{
    if (power is PoisonPower || power is DoomPower)
    {
        // 记录施加
    }
};

// 2. Power 移除
creature.PowerRemoved += (power) =>
{
    // 清除记录
};

// 3. 生物死亡
Hook.AfterDeath += (runState, combatState, creature, wasRemovalPrevented, deathAnimLength) =>
{
    if (creature.IsEnemy && !wasRemovalPrevented)
    {
        // 检查是否会复活
        bool willRevive = !Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(combatState, creature);
        
        if (willRevive)
        {
            // 复活场景：部分 Power 可能保留
            // 需要检查 ShouldPowerBeRemovedOnDeath
        }
        else
        {
            // 普通死亡：清除所有 Power 记录
        }
    }
};

// 4. 生物逃跑
// 需要自己 Patch CreatureCmd.Escape 或监听相关事件
```

### 7.2 判断怪物是否会复活

```csharp
bool WillCreatureRevive(Creature creature)
{
    // 检查是否有阻止移除的 Power
    if (creature.HasPower<AdaptablePower>()) return true;
    if (creature.HasPower<IllusionPower>()) return true;
    
    // 检查其他复活 Power
    foreach (var power in creature.Powers)
    {
        if (!power.ShouldPowerBeRemovedAfterOwnerDeath())
        {
            return true;
        }
    }
    
    return false;
}
```

### 7.3 复活时 Power 的处理建议

```csharp
// 在 AfterDeath Hook 中处理
public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, 
    bool wasRemovalPrevented, float deathAnimLength)
{
    if (creature.IsEnemy && wasRemovalPrevented)
    {
        // 死亡被阻止（复活场景）
        // 检查哪些 Power 被移除了
        
        var illusionPower = creature.GetPower<IllusionPower>();
        if (illusionPower != null)
        {
            // IllusionPower 场景：Debuff 被移除
            // 需要清除 Poison、Vulnerable 等记录
        }
    }
    else if (creature.IsEnemy && !wasRemovalPrevented)
    {
        // 检查是否会复活
        bool willRevive = !Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(
            creature.CombatState, creature);
        
        if (willRevive)
        {
            // 会复活，但 Power 可能被部分移除
            // 需要根据 ShouldPowerBeRemovedOnDeath 判断
        }
        else
        {
            // 不会复活，清除所有 Power 记录
            ClearAllPowerRecords(creature);
        }
    }
}
```

---

## 八、关键代码位置

| 功能 | 文件路径 |
|------|----------|
| Power 施加 | `Core/Commands/PowerCmd.cs` - `Apply()` |
| Power 移除 | `Core/Commands/PowerCmd.cs` - `Remove()` |
| Power 基类 | `Core/Models/PowerModel.cs` |
| Creature Power 管理 | `Core/Entities/Creatures/Creature.cs` |
| 死亡处理 | `Core/Commands/CreatureCmd.cs` - `KillWithoutCheckingWinCondition()` |
| 逃跑处理 | `Core/Commands/CreatureCmd.cs` - `Escape()` |
| Hook 定义 | `Core/Hooks/Hook.cs` |
| AdaptablePower | `Core/Models/Powers/AdaptablePower.cs` |
| IllusionPower | `Core/Models/Powers/IllusionPower.cs` |
| TestSubject | `Core/Models/Monsters/TestSubject.cs` |

---

## 九、时序图

### 9.1 普通死亡时序

```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│ Damage  │     │Creature │     │  Hook   │     │ Combat  │     │  Power  │
│  Cmd    │     │  Cmd    │     │         │     │ Manager │     │  Model  │
└────┬────┘     └────┬────┘     └────┬────┘     └────┬────┘     └────┬────┘
     │               │               │               │               │
     │  Damage()     │               │               │               │
     │──────────────>│               │               │               │
     │               │               │               │               │
     │               │ Kill()        │               │               │
     │               │──────────────>│               │               │
     │               │               │               │               │
     │               │               │ BeforeDeath() │               │
     │               │               │──────────────>│               │
     │               │               │               │               │
     │               │               │ ShouldDie()   │               │
     │               │               │──────────────>│               │
     │               │               │               │               │
     │               │ InvokeDied()  │               │               │
     │               │──────────────>│               │               │
     │               │               │               │               │
     │               │               │ AfterDeath()  │               │
     │               │               │──────────────>│               │
     │               │               │               │               │
     │               │               │               │ RemovePowers()│
     │               │               │               │──────────────>│
     │               │               │               │               │
     │               │               │               │               │ RemoveInternal()
     │               │               │               │               │──────────────>
     │               │               │               │               │
     │               │               │               │               │ AfterRemoved()
     │               │               │               │               │──────────────>
     │               │               │               │               │
```

### 9.2 复活时序

```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│ Creature│     │  Hook   │     │Adaptable│     │ Combat  │     │  Power  │
│  Cmd    │     │         │     │ Power   │     │ State   │     │  Model  │
└────┬────┘     └────┬────┘     └────┬────┘     └────┬────┘     └────┬────┘
     │               │               │               │               │
     │ Kill()        │               │               │               │
     │──────────────>│               │               │               │
     │               │               │               │               │
     │               │ BeforeDeath() │               │               │
     │               │──────────────>│               │               │
     │               │               │               │               │
     │               │ ShouldDie()   │               │               │
     │               │──────────────>│               │               │
     │               │               │               │               │
     │               │ ShouldRemoveFromCombat()      │               │
     │               │──────────────>│               │               │
     │               │               │               │               │
     │               │               │ return false  │               │
     │               │<──────────────│               │               │
     │               │               │               │               │
     │               │ AfterDeath(wasRemovalPrevented: false)        │
     │               │──────────────>│               │               │
     │               │               │               │               │
     │               │               │ AfterDeath()  │               │
     │               │               │──────────────>│               │
     │               │               │               │               │
     │               │               │ 设置复活状态  │               │
     │               │               │──────────────>│               │
     │               │               │               │               │
     │               │               │               │ RemovePowers()│
     │               │               │               │──────────────>│
     │               │               │               │               │
     │               │               │               │ 检查保留条件  │
     │               │               │               │──────────────>│
     │               │               │               │               │
     │               │               │               │ 部分Power保留 │
     │               │               │               │<──────────────│
     │               │               │               │               │
     │               │               │               │               │
     │               │               │ 复活流程      │               │
     │               │               │──────────────>│               │
     │               │               │               │               │
```

---

## 十、总结

### Power 清空的关键判断点

1. **`ShouldCreatureBeRemovedFromCombatAfterDeath()`**：决定生物是否从战斗移除
   - `true`：普通死亡，所有 Power 最终会被清除
   - `false`：复活场景，生物保留在战斗中

2. **`ShouldPowerBeRemovedAfterOwnerDeath()`**：Power 自身是否随死亡移除
   - `true`（默认）：随死亡移除
   - `false`：保留（如 AdaptablePower）

3. **`Hook.ShouldPowerBeRemovedOnDeath(power)`**：其他模型是否阻止 Power 移除
   - 可用于实现"死亡时只移除 Debuff"等逻辑

### Mod 开发建议

1. **监听 `PowerRemoved` 事件**：实时清除记录
2. **监听 `AfterDeath` Hook**：处理死亡和复活场景
3. **检查 `wasRemovalPrevented` 参数**：区分普通死亡和复活
4. **检查 `ShouldCreatureBeRemovedFromCombatAfterDeath`**：预判是否会复活
5. **对于复活怪物**：需要根据 Power 类型判断是否被移除
