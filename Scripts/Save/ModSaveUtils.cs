using System.Text.Json;
using AttackLog.Logger;
using AttackLog.Model;
using AttackLog.State;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Save;

/// <summary>
/// 存档工具类，负责 Mod 存档的加载与保存。
/// 加载时校验种子一致性，不一致则重建存档；保存时同步运行数据到存档
/// </summary>
public static class ModSaveUtils
{
    /// <summary>
    /// 从磁盘加载存档。若文件不存在或种子不匹配，则创建新存档
    /// </summary>
    /// <param name="runState">当前 Run 状态，用于种子校验和新存档初始化</param>
    public static void Load(IRunState runState)
    {
        if (!File.Exists(ModSaveConfig.SavePath))
        {
            LogState.Instance.ModSave = new ModSave(runState);
            OutputData();
            return;
        }

        try
        {
            string saveJson = File.ReadAllText(ModSaveConfig.SavePath);
            ModSave? modSave = JsonSerializer.Deserialize<ModSave>(saveJson, ModSaveConfig.JsonOptions);
            if (modSave == null || modSave.Seed != runState.Rng.Seed)
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
            ModLogger.Error("Save", $"Failed to load save: {ex.Message}");
            LogState.Instance.ModSave = new ModSave(runState);
            OutputData();
        }
    }

    /// <summary>
    /// 将当前运行数据保存到磁盘。仅在种子匹配时执行保存
    /// </summary>
    /// <param name="runState">当前 Run 状态</param>
    /// <param name="combatState">当前战斗状态</param>
    public static void Save(IRunState? runState, CombatState? combatState)
    {
        if (LogState.Instance.ModSave == null) return;
        if (runState?.Rng.Seed != LogState.Instance.ModSave.Seed) return;

        OutputData();

        try
        {
            string saveJson = JsonSerializer.Serialize(LogState.Instance.ModSave, ModSaveConfig.JsonOptions);
            File.WriteAllText(ModSaveConfig.SavePath, saveJson);
        }
        catch (Exception ex)
        {
            ModLogger.Error("Save", $"Failed to save: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存面板位置到存档
    /// </summary>
    /// <param name="isMainPanel">是否为主面板</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    public static void SavePanelPosition(bool isMainPanel, float x, float y)
    {
        if (LogState.Instance.ModSave == null) return;

        if (isMainPanel)
        {
            LogState.Instance.ModSave.MainPanelX = x;
            LogState.Instance.ModSave.MainPanelY = y;
        }
        else
        {
            LogState.Instance.ModSave.DamageStatsPanelX = x;
            LogState.Instance.ModSave.DamageStatsPanelY = y;
        }

        try
        {
            string saveJson = JsonSerializer.Serialize(LogState.Instance.ModSave, ModSaveConfig.JsonOptions);
            File.WriteAllText(ModSaveConfig.SavePath, saveJson);
        }
        catch (Exception ex)
        {
            ModLogger.Error("Save", $"Failed to save panel position: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取面板保存的位置
    /// </summary>
    /// <param name="isMainPanel">是否为主面板</param>
    /// <returns>位置向量，如果未保存则返回 null</returns>
    public static Vector2? GetPanelPosition(bool isMainPanel)
    {
        if (LogState.Instance.ModSave == null) return null;

        float x, y;
        if (isMainPanel)
        {
            x = LogState.Instance.ModSave.MainPanelX;
            y = LogState.Instance.ModSave.MainPanelY;
        }
        else
        {
            x = LogState.Instance.ModSave.DamageStatsPanelX;
            y = LogState.Instance.ModSave.DamageStatsPanelY;
        }

        if (x < 0 || y < 0) return null;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 把存档数据传入 mod 运行数据（加载存档后恢复玩家统计）
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
    /// 把 mod 运行数据传入存档数据（保存前同步最新统计）
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
