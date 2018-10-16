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
using TreasureNative.GameLogic;

namespace TreasureNative.GameLogic.FBotController
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