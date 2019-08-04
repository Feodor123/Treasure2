using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treasure.FBotController
{
    class Map
    {
        private List<MapChunk> chunks = new List<MapChunk>();
        public Map(GameParameters parameters)
        {
            for (int i = 0;i < parameters.Players.Length; i++)
            {
                //chunks.Add(new MapChunk(new KnownTile(TerrainType.Home, i + 1)));
            }
        }
    }
}