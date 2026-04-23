# DoomPower 生命周期文档

## 概述

DoomPower（末日）是 Slay the Spire 2 中的一种减益型 Power，属于 Debuff 类型。其核心机制是在持有者的当前生命值低于或等于 Doom 层数时，在回合结束时直接击杀该单位。

## 基本信息

| 属性       | 值                                 |
| -------- | --------------------------------- |
| **类型**   | `PowerType.Debuff`                |
| **叠加方式** | `PowerStackType.Counter`（数值叠加）    |
| **来源文件** | `Core/Models/Powers/DoomPower.cs` |

## 生命周期流程

```
┌─────────────────────────────────────────────────────────────────┐
│                    DoomPower 生命周期                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. 施加阶段 (Apply)                                             │
│     ├── 卡片使用                                                 │
│     ├── 药水使用                                                 │
│     └── 其他 Power 效果 (CountdownPower)                         │
│              │                                                  │
│              ▼                                                  │
│  2. Hook 修改阶段                                                │
│     ├── ModifyPowerAmountGiven (施加者修改)                      │
│     └── ModifyPowerAmountReceived (接收者修改)                   │
│              │                                                  │
│              ▼                                                  │
│  3. 回合结束检测 (BeforeTurnEnd)                                 │
│     ├── 检查是否为持有者回合                                      │
│     ├── 检查持有者是否存活                                        │
│     ├── 检查是否满足末日条件 (CurrentHp <= Amount)               │
│     └── 触发末日击杀 (DoomKill)                                  │
│              │                                                  │
│              ▼                                                  │
│  4. 末日击杀流程 (DoomKill)                                      │
│     ├── 播放视觉效果                                             │
│     ├── 执行击杀 (CreatureCmd.Kill)                              │
│     └── 触发 AfterDiedToDoom Hook                               │
│              │                                                  │
│              ▼                                                  │
│  5. 过期/移除                                                    │
│     └── 目标死亡后随 Power 移除                                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## 核心代码分析

### 1. 施加阶段

DoomPower 通过 `PowerCmd.Apply<DoomPower>()` 方法施加，流程与 PoisonPower 相同：

```csharp
// PowerCmd.cs - Apply 方法核心流程
public static async Task Apply(PowerModel power, Creature target, decimal amount, ...)
{
    // 1. 检查战斗是否结束
    if (CombatManager.Instance.IsEnding) return;
    
    // 2. 检查目标是否可接收 Power
    if (!target.CanReceivePowers) return;
    
    // 3. 触发 Hook 修改层数
    modifiedAmount = Hook.ModifyPowerAmountGiven(...);
    modifiedAmount = Hook.ModifyPowerAmountReceived(...);
    
    // 4. 应用 Power
    power.ApplyInternal(target, modifiedAmount, silent);
    
    // 5. 记录历史
    CombatManager.Instance.History.PowerReceived(...);
}
```

### 2. 末日条件检测

```csharp
// DoomPower.cs - IsOwnerDoomed
public bool IsOwnerDoomed()
{
    // 当前生命值 <= Doom 层数时触发末日
    return base.Owner.CurrentHp <= base.Amount;
}
```

### 3. 回合结束触发

```csharp
// DoomPower.cs - BeforeTurnEnd
public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
{
    // 检查条件：
    // 1. 战斗未结束
    // 2. 是持有者的回合
    // 3. 持有者未死亡
    // 4. 满足末日条件
    if (!CombatManager.Instance.IsOverOrEnding && 
        side == base.Owner.Side && 
        !base.Owner.IsDead && 
        IsOwnerDoomed())
    {
        // 获取同阵营所有满足末日条件的单位
        IReadOnlyList<Creature> doomedCreatures = GetDoomedCreatures(
            base.Owner.CombatState.GetCreaturesOnSide(side)
        );
        
        // 只有第一个满足条件的单位触发击杀流程
        // (避免重复触发)
        if (doomedCreatures.First() == base.Owner)
        {
            await DoomKill(doomedCreatures);
        }
    }
}
```

### 4. 末日击杀流程

```csharp
// DoomPower.cs - DoomKill
public static async Task DoomKill(IReadOnlyList<Creature> creatures)
{
    if (creatures.Count == 0) return;
    
    CombatState combatState = creatures.First().CombatState;
    
    foreach (Creature creature in creatures)
    {
        // 1. 播放视觉效果
        await PlayVfx(creature);
        
        // 2. 执行击杀
        await CreatureCmd.Kill(creature);
    }
    
    // 3. 触发 AfterDiedToDoom Hook
    await Hook.AfterDiedToDoom(combatState, creatures);
}
```

### 5. 获取末日单位

```csharp
// DoomPower.cs - GetDoomedCreatures
public static IReadOnlyList<Creature> GetDoomedCreatures(IReadOnlyList<Creature> creatures)
{
    return creatures.Where(c => 
        c.GetPower<DoomPower>()?.IsOwnerDoomed() ?? false
    ).ToList();
}
```

### 6. 视觉效果播放

```csharp
// DoomPower.cs - PlayVfx
private static async Task PlayVfx(Creature creature)
{
    NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
    if (nCreature == null) return;
    
    // 检查是否应该消失（怪物特殊处理）
    bool shouldDie = false;
    if (creature.IsMonster)
    {
        shouldDie = Hook.ShouldDie(...) && creature.Monster.ShouldDisappearFromDoom;
    }
    
    // 开始末日动画
    StartDoomAnim(nCreature, shouldDie);
    
    // 显示末日覆盖效果
    NDoomOverlayVfx overlay = NDoomOverlayVfx.GetOrCreate();
    if (overlay != null && !overlay.IsInsideTree())
    {
        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(overlay);
    }
    
    // 等待动画完成
    if (shouldDie)
    {
        List<Creature> teammates = creature.CombatState.GetTeammatesOf(creature)
            .Where(c => c.IsAlive)
            .ToList();
        
        if (teammates.Count != 1 || teammates.First() != creature)
        {
            await Cmd.Wait(0.25f);
        }
        else
        {
            await Cmd.Wait(1.5f);  // 最后一个敌人时等待更长时间
        }
    }
}
```

### 7. 末日动画

```csharp
// DoomPower.cs - StartDoomAnim
private static void StartDoomAnim(NCreature creature, bool shouldDie)
{
    Task task = null;
    
    if (shouldDie)
    {
        // 触发怪物死亡回调
        creature.Entity.Monster?.OnDieToDoom();
        
        // 禁用 UI 并播放消失动画
        Tween tween = creature.AnimDisableUi();
        tween.TweenCallback(Callable.From(creature.QueueFreeSafely));
        task = WaitForTween(tween);
        
        // 设置受击动画
        if (creature.SpineAnimation.IsValid)
        {
            creature.SetAnimationTrigger("Hit");
            // ... 动画处理
        }
        
        // 从战斗房间移除
        NCombatRoom.Instance?.RemoveCreatureNode(creature);
    }
    
    // 创建末日视觉效果
    NDoomVfx doomVfx = NDoomVfx.Create(
        creature.Visuals, 
        creature.Hitbox.GlobalPosition, 
        creature.Hitbox.Size, 
        shouldDie
    );
    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(doomVfx);
    
    if (shouldDie)
    {
        creature.DeathAnimationTask = Task.WhenAll(task, doomVfx.VfxTask);
    }
}
```

## 施加来源

### 卡片

| 卡片                                                                                           | 能量 | 稀有度    | 目标   | 基础层数 | 升级后 |
| -------------------------------------------------------------------------------------------- | -- | ------ | ---- | ---- | --- |
| [Scourge](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/Scourge.cs)     | 1  | Common | 单体敌人 | 13   | 16  |
| [EndOfDays](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/EndOfDays.cs) | 3  | Rare   | 所有敌人 | 29   | 37  |

### 药水

| 药水                                                                                                   | 稀有度    | 目标   | 层数 |
| ---------------------------------------------------------------------------------------------------- | ------ | ---- | -- |
| [PotionOfDoom](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Potions/PotionOfDoom.cs) | Common | 单体敌人 | 33 |

### 其他 Power 效果

| Power                                                                                                   | 触发条件 | 效果          |
| ------------------------------------------------------------------------------------------------------- | ---- | ----------- |
| [CountdownPower](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/CountdownPower.cs) | 回合开始 | 随机敌人获得 Doom |

```csharp
// CountdownPower.cs
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
{
    if (side == base.Owner.Side)
    {
        Creature target = base.Owner.Player.RunState.Rng.CombatTargets
            .NextItem(base.CombatState.HittableEnemies);
        if (target != null)
        {
            await PowerCmd.Apply<DoomPower>(target, base.Amount, base.Owner, null);
        }
    }
}
```

## 交互机制

### 与 UndyingSigil 遗物的交互

[UndyingSigil](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Relics/UndyingSigil.cs) 会降低来自末日状态敌人的伤害：

```csharp
public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, 
    ValueProp props, Creature? dealer, CardModel? cardSource)
{
    // 条件检查：
    // 1. 是攻击伤害
    // 2. 目标是遗物持有者
    // 3. 攻击者不是自己
    // 4. 攻击者处于末日状态 (CurrentHp <= DoomPower层数)
    
    if (props.IsPoweredAttack() && 
        target == base.Owner.Creature && 
        dealer != base.Owner.Creature &&
        dealer.CurrentHp <= dealer.GetPowerAmount<DoomPower>())
    {
        return base.DynamicVars["DamageDecrease"].BaseValue;  // 0.5 (伤害减半)
    }
    return 1m;
}
```

### 与 BookRepairKnife 遗物的交互

[BookRepairKnife](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Relics/BookRepairKnife.cs) 在敌人死于末日时治疗持有者：

```csharp
public override Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
{
    // 计算死于末日的敌人数量（不包括自己）
    int count = creatures.Count(c => 
        c != base.Owner.Creature && 
        c.Powers.All(p => p.ShouldOwnerDeathTriggerFatal())
    );
    
    if (count > 0)
    {
        Flash();
        // 每个敌人治疗 3 点
        return CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue * count);
    }
    return Task.CompletedTask;
}
```

### AfterDiedToDoom Hook

当单位死于末日时会触发 `Hook.AfterDiedToDoom`：

```csharp
// Hook.cs
public static async Task AfterDiedToDoom(CombatState combatState, IReadOnlyList<Creature> creatures)
{
    foreach (AbstractModel model in combatState.IterateHookListeners())
    {
        HookPlayerChoiceContext context = new HookPlayerChoiceContext(model, ...);
        Task task = model.AfterDiedToDoom(context, creatures);
        await context.AssignTaskAndWaitForPauseOrCompletion(task);
        model.InvokeExecutionFinished();
    }
}
```

## 与 PoisonPower 的关键区别

| 特性       | PoisonPower                 | DoomPower              |
| -------- | --------------------------- | ---------------------- |
| **触发时机** | 回合开始 (`AfterSideTurnStart`) | 回合结束 (`BeforeTurnEnd`) |
| **触发效果** | 造成伤害并递减                     | 满足条件时直接击杀              |
| **伤害类型** | 不可格挡、不可强化                   | 无伤害，直接击杀               |
| **层数变化** | 每次触发后 -1                    | 不自动变化                  |
| **移除条件** | 层数降为 0                      | 目标死亡后移除                |
| **死亡触发** | 可能因伤害死亡                     | 直接调用 Kill              |

## 时序图

```
时间线
  │
  ├─ 玩家打出 Scourge
  │    │
  │    ├─ PowerCmd.Apply<DoomPower>(target, 13, player, card)
  │    │    │
  │    │    ├─ Hook.ModifyPowerAmountGiven
  │    │    ├─ Hook.ModifyPowerAmountReceived
  │    │    └─ power.ApplyInternal(target, 13)
  │    │
  │    └─ 敌人获得 13 层 Doom
  │         （敌人 HP: 50，Doom: 13，不满足末日条件）
  │
  ├─ 敌人回合结束
  │    │
  │    ├─ Hook.BeforeTurnEnd(Enemy)
  │    │    │
  │    │    └─ DoomPower.BeforeTurnEnd(choiceContext, Enemy)
  │    │         │
  │    │         ├─ IsOwnerDoomed(): 50 <= 13? → false
  │    │         └─ 不触发末日击杀
  │    │
  │    └─ 敌人回合正常结束
  │
  ├─ 玩家回合
  │    │
  │    └─ 玩家对敌人造成 40 点伤害
  │         → 敌人 HP: 10，Doom: 13
  │
  ├─ 敌人回合结束
  │    │
  │    ├─ DoomPower.BeforeTurnEnd(choiceContext, Enemy)
  │    │    │
  │    │    ├─ IsOwnerDoomed(): 10 <= 13? → true ✓
  │    │    │
  │    │    ├─ GetDoomedCreatures() → [enemy]
  │    │    │
  │    │    └─ DoomKill([enemy])
  │    │         │
  │    │         ├─ PlayVfx(enemy)
  │    │         │    ├─ StartDoomAnim(creature, shouldDie)
  │    │         │    └─ 显示末日视觉效果
  │    │         │
  │    │         ├─ CreatureCmd.Kill(enemy)
  │    │         │    ├─ Hook.BeforeDeath
  │    │         │    ├─ creature.InvokeDiedEvent()
  │    │         │    ├─ Hook.AfterDeath
  │    │         │    └─ 移除所有 Power
  │    │         │
  │    │         └─ Hook.AfterDiedToDoom(combatState, [enemy])
  │    │              └─ BookRepairKnife.AfterDiedToDoom (如果持有)
  │    │                   → 治疗 3 HP
  │    │
  │    └─ 敌人死亡，战斗可能结束
  │
  └─ DoomPower 随目标死亡而移除
```

## EndOfDays 特殊处理

[EndOfDays](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Cards/EndOfDays.cs) 卡片会在施加 Doom 后立即检查并击杀满足条件的敌人：

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 1. 播放施法动画和视觉效果
    await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", ...);
    // ... VFX ...
    
    // 2. 对所有敌人施加 Doom
    foreach (Creature enemy in base.CombatState.HittableEnemies)
    {
        await PowerCmd.Apply<DoomPower>(enemy, 29, base.Owner.Creature, this);
    }
    
    // 3. 立即击杀满足末日条件的敌人
    await DoomPower.DoomKill(
        DoomPower.GetDoomedCreatures(base.CombatState.HittableEnemies)
    );
}
```

这意味着如果敌人 HP <= 29，使用 EndOfDays 会立即击杀它们。

## CombatHistory 记录系统

### 相关 Entry 类型

CombatHistory 会记录 DoomPower 相关的以下事件：

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
│  1. 施加 Doom                                                   │
│     PowerCmd.Apply() → PowerReceivedEntry 记录                  │
│     ├── Power: DoomPower 实例                                   │
│     ├── Amount: 施加层数                                        │
│     └── Applier: 施加者 ✓                                       │
│                                                                 │
│  2. Doom 触发击杀                                                │
│     CreatureCmd.Kill() → 无直接记录                             │
│     ├── 不产生 DamageReceivedEntry (直接击杀，无伤害)            │
│     └── 需要通过 Hook.AfterDiedToDoom 追踪                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 代码示例

```csharp
// 获取所有 Doom 施加记录
var doomApplies = CombatManager.Instance.History.Entries
    .OfType<PowerReceivedEntry>()
    .Where(e => e.Power is DoomPower);

foreach (var entry in doomApplies)
{
    Console.WriteLine($"回合 {entry.RoundNumber}: " +
        $"{entry.Applier?.ModelId.Entry ?? "未知"} " +
        $"对 {entry.Actor.ModelId.Entry} " +
        $"施加 {entry.Amount} 层 Doom");
}

// 注意：Doom 击杀不会产生 DamageReceivedEntry
// 因为是直接调用 CreatureCmd.Kill()，不是 Damage()
```

### 与 PoisonPower 的关键区别

| 特性 | PoisonPower | DoomPower |
|------|-------------|-----------|
| **触发效果** | 造成伤害 | 直接击杀 |
| **CombatHistory 记录** | `DamageReceivedEntry` (Dealer=null) | 无伤害记录 |
| **追踪方式** | 可通过伤害记录追踪 | 需通过 `Hook.AfterDiedToDoom` 追踪 |

### 末日击杀追踪方案

DoomPower 击杀时**不会产生伤害记录**，需要通过 Hook 追踪：

```csharp
// 方案1：通过 Hook.AfterDiedToDoom 追踪
public override Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
{
    foreach (var creature in creatures)
    {
        var doomPower = creature.GetPower<DoomPower>();
        Console.WriteLine($"末日击杀: {creature.ModelId.Entry}");
        Console.WriteLine($"  Doom 层数: {doomPower?.Amount}");
        Console.WriteLine($"  施加者: {doomPower?.Applier?.ModelId.Entry ?? "未知"}");
    }
    return Task.CompletedTask;
}

// 方案2：结合 CombatHistory 查找施加记录
void TrackDoomKill(Creature killedCreature)
{
    var doomPower = killedCreature.GetPower<DoomPower>();
    if (doomPower == null) return;
    
    // 查找所有施加 Doom 的记录
    var doomApplies = CombatManager.Instance.History.Entries
        .OfType<PowerReceivedEntry>()
        .Where(e => e.Actor == killedCreature && e.Power is DoomPower);
    
    foreach (var entry in doomApplies)
    {
        Console.WriteLine($"施加者: {entry.Applier?.ModelId.Entry}, 层数: {entry.Amount}");
    }
}
```

### 局限性分析

| 问题 | 说明 |
|------|------|
| **击杀无伤害记录** | Doom 击杀是直接 `Kill()`，不产生 `DamageReceivedEntry` |
| **施加者只能记录一人** | `Power.Applier` 只记录第一个施加者，后续叠加不会更新 |
| **多人施加无法区分** | 多人施加的 Doom 会合并层数，无法区分各自贡献 |

### 多人施加追踪方案

```
场景：玩家A 施加 13 层，玩家B 施加 5 层，敌人 HP: 15

CombatHistory 记录：
  ├─ PowerReceivedEntry { Applier: 玩家A, Amount: 13 }
  └─ PowerReceivedEntry { Applier: 玩家B, Amount: 5 }

Power.Applier 值：
  └─ 玩家A (只记录第一个)

Doom 击杀时：
  └─ 敌人 HP(15) <= Doom(18) → 触发击杀
     但无法知道是谁 "完成" 了击杀
```

**Mod 开发建议**：需要自己维护施加者追踪字典

```csharp
// 建议的数据结构
Dictionary<Creature, List<DoomApplierRecord>> doomTracker;

class DoomApplierRecord
{
    Creature Applier;    // 施加者
    int Amount;          // 施加层数
    int RoundNumber;     // 施加回合
}

// 在 PowerReceived 时记录
void OnPowerReceived(PowerReceivedEntry entry)
{
    if (entry.Power is DoomPower)
    {
        var tracker = GetOrCreateTracker(entry.Actor);
        tracker.Add(new DoomApplierRecord
        {
            Applier = entry.Applier,
            Amount = (int)entry.Amount,
            RoundNumber = entry.RoundNumber
        });
    }
}
```

## 关键代码位置

| 功能           | 文件路径                                                                                                                    |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| DoomPower 定义 | [Core/Models/Powers/DoomPower.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/Powers/DoomPower.cs)     |
| Power 基类     | [Core/Models/PowerModel.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Models/PowerModel.cs)                 |
| Power 命令     | [Core/Commands/PowerCmd.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Commands/PowerCmd.cs)                 |
| Hook 系统      | [Core/Hooks/Hook.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Hooks/Hook.cs)                               |
| 击杀命令         | [Core/Commands/CreatureCmd.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Commands/CreatureCmd.cs)           |
| 战斗管理器        | [Core/Combat/CombatManager.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/CombatManager.cs)           |
| 末日 VFX       | [Core/Nodes/Vfx/NDoomVfx.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Nodes/Vfx/NDoomVfx.cs)               |
| 末日覆盖 VFX     | [Core/Nodes/Vfx/NDoomOverlayVfx.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Nodes/Vfx/NDoomOverlayVfx.cs) |
| 战斗历史 | [Core/Combat/History/CombatHistory.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/History/CombatHistory.cs) |
| Power 记录 Entry | [Core/Combat/History/Entries/PowerReceivedEntry.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/History/Entries/PowerReceivedEntry.cs) |
| 伤害记录 Entry | [Core/Combat/History/Entries/DamageReceivedEntry.cs](file:///d:/Godot/Slay%20the%20Spire2%20-%20O/src/Core/Combat/History/Entries/DamageReceivedEntry.cs) |

