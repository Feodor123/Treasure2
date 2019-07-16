using System.Collections.Generic;

namespace Treasure
{
    public class PlayerHelper
    {
        public PlayerParameters parameters;
        public List<TurnInfo> actionHistory = new List<TurnInfo>() {new TurnInfo() };
    }
}