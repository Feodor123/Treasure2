using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treasure
{
    public class Stuff
    {
        public StuffType type;
        public int count;

        public Stuff(StuffType type, int count = 1)
        {
            this.type = type;
            this.count = count;
        }
    }
}