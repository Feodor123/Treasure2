using System.Collections.Generic;

namespace Treasure
{
    public class Tile
    {
        public Dictionary<Direction, BorderType> walls = new Dictionary<Direction, BorderType>();
        public TerrainType terrainType;
        public Orientation orientation;
        public Point position;
        public int intParam;
        public Tile tileParam;
        public Player playerParam;
        public List<Stuff> stuff = new List<Stuff>();
        private GameField gameField;

        public Tile(GameField gameField, Point position, TerrainType terrainType, int intParam = 0, Tile tileParam = null, Player playerParam = null, Orientation orientation = Orientation.None)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.gameField = gameField;
            this.intParam = intParam;
            this.tileParam = tileParam;
            this.playerParam = playerParam;
            this.orientation = orientation;
        }

        public bool LastRiverTile
        {
            get
            {
                return terrainType == TerrainType.Water && tileParam == null;
            }
        }

        public void DropStuff(List<Stuff> drop)
        {
            foreach (var st in drop)
            {
                Stuff toAddCount = stuff.Find(_ => _.type == st.type);
                if (toAddCount == null)
                    stuff.Add(st);
                else
                    toAddCount.count += st.count;
            }
        }
    }
}