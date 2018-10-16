using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TreasureNative.UserInterface;

namespace TreasureNative.GameLogic.FBotController
{
    class FBotController : BotController
    {
        public FBotController(GameParameters gameParameters, GameActivity activity) : base(gameParameters, activity) { }
        public override PlayerAction GetAction(System.Threading.CancellationToken cancellationToken, List<TurnInfo>[] allTurns)
        {
            return base.GetAction(cancellationToken, allTurns);
        }
    }
}