using System;
using System.Collections.Generic;
using System.Linq;

namespace Treasure
{
    public class GameField
    {
        private static Dictionary<Direction, Point> directions = new Dictionary<Direction, Point>
        {
            {Direction.Up,new Point(0,-1)},
            {Direction.Right,new Point(1,0)},
            {Direction.Down,new Point(0,1)},
            {Direction.Left,new Point(-1,0)},
        };
        private static Dictionary<Direction, Direction> antiDirections = new Dictionary<Direction, Direction>()
        {
            {Direction.Up,Direction.Down},
            {Direction.Down,Direction.Up},
            {Direction.Right,Direction.Left},
            {Direction.Left,Direction.Right},
        };

        private const int AttemptCount = 100;
        private const int RiverFlow = 2;
        private const double WallGenerationChance = 0.1;

        private Point treasurePos;
        private Nullable<Point> lastRiverTilePos;
        private GameParameters gameParams;
        private Tile[,] tiles;
        private Tile[] river;
        private Tile[] holes;
        private Random rnd;
        private Player currentPlayer;

        public Player[] players;


        public GameField(GameParameters gameParams, Player[] players,Random rnd)
        {
            this.gameParams = gameParams;
            this.players = players;
            this.rnd = rnd;
        }

        public Tile this[int x, int y]
        {
            get => tiles[x, y];
            set => tiles[x, y] = value;
        }

        public Tile this[Point p]
        {
            get => tiles[p.X, p.Y];
            set => tiles[p.X, p.Y] = value;
        }

        public Tuple<Tile[,], Player[]> GetInfo()
        {
            return new Tuple<Tile[,], Player[]>(tiles,players);
        }

        #region Field Generation

        public bool Generate()
        {
            bool acceptable;
            int count = 0;
            do
            {
                count++;
                tiles = new Tile[gameParams.FieldWidth, gameParams.FieldHeight];
                acceptable = GenerateField() &&
                GenerateRiver(rnd) && 
                GenerateSwamps(rnd, gameParams.SwampSize, gameParams.SwampCount) &&
                GenerateHoles(rnd, gameParams.PortalCount) &&
                GenerateHomes(rnd, players) &&
                GenerateWalls(rnd, WallGenerationChance) &&
                GenerateTreasure(rnd) &&
                CondensationCheck();
            }
            while (!acceptable && count <= AttemptCount);
            return acceptable;
        }

        private bool GenerateField()
        {
            for (int x = 0; x < gameParams.FieldWidth; x++)
            {
                for (int y = 0; y < gameParams.FieldHeight; y++)
                {
                    tiles[x, y] = new Tile(this, new Point(x, y), TerrainType.Field);
                }
            }
            return true;
        }

        private bool GenerateRiver(Random rnd)
        {
            List<Tile> riverTiles = new List<Tile>();
            if (gameParams.Through)
            {
                lastRiverTilePos = null;
                for (int i = 0; i < AttemptCount; i++)
                {                    
                    Point p = new Point(rnd.Next(gameParams.FieldWidth), gameParams.FieldHeight - 1);
                    List<Point> river = new List<Point>();
                    bool[,] water = new bool[gameParams.FieldWidth, gameParams.FieldHeight];
                    while (p.Y >= 0)
                    {
                        river.Add(p);
                        water[p.X, p.Y] = true;
                        List<Direction> dir = new List<Direction>() { Direction.Up };
                        if (river.Count == 1 || (river[river.Count - 1] + directions[Direction.Left]).Mod(gameParams.FieldWidth,gameParams.FieldHeight) != river[river.Count - 2])
                        {
                            dir.Add(Direction.Left);
                        }
                        if (river.Count == 1 || (river[river.Count - 1] + directions[Direction.Right]).Mod(gameParams.FieldWidth, gameParams.FieldHeight) != river[river.Count - 2])
                        {
                            dir.Add(Direction.Right);
                        }
                        p += directions[dir[rnd.Next(dir.Count)]];
                        p.X = ((p.X % gameParams.FieldWidth) + gameParams.FieldWidth) % gameParams.FieldWidth;
                    }
                    if (river.First().X != river.Last().X)
                    {
                        continue;
                    }
                    foreach (var r in river)
                    {
                        int count = 0;
                        foreach (var d in directions.Values)
                        {
                            Point rr = (r + d).Mod(gameParams.FieldWidth,gameParams.FieldHeight);
                            if (water[rr.X, rr.Y]) count++;
                        }
                        if (count != 2)
                            goto Cont;
                    }
                    Tile lastTile = null;
                    foreach (var r in river)
                    {
                        lastTile = new Tile(this,r,TerrainType.Water,tileParam: lastTile);
                        this[r] = lastTile;
                        riverTiles.Add(lastTile);
                    }
                    this[river[0]].tileParam = lastTile;
                    this.river = riverTiles.ToArray();
                    return true;
                    Cont:;
                }
            }
            else
            {
                Point p = new Point(rnd.Next(gameParams.FieldWidth), gameParams.FieldHeight - 1);
                lastRiverTilePos = p;
                Tile lastTile = null;
                while (p.Y >= 0)
                {
                    this[p] = new Tile(this, p, TerrainType.Water, tileParam: lastTile);
                    lastTile = this[p];
                    riverTiles.Add(lastTile);
                    List<Direction> dir = new List<Direction>() { Direction.Up };
                    if (p.X != 0 && this[p.X - 1, p.Y].terrainType == TerrainType.Field && (p.Y == gameParams.FieldHeight - 1 || this[p.X - 1, p.Y + 1].terrainType != TerrainType.Water))
                    {
                        dir.Add(Direction.Left);
                    }
                    if (p.X != gameParams.FieldWidth - 1 && this[p.X + 1, p.Y].terrainType == TerrainType.Field && (p.Y == gameParams.FieldHeight - 1 || this[p.X + 1, p.Y + 1].terrainType != TerrainType.Water))
                    {
                        dir.Add(Direction.Right);
                    }
                    p += directions[dir[rnd.Next(dir.Count)]];
                }
                river = riverTiles.ToArray();
                return true;
            }
            return false;
        }

        private bool GenerateSwamp(Random rnd, int size)
        {
            List<Point> swamp = new List<Point>();
            List<Point> candidates = new List<Point>();

            // helper functions follow

            bool HaveSwampsAround(Point p)
            {
                foreach (var d in directions.Values)
                {
                    Point pp = p + d;
                    if (gameParams.Through)
                        pp = pp.Mod(gameParams.FieldWidth, gameParams.FieldHeight);
                    if (HavePoint(pp) && this[pp].terrainType == TerrainType.Swamp)
                        return true;
                }
                return false;
            }

            Point FindPointWithoutSwampsAround()
            {
                int i = 0;
                Point p;
                do
                {
                    p = new Point(rnd.Next(gameParams.FieldWidth), rnd.Next(gameParams.FieldHeight));
                    i++;
                }
                while ((this[p].terrainType != TerrainType.Field || HaveSwampsAround(p)) && i < AttemptCount);

                if (i == AttemptCount)
                    return new Point(-1, -1);
                else
                    return p;
            }

            // finds such field neighbourgs, that does not have swamps around it
            void FindCandidates(Point p)
            {
                foreach (var d in directions.Values)
                {
                    Point neigh = p + d;

                    if (gameParams.Through)
                        neigh = neigh.Mod(gameParams.FieldWidth, gameParams.FieldHeight);

                    if (!HavePoint(neigh))
                        continue;

                    if (this[neigh].terrainType != TerrainType.Field)
                        continue;

                    if (swamp.Contains(neigh) || candidates.Contains(neigh))
                        continue;

                    if (!HaveSwampsAround(neigh))
                        candidates.Add(neigh);
                }
            }

            for (int j = 0;j < AttemptCount;j++)
            {
                swamp.Clear();
                Point p = FindPointWithoutSwampsAround();
                if (p == new Point(-1, -1))
                    continue;
                swamp.Add(p);

                FindCandidates(p);

                for (int i = 1; i < size; i++)
                {
                    if (candidates.Count == 0)
                    {
                        // No more candidates. Try again
                        goto Cont;
                    }

                    // Select next candidate and set it to swamp
                    p = candidates[rnd.Next(candidates.Count)];
                    candidates.Remove(p);
                    swamp.Add(p);

                    // Update candidate list

                    FindCandidates(p);
                }
                
                if (swamp.Count != 3)
                {

                }


                // Apply results
                foreach (var s in swamp)
                    this[s] = new Tile(this, s, TerrainType.Swamp);
                return true;
                Cont:;
            }
            return false;
        }

        /// <summary>
        /// Generates swamps on field
        /// </summary>
        /// <param name="rnd">RNG used to generate swamps</param>
        /// <param name="size">Size of each swamp</param>
        /// <param name="count">Count of swamps</param>
        private bool GenerateSwamps(Random rnd, int size, int count)
        {
            bool b = true;
            for (int k = 0; k < count; k++)
                b &= GenerateSwamp(rnd, size);
            return b;
        }

        private bool GenerateHoles(Random rnd, int count)
        {
            holes = new Tile[count];
            for (int i = 0;i < count; i++)
            {
                Point p;
                int j = 0;
                do
                {
                    p = new Point(rnd.Next(gameParams.FieldWidth), rnd.Next(gameParams.FieldHeight));
                    j++;
                }
                while (this[p].terrainType != TerrainType.Field && j < AttemptCount);
                if (j == AttemptCount)
                    return false;
                this[p] = new Tile(this,p,TerrainType.Hole,i + 1);
                holes[i] = this[p];
            }
            return true;
        }

        private bool GenerateHomes(Random rnd, Player[] players)
        {
            List<Point> homes = new List<Point>();
            for (int i = 0; i < players.Length; i++)
            {
                bool b = false;
                for (int j = 0; j < AttemptCount && !b; j++)
                {
                    b = true;
                    Point p;
                    int k = 0;
                    do
                    {
                        p = new Point(rnd.Next(gameParams.FieldWidth), rnd.Next(gameParams.FieldHeight));
                        k++;
                    }
                    while (this[p].terrainType != TerrainType.Field && k < AttemptCount);
                    if (k == AttemptCount)
                    {
                        b = false;
                        goto exit;
                    }
                    foreach (var h in homes)
                    {
                        if (Math.Min(Math.Abs(h.X - p.X),gameParams.FieldWidth - Math.Abs(h.X - p.X)) + Math.Min(Math.Abs(h.Y - p.Y), gameParams.FieldHeight - Math.Abs(h.Y - p.Y)) <= 2)
                        {
                            b = false;
                            goto exit;
                        }
                    }
                    homes.Add(p);
                    Tile t = new Tile(this, p, TerrainType.Home, i + 1, playerParam: players[i]);
                    this[p] = t;
                    players[i].home = t;
                    players[i].position = p;
                    exit:;
                }
                if (!b)
                    return false;
            }
            return true;
        }

        private bool GenerateWalls(Random rnd, double chance)
        {
            for (int x = 0;x < gameParams.FieldWidth; x++)
            {
                for (int y = 0;y < gameParams.FieldHeight; y++)
                {
                    foreach (var d in new Direction[] { Direction.Up, Direction.Left })
                    {
                        Point p1 = new Point(x, y);
                        Point p2 = (p1 + directions[d]).Mod(gameParams.FieldWidth, gameParams.FieldHeight);
                        BorderType type = BorderType.Empty;
                        if (rnd.NextDouble() < chance && !(this[p1].terrainType == TerrainType.Swamp && this[p2].terrainType == TerrainType.Swamp) && !(this[p1].terrainType == TerrainType.Water && this[p2].terrainType == TerrainType.Water))
                        {
                            type = BorderType.BreakableWall;
                        }
                        this[p1].walls[d] = type;
                        this[p2].walls[antiDirections[d]] = type;
                    }
                }
            }
            if (!gameParams.Through)
            {
                for (int x = 0; x < gameParams.FieldWidth; x++)
                {
                    this[x, 0].walls[Direction.Up] = BorderType.UnbreakableWall;
                    this[x, gameParams.FieldHeight - 1].walls[Direction.Down] = BorderType.UnbreakableWall;
                }
                for (int y = 0; y < gameParams.FieldHeight; y++)
                {
                    this[0, y].walls[Direction.Left] = BorderType.UnbreakableWall;
                    this[gameParams.FieldWidth - 1, y].walls[Direction.Right] = BorderType.UnbreakableWall;
                }
                this[lastRiverTilePos.Value].walls[Direction.Down] = BorderType.Grate;
            }
            return true;
        }

        private bool GenerateTreasure(Random rnd)
        {
            int i = 0;
            Point p;
            do
            {
                p = new Point(rnd.Next(gameParams.FieldWidth), rnd.Next(gameParams.FieldHeight));
                i++;
            }
            while (this[p].terrainType != TerrainType.Field  && i < AttemptCount);
            if (i == AttemptCount)
                return false;
            Stuff treasure = new Stuff(StuffType.Treasure);
            this[p].stuff.Add(treasure);
            treasurePos = p;
            return true;
        }        

        private bool CondensationCheck()
        {
            Dictionary<Point, Node> dict = new Dictionary<Point, Node>();
            dict.Add(treasurePos,new Node(treasurePos,this));
            dict[treasurePos].Go(dict);
            int i = dict[treasurePos].Go2();
            if (i < dict.Count)
                return false;
            if (players.Count(_ => !dict.ContainsKey(_.home.position)) > 0)
                return false;
            return true;
        }

        class Node
        {
            public GameField field;
            public Point point;
            public List<Node> from = new List<Node>();
            public bool b;
            public Node(Point point,GameField field)
            {
                this.point = point;
                this.field = field;
            }
            public void Go(Dictionary<Point, Node> dict)
            {
                foreach (var d in directions.Keys)
                {
                    Point p2 = field.CheckGo(point, d);
                    if (!dict.ContainsKey(p2))
                    {
                        var n = new Node(p2, field);
                        dict.Add(p2, n);
                        n.Go(dict);
                    }
                    dict[p2].from.Add(this);
                }
            }
            public int Go2()
            {
                b = true;
                int i = 1;
                foreach (var n in from)
                    if (!n.b)
                        i += n.Go2();
                return i;
            }
        }

        private Point CheckGo(Point start, Direction direction)
        {
            if (this[start].walls[direction] != BorderType.Empty)
            {
                return start;
            }
            Point p = (start + directions[direction]).Mod(gameParams.FieldWidth,gameParams.FieldHeight);
            Tile t = this[p];
            switch (t.terrainType)
            {
                case TerrainType.Field:
                    return p;
                case TerrainType.Hole:
                    return holes[t.intParam % holes.Length].position;
                case TerrainType.Home:
                    return p;
                case TerrainType.Swamp:
                    return start;
                case TerrainType.Water:
                    for (int i = 0; i < RiverFlow; i++)
                    {
                        if (this[p].LastRiverTile)
                        {
                            return p;
                        }
                        p = this[p].tileParam.position;
                    }
                    return p;
                default:
                    throw new Exception();
            }
        }

        private bool IsWay(Point start, Point finish)
        {
            Queue<Point> check = new Queue<Point>();
            bool[,] canGo = new bool[gameParams.FieldWidth, gameParams.FieldHeight];
            canGo[start.X,start.Y] = true;
            check.Enqueue(start);
            while (check.Count > 0)
            {
                Point p = check.Dequeue();
                foreach (var d in directions.Keys)
                {
                    Point pp = CheckGo(p, d);
                    if (!canGo[pp.X, pp.Y])
                    {
                        canGo[pp.X, pp.Y] = true;
                        check.Enqueue(pp);
                    }
                }
            }
            if (!canGo[finish.X,finish.Y])
            {
                return false;
            }
            return true;
        }

        private bool IsWay(Point start, IEnumerable<Point> finish)
        {
            Queue<Point> check = new Queue<Point>();
            bool[,] canGo = new bool[gameParams.FieldWidth, gameParams.FieldHeight];
            canGo[start.X, start.Y] = true;
            check.Enqueue(start);
            while (check.Count > 0)
            {
                Point p = check.Dequeue();
                foreach (var d in directions.Keys)
                {
                    Point pp = CheckGo(p, d);
                    if (!canGo[pp.X, pp.Y])
                    {
                        canGo[pp.X, pp.Y] = true;
                        check.Enqueue(pp);
                    }
                }
            }
            foreach (var f in finish)
                if (!canGo[f.X, f.Y])
                {
                    return false;
                }
            return true;
        }

        private Tile GetTileByDir(Point p,Direction direction)
        {
            if (gameParams.Through)
                return this[(p + directions[direction]).Mod(gameParams.FieldWidth, gameParams.FieldHeight)];
            else if (p.X >= 0 && p.Y >= 0 && p.X < gameParams.FieldWidth && p.Y < gameParams.FieldHeight)
                return this[p + directions[direction]];
            else
                return null;
        }

        #endregion

        #region Player action handling

        public bool Update(Player player, PlayerAction motion)
        {
            currentPlayer = player;
            bool b;
            switch (motion.type)
            {
                case ActionType.Go:
                    b = Go(player, motion);
                    break;
                case ActionType.Shoot:
                    b = Shoot(player, motion);
                    break;
                default:
                    throw new Exception();
            }
            if (b)
                player.playerHelper.actionHistory.Add(new TurnInfo());
            return b;
        }

        /// <summary>
        /// Performs action - move
        /// Checks all game rules to be followed
        /// </summary>
        /// <param name="player">player, who performs action</param>
        /// <param name="direction">direction of move</param>
        /// <returns>Successfullity of action</returns>
        private bool Go(Player player, PlayerAction action)
        {
            var tf = player.playerHelper.actionHistory[player.playerHelper.actionHistory.Count - 1];
            var direction = action.direction;
            List<TileInfo> tilesInfo = new List<TileInfo>();
            if (this[player.position].walls[direction] != BorderType.Empty)
            {
                if (this[player.position].walls[direction] == BorderType.Grate)
                    tilesInfo.Add(new TileInfo(MoveResult.Grate));
                else
                    tilesInfo.Add(new TileInfo(MoveResult.Wall));
                tf.actions.Add(new ActionInfo(action, tilesInfo));
                return true;
            }
            Point p = (player.position + directions[direction]).Mod(gameParams.FieldWidth,gameParams.FieldHeight);
            Tile t = this[p];

            // move player and check if somebody was there; Kill him if so
            void MovePlayer(Point pos)
            {
                player.position = pos;
                CheckPlayerCollision(player);
                CheckStuff();
            }

            // Checks if player is standing on treasure; If so - take it and add this information to answer
            void CheckStuff()
            {
                tilesInfo.Last().stuff = TakeStuff(player);
            }

            switch (t.terrainType)
            {
                case TerrainType.Field:
                    tilesInfo.Add(new TileInfo(MoveResult.Field));
                    MovePlayer(p);
                    break;
                case TerrainType.Hole:
                    tilesInfo.Add(new TileInfo(MoveResult.Hole, t.intParam));
                    MovePlayer(p);
                    t = holes[t.intParam % holes.Length];
                    tilesInfo.Add(new TileInfo(MoveResult.Hole, t.intParam));
                    MovePlayer(t.position);
                    break;
                case TerrainType.Home:
                    if (t == player.home)
                    {
                        player.UpdateBullets();
                    }
                    tilesInfo.Add(new TileInfo(MoveResult.Home, t.intParam));
                    MovePlayer(p);
                    break;
                case TerrainType.Swamp:
                    tilesInfo.Add(new TileInfo(MoveResult.Swamp));
                    break;
                case TerrainType.Water:
                    tilesInfo.Add(new TileInfo(MoveResult.Water));
                    MovePlayer(p);
                    for (int i = 0; i < RiverFlow; i++)
                    {
                        if (this[p].LastRiverTile)
                        {
                            tilesInfo.Add(new TileInfo(MoveResult.Grate));
                            break;
                        }
                        else
                        {
                            p = this[p].tileParam.position;
                            tilesInfo.Add(new TileInfo(MoveResult.Water));
                            MovePlayer(p);
                        }
                    }
                    break;
                default:
                    throw new Exception();
            }
            tf.actions.Add(new ActionInfo(action, tilesInfo));
            return true;
        }       

        private bool Shoot(Player player, PlayerAction action)
        {
            var tf = player.playerHelper.actionHistory[player.playerHelper.actionHistory.Count - 1];
            var direction = action.direction;
            if (player.bulletCount == 0)
                return false;
            player.bulletCount--;
            if (player.position == player.home.position)
                player.UpdateBullets();
            action.intParam = player.bulletCount;//bullet count reported for moment BEFORE self-killing
            Point p = player.position;
            for (int i = 0; true; i++)
            {
                if (i != 0 && players.Count(_ => _.position == p) > 0)
                {
                    KillPlayer(players.First(_ => _.position == p));
                    break;
                }
                if (this[p].walls[direction] != BorderType.Empty)
                {
                    BreakWall(p, direction);
                    break;
                }
                p = (p + directions[direction]).Mod(gameParams.FieldWidth, gameParams.FieldHeight);
            }

            if (player.position == player.home.position)//again, if we was killed
                player.UpdateBullets();

            tf.actions.Add(new ActionInfo(action,new List<TileInfo>()));
            return true;
        }

        public Player CheckWin()
        {
            foreach (var p in players)
                if (p.position == p.home.position && p.stuff.Any(_ => _.type == StuffType.Treasure))
                    return p;
            return null;
        }

        private bool BreakWall(Point p, Direction direction)
        {
            if (this[p].walls[direction] == BorderType.UnbreakableWall)
                return false;
            Point pp = (p + directions[direction]).Mod(gameParams.FieldWidth,gameParams.FieldHeight);
            this[p].walls[direction] = BorderType.Empty;
            this[pp].walls[antiDirections[direction]] = BorderType.Empty;
            return true;
        }

        private List<Stuff> TakeStuff(Player player)
        {
            List<Stuff> stuff = this[player.position].stuff.ToList();
            this[player.position].stuff = new List<Stuff>();
            foreach (var st in stuff)
            {
                Stuff toAddCount = player.stuff.Find(_ => _.type == st.type);
                if (toAddCount == null)
                    player.stuff.Add(st);
                else
                    toAddCount.count += st.count;
            }
            return stuff;
        }

        private void CheckPlayerCollision(Player player)
        {
            var coll = players.Where(_ => _.position == player.position && _ != player);
            if (coll.Count() > 0)
                KillPlayer(coll.Single());
        }

        private void KillPlayer(Player player)
        {
            player.UpdateBullets();
            this[player.position].DropStuff(player.stuff);
            player.stuff.Clear();
            player.position = player.home.position;
            CheckPlayerCollision(player);
            player.playerHelper.actionHistory.Last().actions.Add(ActionInfo.DieAction(new TileInfo(MoveResult.Home, TakeStuff(player))));
        }

        private bool HavePoint(Point p)
        {
            return p.X >= 0 && p.X < gameParams.FieldWidth && p.Y >= 0 && p.Y < gameParams.FieldHeight;
        }      

        private bool Parallel(Direction dir,Orientation or)
        {
            return ((or == Orientation.Horisontal && (dir == Direction.Left || dir == Direction.Right)) || (or == Orientation.Vertical && (dir == Direction.Up || dir == Direction.Down)));
        }

        #endregion
    }
}