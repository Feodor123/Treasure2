using System.Collections.Generic;
using System.Threading;
using TreasureNative.UserInterface;

namespace TreasureNative.GameLogic
{
    public interface IPlayerController
    {
        PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] turns);
        void Initialize(GameActivity activity);
        void PerformAction(PlayerAction action);
    }
}