using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treasure
{
    public class PlayerAction
    {
        private static Random _rnd = new Random();
        public PlayerAction(ActionType action, Direction direction, int intParam = -1)
        {
            this.type = action;
            this.direction = direction;
            this.intParam = intParam;
        }
        public ActionType type;
        public Direction direction;
        public int intParam;

        public override string ToString()
        {
            return $"{(type == ActionType.Go ? "Go" : "Shoot")} {direction}";
        }
        public static PlayerAction RandomGo()
        {
            return new PlayerAction(ActionType.Go, (Direction)_rnd.Next(4));
        }
    }
}