﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Treasure
{
    public class BotController : IPlayerController
    {
        private GameParameters gameParameters;

        public BotController(GameParameters gameParameters)
        {
            this.gameParameters = gameParameters;
        }

        public virtual PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] allTurns,GameField field)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return PlayerAction.RandomGo();
        }
        public void PerformAction(PlayerAction action) { }//usual bot don't react on user's clicks
    }
}