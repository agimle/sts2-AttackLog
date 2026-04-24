using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

public class PanelSortController
{
    private readonly LogState _state;
    private bool _sortByTotal = true;

    public bool SortByTotal => _sortByTotal;

    public PanelSortController(LogState state)
    {
        _state = state;
    }

    public void Toggle()
    {
        _sortByTotal = !_sortByTotal;
    }

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

    public string GetSortLabel()
    {
        return _sortByTotal ? "总伤害" : "本层伤害";
    }
}
