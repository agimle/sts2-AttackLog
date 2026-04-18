# 战斗日志Mod开发指南

## 项目结构

```
sts2-AttackLog/
├── 文档/
│   ├── 战斗日志Mod源代码分析.md
│   ├── 战斗历史系统API参考.md
│   └── 战斗日志Mod开发指南.md
├── Scripts/
│   └── ModEntry.cs
├── project.godot
├── sts2-AttackLog.csproj
└── sts2-AttackLog.sln
```

---

## 命名空间引用

开发战斗日志Mod需要引用以下命名空间：

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
```

---

## Mod入口类

### 基本结构

```csharp
using MegaCrit.Sts2.Core.Modding;

public class ModEntry : Mod
{
    public override string Name => "Attack Log";
    public override string Version => "1.0.0";
    
    public override void OnLoad()
    {
        // Mod加载时执行
    }
    
    public override void OnUnload()
    {
        // Mod卸载时执行
    }
}
```

---

## 核心功能实现

### 1. 战斗数据收集器

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;

public class CombatDataCollector
{
    private readonly Dictionary<int, RoundData> _roundData = new();
    private int _currentRound;
    
    public IReadOnlyDictionary<int, RoundData> RoundData => _roundData;
    
    public void Initialize()
    {
        CombatManager.Instance.CombatSetUp += OnCombatSetUp;
        CombatManager.Instance.CombatEnded += OnCombatEnded;
        CombatManager.Instance.TurnStarted += OnTurnStarted;
    }
    
    public void Cleanup()
    {
        CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
        CombatManager.Instance.CombatEnded -= OnCombatEnded;
        CombatManager.Instance.TurnStarted -= OnTurnStarted;
    }
    
    private void OnCombatSetUp(CombatState state)
    {
        _roundData.Clear();
        _currentRound = 0;
        CombatManager.Instance.History.Changed += OnHistoryChanged;
    }
    
    private void OnCombatEnded(CombatRoom room)
    {
        CombatManager.Instance.History.Changed -= OnHistoryChanged;
    }
    
    private void OnTurnStarted(CombatState state)
    {
        _currentRound = state.RoundNumber;
        if (!_roundData.ContainsKey(_currentRound))
        {
            _roundData[_currentRound] = new RoundData { RoundNumber = _currentRound };
        }
    }
    
    private void OnHistoryChanged()
    {
        var history = CombatManager.Instance.History;
        ProcessEntries(history.Entries);
    }
    
    private void ProcessEntries(IEnumerable<CombatHistoryEntry> entries)
    {
        foreach (var entry in entries)
        {
            ProcessEntry(entry);
        }
    }
    
    private void ProcessEntry(CombatHistoryEntry entry)
    {
        if (!_roundData.TryGetValue(entry.RoundNumber, out var roundData))
        {
            roundData = new RoundData { RoundNumber = entry.RoundNumber };
            _roundData[entry.RoundNumber] = roundData;
        }
        
        switch (entry)
        {
            case DamageReceivedEntry damageEntry:
                ProcessDamageEntry(roundData, damageEntry);
                break;
            case BlockGainedEntry blockEntry:
                ProcessBlockEntry(roundData, blockEntry);
                break;
            case CardPlayFinishedEntry cardEntry:
                ProcessCardEntry(roundData, cardEntry);
                break;
            case EnergySpentEntry energyEntry:
                ProcessEnergyEntry(roundData, energyEntry);
                break;
        }
    }
    
    private void ProcessDamageEntry(RoundData roundData, DamageReceivedEntry entry)
    {
        if (entry.Dealer != null && entry.Dealer.IsPlayer)
        {
            roundData.DamageDealt += entry.Result.UnblockedDamage;
            if (entry.Result.WasTargetKilled)
            {
                roundData.Kills++;
            }
        }
        
        if (entry.Receiver.IsPlayer)
        {
            roundData.DamageTaken += entry.Result.UnblockedDamage;
        }
    }
    
    private void ProcessBlockEntry(RoundData roundData, BlockGainedEntry entry)
    {
        if (entry.Receiver.IsPlayer)
        {
            roundData.BlockGained += entry.Amount;
        }
    }
    
    private void ProcessCardEntry(RoundData roundData, CardPlayFinishedEntry entry)
    {
        roundData.CardsPlayed++;
    }
    
    private void ProcessEnergyEntry(RoundData roundData, EnergySpentEntry entry)
    {
        roundData.EnergySpent += entry.Amount;
    }
}

public class RoundData
{
    public int RoundNumber { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public int BlockGained { get; set; }
    public int CardsPlayed { get; set; }
    public int EnergySpent { get; set; }
    public int Kills { get; set; }
}
```

### 2. 战斗统计报告生成器

```csharp
using System.Text;

public class CombatReportGenerator
{
    public string GenerateReport(CombatDataCollector collector)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== 战斗统计报告 ===");
        sb.AppendLine();
        
        int totalDamageDealt = 0;
        int totalDamageTaken = 0;
        int totalBlockGained = 0;
        int totalCardsPlayed = 0;
        int totalEnergySpent = 0;
        int totalKills = 0;
        
        foreach (var kvp in collector.RoundData.OrderBy(x => x.Key))
        {
            var round = kvp.Value;
            sb.AppendLine($"--- 回合 {round.RoundNumber} ---");
            sb.AppendLine($"造成伤害: {round.DamageDealt}");
            sb.AppendLine($"受到伤害: {round.DamageTaken}");
            sb.AppendLine($"获得护盾: {round.BlockGained}");
            sb.AppendLine($"打出卡牌: {round.CardsPlayed}");
            sb.AppendLine($"消耗能量: {round.EnergySpent}");
            sb.AppendLine($"击杀数: {round.Kills}");
            sb.AppendLine();
            
            totalDamageDealt += round.DamageDealt;
            totalDamageTaken += round.DamageTaken;
            totalBlockGained += round.BlockGained;
            totalCardsPlayed += round.CardsPlayed;
            totalEnergySpent += round.EnergySpent;
            totalKills += round.Kills;
        }
        
        sb.AppendLine("=== 总计 ===");
        sb.AppendLine($"总回合数: {collector.RoundData.Count}");
        sb.AppendLine($"总造成伤害: {totalDamageDealt}");
        sb.AppendLine($"总受到伤害: {totalDamageTaken}");
        sb.AppendLine($"总获得护盾: {totalBlockGained}");
        sb.AppendLine($"总打出卡牌: {totalCardsPlayed}");
        sb.AppendLine($"总消耗能量: {totalEnergySpent}");
        sb.AppendLine($"总击杀数: {totalKills}");
        
        return sb.ToString();
    }
}
```

### 3. 详细伤害日志记录器

```csharp
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat.History.Entries;

public class DamageLogEntry
{
    public int RoundNumber { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
    public string CardName { get; set; }
    public int TotalDamage { get; set; }
    public int BlockedDamage { get; set; }
    public int ActualDamage { get; set; }
    public bool WasKill { get; set; }
}

public class DetailedDamageLogger
{
    private readonly List<DamageLogEntry> _entries = new();
    
    public IReadOnlyList<DamageLogEntry> Entries => _entries;
    
    public void Initialize()
    {
        CombatManager.Instance.History.Changed += OnHistoryChanged;
    }
    
    public void Cleanup()
    {
        CombatManager.Instance.History.Changed -= OnHistoryChanged;
    }
    
    public void Clear()
    {
        _entries.Clear();
    }
    
    private void OnHistoryChanged()
    {
        var history = CombatManager.Instance.History;
        
        foreach (var entry in history.Entries.OfType<DamageReceivedEntry>())
        {
            if (!_entries.Any(e => e.RoundNumber == entry.RoundNumber && 
                                   e.Target == entry.Receiver.Name &&
                                   e.ActualDamage == entry.Result.UnblockedDamage))
            {
                _entries.Add(new DamageLogEntry
                {
                    RoundNumber = entry.RoundNumber,
                    Source = entry.Dealer?.Name ?? "Unknown",
                    Target = entry.Receiver.Name,
                    CardName = entry.CardSource?.Id.Entry ?? "Unknown",
                    TotalDamage = entry.Result.TotalDamage,
                    BlockedDamage = entry.Result.BlockedDamage,
                    ActualDamage = entry.Result.UnblockedDamage,
                    WasKill = entry.Result.WasTargetKilled
                });
            }
        }
    }
    
    public string GenerateLog()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== 详细伤害日志 ===");
        
        foreach (var entry in _entries.OrderBy(e => e.RoundNumber))
        {
            sb.AppendLine($"[回合{entry.RoundNumber}] {entry.Source} -> {entry.Target}");
            sb.AppendLine($"  卡牌: {entry.CardName}");
            sb.AppendLine($"  总伤害: {entry.TotalDamage} (阻挡: {entry.BlockedDamage}, 实际: {entry.ActualDamage})");
            if (entry.WasKill)
            {
                sb.AppendLine($"  [击杀!]");
            }
        }
        
        return sb.ToString();
    }
}
```

---

## 完整Mod示例

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;

public class ModEntry : Mod
{
    public override string Name => "Attack Log";
    public override string Version => "1.0.0";
    
    private CombatDataCollector _dataCollector;
    private DetailedDamageLogger _damageLogger;
    private CombatReportGenerator _reportGenerator;
    
    public override void OnLoad()
    {
        _dataCollector = new CombatDataCollector();
        _damageLogger = new DetailedDamageLogger();
        _reportGenerator = new CombatReportGenerator();
        
        _dataCollector.Initialize();
        _damageLogger.Initialize();
        
        CombatManager.Instance.CombatEnded += OnCombatEnded;
    }
    
    public override void OnUnload()
    {
        _dataCollector.Cleanup();
        _damageLogger.Cleanup();
        
        CombatManager.Instance.CombatEnded -= OnCombatEnded;
    }
    
    private void OnCombatEnded(CombatRoom room)
    {
        var report = _reportGenerator.GenerateReport(_dataCollector);
        var damageLog = _damageLogger.GenerateLog();
        
        SaveReport(report, damageLog);
        
        _dataCollector = new CombatDataCollector();
        _damageLogger.Clear();
    }
    
    private void SaveReport(string report, string damageLog)
    {
        var fullReport = report + "\n\n" + damageLog;
        var filePath = $"user://attack_logs/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        
        var dir = System.IO.Path.GetDirectoryName(filePath);
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }
        
        System.IO.File.WriteAllText(filePath, fullReport);
    }
}
```

---

## 数据持久化

### 保存到文件

```csharp
public class AttackLogSaver
{
    private readonly string _logDirectory;
    
    public AttackLogSaver(string logDirectory = "user://attack_logs")
    {
        _logDirectory = logDirectory;
        EnsureDirectoryExists();
    }
    
    private void EnsureDirectoryExists()
    {
        if (!System.IO.Directory.Exists(_logDirectory))
        {
            System.IO.Directory.CreateDirectory(_logDirectory);
        }
    }
    
    public void SaveLog(string content, string fileName = null)
    {
        fileName ??= $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        var filePath = System.IO.Path.Combine(_logDirectory, fileName);
        System.IO.File.WriteAllText(filePath, content);
    }
    
    public IEnumerable<string> GetAllLogs()
    {
        return System.IO.Directory.GetFiles(_logDirectory, "*.txt")
            .OrderByDescending(f => f)
            .Select(System.IO.File.ReadAllText);
    }
}
```

### JSON序列化

```csharp
using System.Text.Json;

public class CombatStatsSerializer
{
    public string Serialize(CombatDataCollector collector)
    {
        var data = new
        {
            Rounds = collector.RoundData.Select(kvp => new
            {
                RoundNumber = kvp.Value.RoundNumber,
                DamageDealt = kvp.Value.DamageDealt,
                DamageTaken = kvp.Value.DamageTaken,
                BlockGained = kvp.Value.BlockGained,
                CardsPlayed = kvp.Value.CardsPlayed,
                EnergySpent = kvp.Value.EnergySpent,
                Kills = kvp.Value.Kills
            })
        };
        
        return JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}
```

---

## 性能优化建议

### 1. 使用增量更新

不要每次都遍历所有历史条目，只处理新增的条目：

```csharp
private int _lastProcessedIndex = 0;

private void OnHistoryChanged()
{
    var entries = CombatManager.Instance.History.Entries.ToList();
    
    for (int i = _lastProcessedIndex; i < entries.Count; i++)
    {
        ProcessEntry(entries[i]);
    }
    
    _lastProcessedIndex = entries.Count;
}
```

### 2. 使用对象池

对于频繁创建的对象，使用对象池减少GC压力：

```csharp
public class RoundDataPool
{
    private readonly Stack<RoundData> _pool = new();
    
    public RoundData Get()
    {
        return _pool.Count > 0 ? _pool.Pop() : new RoundData();
    }
    
    public void Return(RoundData data)
    {
        data.Reset();
        _pool.Push(data);
    }
}
```

### 3. 异步保存

将文件保存操作放到异步任务中：

```csharp
public async Task SaveLogAsync(string content, string filePath)
{
    await Task.Run(() => System.IO.File.WriteAllText(filePath, content));
}
```

---

## 调试技巧

### 日志输出

```csharp
using MegaCrit.Sts2.Core.Logging;

public class DebugLogger
{
    public static void Log(string message)
    {
        Log.Info($"[AttackLog] {message}");
    }
    
    public static void LogWarning(string message)
    {
        Log.Warn($"[AttackLog] {message}");
    }
    
    public static void LogError(string message)
    {
        Log.Error($"[AttackLog] {message}");
    }
}
```

### 条件断点

在开发过程中，可以在关键位置添加条件断点：

```csharp
private void ProcessEntry(CombatHistoryEntry entry)
{
    if (entry is DamageReceivedEntry damageEntry)
    {
        if (damageEntry.Result.WasTargetKilled)
        {
            Debugger.Break();
        }
    }
}
```

---

*文档生成日期: 2026-04-18*
