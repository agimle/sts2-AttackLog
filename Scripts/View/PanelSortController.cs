using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

/// <summary>
/// 面板排序控制器，支持按总伤害或房间伤害降序排列玩家。
/// 通过 Toggle 切换排序维度
/// </summary>
public class PanelSortController
{
    /// <summary>LogState 实例</summary>
    private readonly LogState _state;

    /// <summary>是否按总伤害排序（否则按房间伤害）</summary>
    private bool _sortByTotal = true;

    /// <summary>当前是否按总伤害排序</summary>
    public bool SortByTotal => _sortByTotal;

    public PanelSortController(LogState state)
    {
        _state = state;
    }

    /// <summary>
    /// 切换排序维度（总伤害 ↔ 房间伤害）
    /// </summary>
    public void Toggle()
    {
        _sortByTotal = !_sortByTotal;
    }

    /// <summary>
    /// 获取排序后的玩家统计数据列表
    /// </summary>
    /// <returns>按伤害降序排列的玩家统计列表</returns>
    public List<KeyValuePair<PlayerInfo, CachedPlayerStats>> GetSortedPlayers()
    {
        var allStats = _state.GetAllCachedStats();
        var sorted = allStats.ToList();

        if (_sortByTotal)
        {
            sorted.Sort((a, b) =>
            {
                int damageA = a.Value.RunLog?.RealDamageDealt ?? 0;
                int damageB = b.Value.RunLog?.RealDamageDealt ?? 0;
                return damageB.CompareTo(damageA);
            });
        }
        else
        {
            sorted.Sort((a, b) =>
            {
                int damageA = a.Value.RoomLog?.RealDamageDealt ?? 0;
                int damageB = b.Value.RoomLog?.RealDamageDealt ?? 0;
                return damageB.CompareTo(damageA);
            });
        }

        return sorted;
    }

    /// <summary>
    /// 获取当前排序维度的显示标签
    /// </summary>
    /// <returns>排序标签文本</returns>
    public string GetSortLabel()
    {
        return _sortByTotal ? "总伤害" : "本层伤害";
    }
}
