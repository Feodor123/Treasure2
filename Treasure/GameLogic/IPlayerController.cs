using System.Collections.Generic;
using System.Threading;

namespace Treasure
{
    public interface IPlayerController
    {
        PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] turns);
        void PerformAction(PlayerAction action);
    }
}