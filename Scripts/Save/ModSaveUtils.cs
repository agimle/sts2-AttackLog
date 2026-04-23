using System.Diagnostics;
using System.Text.Json;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Save;

public static class ModSaveUtils
{
    /// <summary>
    /// 加载数据文件
    /// </summary>
    /// <param name="runState"></param>
    public static void Load(IRunState runState)
    {
        // 不存在文件
        if (!File.Exists(ModSaveConfig.SavePath))
        {
            LogState.Instance.ModSave = new ModSave(runState);
            OutputData();
        }
        else
        {
            try
            {
                string saveJson = File.ReadAllText(ModSaveConfig.SavePath);
                ModSave? modSave = JsonSerializer.Deserialize<ModSave>(saveJson, ModSaveConfig.JsonOptions);
                if(modSave == null || modSave.Seed != runState.Rng.Seed)
                {
                    LogState.Instance.ModSave = new ModSave(runState);
                    OutputData();
                }
                else
                {
                    LogState.Instance.ModSave = modSave;
                    InputData();
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }

    /// <summary>
    /// 保存数据存档
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    public static void Save(IRunState? runState, CombatState? combatState)
    {
        if (LogState.Instance.ModSave == null)
        {
            return;
        }
        
        if(runState?.Rng.Seed != LogState.Instance.ModSave.Seed)
        {
            return;
        }
        
        OutputData();
        
        try
        {
            string saveJson = JsonSerializer.Serialize(LogState.Instance.ModSave, ModSaveConfig.JsonOptions);
            File.WriteAllText(ModSaveConfig.SavePath, saveJson);
        }
        catch
        {
            
        }
    }

    /// <summary>
    /// 把存档数据传入mod运行数据
    /// </summary>
    private static void InputData()
    {
        if(LogState.Instance.RunLog == null) return;
        if(LogState.Instance.ModSave == null) return;
        
        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            player.RunLogData = LogState.Instance.ModSave.PlayerDataDict.TryGetValue(player.PlayerInfo.NetId, out var playerData) ? playerData : new SingleRunLogData();
        }
    }

    /// <summary>
    /// 把mod运行数据传入存档数据
    /// </summary>
    private static void OutputData()
    {
        if(LogState.Instance.RunLog == null) return;
        if (LogState.Instance.ModSave == null) return;
        
        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            LogState.Instance.ModSave.PlayerDataDict[player.PlayerInfo.NetId] = player.RunLogData;
        }
    }
}