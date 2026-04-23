using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Model;

public class RunLog
{
    private readonly IRunState _runState;
    private readonly Dictionary<ulong, PlayerData> _netIdToPlayers = new();
    private readonly Dictionary<Creature, PlayerData> _creaturesToPlayers = new();

    public RunLog(IRunState runState)
    {
        _runState = runState;
    }

    public bool IsSameRun(IRunState runState)
    {
        return _runState == runState;
    }

    public void RegisterPlayer(Player player)
    {
        var playerData = new PlayerData(player);
        _netIdToPlayers.TryAdd(player.NetId, playerData);
        _creaturesToPlayers.TryAdd(player.Creature, playerData);
    }

    public PlayerData? GetPlayerByNetId(ulong netId)
    {
        return _netIdToPlayers.TryGetValue(netId, out var data) ? data : null;
    }

    public PlayerData? GetPlayerByCreature(Creature creature)
    {
        return _creaturesToPlayers.TryGetValue(creature, out var data) ? data : null;
    }

    public IReadOnlyCollection<PlayerData> GetAllPlayers()
    {
        return _netIdToPlayers.Values;
    }
}
