using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TreasureNative.GameLogic
{
    public class PlayerAction
    {
        private static Random _rnd = new Random();
        public PlayerAction(Action action, Direction direction, int intParam = -1)
        {
            this.action = action;
            this.direction = direction;
            this.intParam = intParam;
        }
        public Action action;
        public Direction direction;
        public int intParam;

        public override string ToString()
        {
            return $"{(action == Action.Go ? "Go" : "Shoot")} {direction}";
        }
        public static PlayerAction RandomGo()
        {
            return new PlayerAction(Action.Go, (Direction)_rnd.Next(4));
        }
    }
}