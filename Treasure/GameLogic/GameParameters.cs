using System;

namespace Treasure
{
    public class GameParameters
    {
        public int FieldWidth { get; set; } = 10;
        public int FieldHeight { get; set; } = 10;
        public int PortalCount { get; set; } = 3;
        public int SwampCount { get; set; } = 4;
        public int SwampSize { get; set; } = 3;
        public bool Through { get; set; } = false;
        public int PlayerCount { get; set; } = 2;
    }
}