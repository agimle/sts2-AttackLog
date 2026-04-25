using AttackLog.Model;
using AttackLog.Save;

namespace AttackLog.State;

/// <summary>
/// Run 级状态，管理 Run 生命周期内的日志和存档数据
/// </summary>
public class AttackLogRunState
{
    /// <summary>当前 Run 的日志，Run 结束时置空</summary>
    public RunLog? RunLog { get; set; }

    /// <summary>Mod 存档数据</summary>
    public ModSave? ModSave { get; set; }

    /// <summary>
    /// 重置所有 Run 级状态
    /// </summary>
    public void Reset()
    {
        RunLog = null;
        ModSave = null;
    }
}
