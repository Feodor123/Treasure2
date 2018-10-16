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
    class KnownTile
    {
        public MapChunk mapChunk;
        public int intParam;
        public TerrainType terrain;
        public Dictionary<Direction, BorderType> borders;
        public KnownTile(TerrainType terrain, MapChunk mapChunk, int intParam = -1)
        {
            this.mapChunk = mapChunk;
            this.intParam = intParam;
            this.terrain = terrain;
            borders = new Dictionary<Direction, BorderType>()
            {
                {Direction.Up,BorderType.Unknown},
                {Direction.Right,BorderType.Unknown},
                {Direction.Left,BorderType.Unknown},
                {Direction.Down,BorderType.Unknown},
            };
        }
        public void Merge(KnownTile tile)
        {
            if (tile.terrain != TerrainType.Unknown && terrain != TerrainType.Unknown && terrain != tile.terrain)
            {
                throw new Exception();
            }
            if (terrain == TerrainType.Unknown)
            {
                terrain = tile.terrain;
            }
            foreach (var dir in MapChunk.directions)
            {
                borders[dir] = MergeBorders(new BorderType[] { borders[dir], tile.borders[dir] });
            }
        }
        public BorderType MergeBorders(BorderType[] choice)
        {
            if (choice.Contains(BorderType.UnbreakableWall) && choice.Contains(BorderType.Empty))
            {
                throw new Exception();
            }
            if (choice.Contains(BorderType.UnbreakableWall))
            {
                return BorderType.UnbreakableWall;
            }
            if (choice.Contains(BorderType.Empty))
            {
                return BorderType.Empty;
            }
            if (choice.Contains(BorderType.ShootedWall) && choice.Contains(BorderType.BreakableWall))
            {
                return BorderType.Empty;
            }
            if (choice.Contains(BorderType.ShootedWall))
            {
                return BorderType.ShootedWall;
            }
            if (choice.Contains(BorderType.BreakableWall))
            {
                return BorderType.BreakableWall;
            }
            if (choice.Contains(BorderType.Wall))
            {
                return BorderType.Wall;
            }
            return BorderType.Unknown;
        }
    }
}