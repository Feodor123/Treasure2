using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treasure.FBotController
{
    class FBotController : BotController
    {
        public FBotController(GameParameters gameParameters) : base(gameParameters) { }
        public override PlayerAction GetAction(System.Threading.CancellationToken cancellationToken, List<TurnInfo>[] allTurns)
        {
            return base.GetAction(cancellationToken, allTurns);
        }
    }
}