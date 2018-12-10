using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treasure.FBotController
{
    /*
    class MapPlayer
    {
        KnownTile tile;
        KnownTile home;
        public MapPlayer(KnownTile home)
        {
            this.home = home;
            tile = home;
        }
        public void Update(TurnInfo turnInfo)
        {
            if (turnInfo.wasKilled)
            {
                tile = home;
            }
            Direction dir = turnInfo.playerAction.direction;
            MapChunk chunk = tile.mapChunk;
            Point p = chunk.knownPoints[tile] + GameField.directions[dir];
            switch (turnInfo.tileInfo[0].moveResult)
            {
                case MoveResult.Field:
                    chunk.AddTile(p,new KnownTile(TerrainType.Field,chunk));
                    tile.borders[dir] = BorderType.Empty;
                    chunk.knownTiles[p].borders[GameField.antiDirections[dir]] = BorderType.Empty;
                    break;
                case MoveResult.Grate:
                    if (dir != Direction.Down)
                        throw new Exception();
                    tile.borders[Direction.Down] = BorderType.UnbreakableWall;
                    break;
                case MoveResult.Hole:
                    chunk.AddTile(p, new KnownTile(TerrainType.Hole, chunk,turnInfo.tileInfo[0].intParam));
                    tile.borders[dir] = BorderType.Empty;
                    chunk.knownTiles[p].borders[GameField.antiDirections[dir]] = BorderType.Empty;
                    break;
                case MoveResult.Home:
                    break;
                case MoveResult.Shoot:
                    break;
                case MoveResult.Swamp:
                    break;
                case MoveResult.Wall:
                    break;
                case MoveResult.Water:
                    break;
            }
        }
    }*/
}