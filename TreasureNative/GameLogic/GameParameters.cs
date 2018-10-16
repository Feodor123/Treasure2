using System;

namespace TreasureNative.GameLogic
{
    public class GameParameters
    {
        public int FieldWidth { get; set; }
        public int FieldHeight { get; set; }
        public int PortalCount { get; set; }
        public int SwampCount { get; set; }
        public int SwampSize { get; set; }
        public PlayerHelper[] Players { get; set; }

        public void Dispose() { }

        public IntPtr Handle => throw new NotImplementedException();
    }
}