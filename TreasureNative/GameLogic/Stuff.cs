using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureNative.GameLogic
{
    public class Stuff
    {
        public StuffType type;
        public int count;
        public State state;

        public Stuff(StuffType type, int count = 1, State state = State.None)
        {
            this.type = type;
            this.count = count;
        }
    }
}