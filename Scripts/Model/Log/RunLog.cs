using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Model;

public class RunLog
{
    private readonly IRunState _runState;
    private readonly Dictionary<ulong, PlayerData> _netIdToPlayers = new();

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
    }

    public PlayerData? GetPlayerByNetId(ulong netId)
    {
        return _netIdToPlayers.TryGetValue(netId, out var data) ? data : null;
    }

    public PlayerData? GetPlayerByCreature(Creature creature)
    {
        foreach (var playerData in _netIdToPlayers.Values)
        {
            if (ReferenceEquals(playerData.PlayerInfo.Creature, creature))
                return playerData;
        }
        return null;
    }

    public IReadOnlyCollection<PlayerData> GetAllPlayers()
    {
        return _netIdToPlayers.Values;
    }
}
