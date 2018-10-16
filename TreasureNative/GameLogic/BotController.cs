using System;
using System.Collections.Generic;
using System.Threading;
using TreasureNative.UserInterface;

namespace TreasureNative.GameLogic
{
    public class BotController : IPlayerController
    {
        private GameParameters gameParameters;
        private GameActivity activity;

        public BotController(GameParameters gameParameters,GameActivity activity)
        {
            this.gameParameters = gameParameters;
            this.activity = activity;
        }

        public void UseResult()
        {
            activity.RunOnUiThread(() => activity.UseResult());
        }

        public virtual PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] allTurns)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(100);
            return PlayerAction.RandomGo();
        }
        public void PerformAction(PlayerAction action) { }//usual bot don't react on user's clicks

        public void Initialize(GameActivity activity) { }
    }
}