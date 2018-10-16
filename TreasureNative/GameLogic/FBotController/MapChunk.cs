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
    class MapChunk
    {
        public static readonly Direction[] directions = new Direction[] {Direction.Up,Direction.Left,Direction.Down,Direction.Right};
        public Dictionary<Point, KnownTile> knownTiles { get; }
        public Dictionary<KnownTile, Point> knownPoints { get; }
        public Point position = new Point(-1, -1);
        public int minX, minY, maxX, maxY = 0;

        public int xSize
        {
            get => maxX - minX + 1;
        }

        public int ySize
        {
            get => maxY - minY + 1;
        }

        public MapChunk()
        {
            knownTiles = new Dictionary<Point, KnownTile>();
            knownPoints = new Dictionary<KnownTile, Point>();
        }

        public void SetStartTile(KnownTile startTile)
        {
            AddTile(new Point(0, 0), startTile);
        }

        public void AddTile(Point point, KnownTile tile)
        {
            if (!knownTiles.ContainsKey(point))
            {
                knownTiles.Add(point, tile);
                knownPoints.Add(tile, point);
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        public Point Merge(Point myPoint,Point hisPoint,MapChunk mapChunk)
        {
            Point adding = myPoint - hisPoint;
            foreach (var pair in mapChunk.knownTiles)
            {
                Point p = pair.Key + adding;
                if (knownTiles.ContainsKey(p))
                {
                    knownTiles[p].Merge(pair.Value);
                }
                else
                {
                    AddTile(p, pair.Value);
                    pair.Value.mapChunk = this;
                }
            }
            return adding;
        }
    }   
}