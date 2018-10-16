using System.Collections.Generic;
using TreasureNative.GameLogic;

namespace TreasureNative.GameLogic
{
    public class PlayerHelper
    {
        public PlayerParameters parameters;
        public List<TurnInfo> actionHistory = new List<TurnInfo>();
    }
}