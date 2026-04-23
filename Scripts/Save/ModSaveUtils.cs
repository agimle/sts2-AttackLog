using System.Diagnostics;
using System.Text.Json;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Save;

public static class SaveUtils
{
    /// <summary>
    /// 加载数据文件
    /// </summary>
    /// <param name="runState"></param>
    public static void Load(IRunState runState)
    {
        // 不存在文件
        if (!File.Exists(SaveConfig.SavePath))
        {
            LogState.Instance.ModSave = new ModSave(runState);
        }
        else
        {
            try
            {
                string saveJson = File.ReadAllText(SaveConfig.SavePath);
                ModSave? modSave = JsonSerializer.Deserialize<ModSave>(saveJson, SaveConfig.JsonOptions);
                if(modSave == null || modSave.Seed != runState.Rng.Seed)
                {
                    LogState.Instance.ModSave = new ModSave(runState);
                }
                else
                {
                    LogState.Instance.ModSave = modSave;
                }
            }
            catch
            {
                Debug.WriteLine("读取存档失败");
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
        
        try
        {
            string saveJson = JsonSerializer.Serialize(LogState.Instance.ModSave, SaveConfig.JsonOptions);
            File.WriteAllText(SaveConfig.SavePath, saveJson);
        }
        catch
        {
            Debug.WriteLine("保存失败");
        }
    }
}