using System.Collections.Generic;
using System.Threading;

namespace Treasure
{
    public interface IPlayerController
    {
        PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] turns, GameField field);
        void PerformAction(PlayerAction action);
    }
}