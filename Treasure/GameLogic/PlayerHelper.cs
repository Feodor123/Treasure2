using System.Collections.Generic;

namespace Treasure
{
    /// <summary>
    /// Players parameters + his turns info
    /// </summary>
    public class PlayerHelper
    {
        public PlayerHelper(PlayerParameters parameters)
        {
            this.parameters = parameters;
        }
        public PlayerParameters parameters;
        public List<TurnInfo> actionHistory = new List<TurnInfo>() {new TurnInfo() };
    }
}