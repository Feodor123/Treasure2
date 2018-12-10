using System;
using System.Collections.Generic;
using System.Linq;

namespace Treasure
{
    public class GameField
    {
        private const int AttemptCount = 100;
        private const int RiverFlow = 2;
        private const double WallGenerationChance = 0.1;
        public static Dictionary<Direction, Point> directions = new Dictionary<Direction, Point>
        {
            {Direction.Up,new Point(0,-1)},
            {Direction.Right,new Point(1,0)},
            {Direction.Down,new Point(0,1)},
            {Direction.Left,new Point(-1,0)},
        };
        public static Dictionary<Direction, Direction> antiDirections = new Dictionary<Direction, Direction>()
        {
            {Direction.Up,Direction.Down},
            {Direction.Down,Direction.Up},
            {Direction.Right,Direction.Left},
            {Direction.Left,Direction.Right},
        };
        private Point treasurePos;
        private Nullable<Point> lastRiverTilePos;
        private int portalCount;
        private int swampSize;
        private int swampCount;
        private int bridgeCount;
        private int width;
        private int height;
        private Tile[,] tiles;
        private Tile[] river;
        private Tile[] holes;
        private bool through;
        private Random rnd;
        public Player[] players;
        public GameField(int width, int height, int portalCount, int swampCount, int swampSize, int bridgeCount, bool through, Player[] players,Random rnd)
        {
            this.swampCount = swampCount;
            this.swampSize = swampSize;
            this.portalCount = portalCount;
            this.bridgeCount = bridgeCount;
            this.width = width;
            this.height = height;
            this.players = players;
            this.through = through;
            this.rnd = rnd;
        }

        private Tile this[int x, int y]
        {
            get => tiles[x, y];
            set => tiles[x, y] = value;
        }

        private Tile this[Point p]
        {
            get => tiles[p.X, p.Y];
            set => tiles[p.X, p.Y] = value;
        }

        #region Field Generation

        public bool Generate()
        {
            bool acceptable;
            int count = 0;
            do
            {
                count++;
                tiles = new Tile[width, height];
                acceptable = GenerateField() &&
                GenerateRiver(rnd) &&
                GenerateBridge(bridgeCount) && 
                GenerateSwamps(rnd, swampSize, swampCount) &&
                GenerateHoles(rnd, portalCount) &&
                GenerateHomes(rnd, players) &&
                GenerateWalls(rnd, WallGenerationChance) &&
                GenerateTreasure(rnd) &&
                CheckHoles() &&
                CheckRiver() &&
                CheckTreasure();
            }
            while (!acceptable && count <= AttemptCount);
            return acceptable;
        }

        private bool GenerateField()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = new Tile(this, new Point(x, y), TerrainType.Field);
                }
            }
            return true;
        }

        private bool GenerateRiver(Random rnd)
        {
            List<Tile> riverTiles = new List<Tile>();
            if (through)
            {
                lastRiverTilePos = null;
                for (int i = 0; i < AttemptCount; i++)
                {                    
                    Point p = new Point(rnd.Next(width), height - 1);
                    List<Point> river = new List<Point>();
                    bool[,] water = new bool[width, height];
                    while (p.Y >= 0)
                    {
                        river.Add(p);
                        water[p.X, p.Y] = true;
                        List<Direction> dir = new List<Direction>() { Direction.Up };
                        if (river.Count == 1 || (river[river.Count - 1] + directions[Direction.Left]).Mod(width,height) != river[river.Count - 2])
                        {
                            dir.Add(Direction.Left);
                        }
                        if (river.Count == 1 || (river[river.Count - 1] + directions[Direction.Right]).Mod(width, height) != river[river.Count - 2])
                        {
                            dir.Add(Direction.Right);
                        }
                        p += directions[dir[rnd.Next(dir.Count)]];
                        p.X = ((p.X % width) + width) % width;
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
                            Point rr = (r + d).Mod(width,height);
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
                Point p = new Point(rnd.Next(width), height - 1);
                lastRiverTilePos = p;
                Tile lastTile = null;
                while (p.Y >= 0)
                {
                    this[p] = new Tile(this, p, TerrainType.Water, tileParam: lastTile);
                    lastTile = this[p];
                    riverTiles.Add(lastTile);
                    List<Direction> dir = new List<Direction>() { Direction.Up };
                    if (p.X != 0 && this[p.X - 1, p.Y].terrainType == TerrainType.Field && (p.Y == height - 1 || this[p.X - 1, p.Y + 1].terrainType != TerrainType.Water))
                    {
                        dir.Add(Direction.Left);
                    }
                    if (p.X != width - 1 && this[p.X + 1, p.Y].terrainType == TerrainType.Field && (p.Y == height - 1 || this[p.X + 1, p.Y + 1].terrainType != TerrainType.Water))
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
                    if (through)
                        pp = pp.Mod(width, height);
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
                    p = new Point(rnd.Next(width), rnd.Next(height));
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

                    if (through)
                        neigh = neigh.Mod(width, height);

                    if (!HavePoint(neigh))
                        continue;

                    if (this[neigh].terrainType != TerrainType.Field)
                        continue;

                    if (swamp.Contains(neigh) || candidates.Contains(neigh))
                        continue;

                    bool isGood = true;
                    foreach (var dd in directions.Values)
                    {
                        Point neighOfNeigh = neigh + dd;
                        if (through)
                            neighOfNeigh = neighOfNeigh.Mod(width, height);
                        if (HavePoint(neighOfNeigh) && this[neighOfNeigh].terrainType == TerrainType.Swamp)
                            isGood = false;
                    }
                    if (isGood)
                        candidates.Add(neigh);
                }
            }

            for (int j = 0;j < AttemptCount;j++)
            {
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
                    p = new Point(rnd.Next(width), rnd.Next(height));
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
                        p = new Point(rnd.Next(width), rnd.Next(height));
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
                        if (Math.Min(Math.Abs(h.X - p.X),width - Math.Abs(h.X - p.X)) + Math.Min(Math.Abs(h.Y - p.Y), height - Math.Abs(h.Y - p.Y)) <= 2)
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
            for (int x = 0;x < width; x++)
            {
                for (int y = 0;y < height; y++)
                {
                    foreach (var d in new Direction[] { Direction.Up, Direction.Left })
                    {
                        Point p1 = new Point(x, y);
                        Point p2 = (p1 + directions[d]).Mod(width, height);
                        BorderType type = BorderType.Empty;
                        if (rnd.NextDouble() < chance && !(this[p1].terrainType == TerrainType.Swamp && this[p2].terrainType == TerrainType.Swamp) && !((this[p1].terrainType == TerrainType.Water || this[p1].terrainType == TerrainType.Bridge) && (this[p2].terrainType == TerrainType.Water || this[p2].terrainType == TerrainType.Bridge)))
                        {
                            type = BorderType.BreakableWall;
                        }
                        this[p1].walls[d] = type;
                        this[p2].walls[antiDirections[d]] = type;
                    }
                }
            }
            if (!through)
            {
                for (int x = 0; x < width; x++)
                {
                    this[x, 0].walls[Direction.Up] = BorderType.UnbreakableWall;
                    this[x, height - 1].walls[Direction.Down] = BorderType.UnbreakableWall;
                }
                for (int y = 0; y < height; y++)
                {
                    this[0, y].walls[Direction.Left] = BorderType.UnbreakableWall;
                    this[width - 1, y].walls[Direction.Right] = BorderType.UnbreakableWall;
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
                p = new Point(rnd.Next(width), rnd.Next(height));
                i++;
            }
            while (this[p].terrainType != TerrainType.Field && this[p].terrainType != TerrainType.Bridge && i < AttemptCount);
            if (i == AttemptCount)
                return false;
            Stuff treasure = new Stuff(StuffType.Treasure);
            if (this[p].terrainType == TerrainType.Bridge)
                treasure.state = State.OnBridge;
            this[p].stuff.Add(treasure);
            treasurePos = p;
            return true;
        }

        private bool GenerateBridge(int count)
        {
            List<Tile> riverList = river.ToList();
            for (int i = 0; i < bridgeCount; i++)
            {
                var v = GetRiverTile(riverList);
                if (v == null)
                    return false;
                buildBridge(v);
            }
            return true;

            Tuple<Point, Orientation> GetRiverTile(List<Tile> river)
            {
                while (river.Count > 0)
                {
                    Tile t = river[rnd.Next(river.Count)];
                    river.Remove(t);
                    if ((GetTileByDir(t.position, Direction.Up).terrainType == TerrainType.Water || GetTileByDir(t.position, Direction.Up).terrainType == TerrainType.Bridge)
                        && (GetTileByDir(t.position, Direction.Down).terrainType == TerrainType.Water || GetTileByDir(t.position, Direction.Down).terrainType == TerrainType.Bridge))
                    {
                        return new Tuple<Point, Orientation>(t.position, Orientation.Horisontal);
                    }
                    if ((GetTileByDir(t.position, Direction.Left).terrainType == TerrainType.Water || GetTileByDir(t.position, Direction.Left).terrainType == TerrainType.Bridge)
                        && (GetTileByDir(t.position, Direction.Right).terrainType == TerrainType.Water || GetTileByDir(t.position, Direction.Right).terrainType == TerrainType.Bridge))
                    {
                        return new Tuple<Point, Orientation>(t.position, Orientation.Vertical);
                    }
                }
                return null;
            }

            void buildBridge(Tuple<Point, Orientation> tuple)
            {
                Tile tile = this[tuple.Item1];
                tile.terrainType = TerrainType.Bridge;
                tile.orientation = tuple.Item2;
            }
        }

        private Point CheckGo(Point start, Direction direction)//не учитывает "верха" мостов, так как если с него спрыгнуть, то будет аналогично реке, а полагаться на него нельзя, так как любой пироман может его сломать (ну, не всегда...)
        {
            if (this[start].walls[direction] != BorderType.Empty || (this[start].terrainType == TerrainType.Bridge && Parallel(direction, this[start].orientation)))
            {
                return start;
            }
            Point p = (start + directions[direction]).Mod(width,height);
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
                case TerrainType.Bridge:
                    for (int i = 0; i < 2; i++)
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

        private bool CheckRiver()
        {
            if (through)
                return true;
            return IsWay(lastRiverTilePos.Value, players.Select(_ => _.home.position));
        }

        private bool CheckTreasure()
        {
            return IsWay(treasurePos, players.Select(_ => _.home.position));
        }

        private bool CheckHoles()
        {
            foreach (var h in holes)
                if (!IsWay(h.position, players.Select(_ => _.home.position)))
                    return false;
            return true;
        }

        private bool IsWay(Point start, Point finish)
        {
            Queue<Point> check = new Queue<Point>();
            bool[,] canGo = new bool[width, height];
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
            bool[,] canGo = new bool[width, height];
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
            if (through)
                return this[(p + directions[direction]).Mod(width, height)];
            else if (p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height)
                return this[p + directions[direction]];
            else
                return null;
        }

        #endregion

        #region Player action handling

        public TurnInfo Update(Player player, PlayerAction motion)
        {
            TurnInfo tf;
            switch (motion.action)
            {
                case Action.Go:
                    tf = Go(player, motion);
                    break;
                case Action.Shoot:
                    tf = Shoot(player, motion);
                    break;
                default:
                    throw new Exception();
            }
            player.actions = new List<PlayerAction>();
            return tf;
        }

        /// <summary>
        /// Performs action - move
        /// Checks all game rules to be followed
        /// </summary>
        /// <param name="player">player, who performs action</param>
        /// <param name="direction">direction of move</param>
        /// <returns>Result of action</returns>
        private TurnInfo Go(Player player, PlayerAction action)
        {
            player.actions.Add(action);
            var direction = action.direction;
            List<TileInfo> tilesInfo = new List<TileInfo>();
            if (this[player.position].walls[direction] != BorderType.Empty || ((player.state & State.OnBridge) == 0 && this[player.position].terrainType == TerrainType.Bridge && Parallel(direction, this[player.position].orientation)))
            {
                if (this[player.position].walls[direction] == BorderType.Grate)
                    tilesInfo.Add(new TileInfo(MoveResult.Grate));
                else
                    tilesInfo.Add(new TileInfo(MoveResult.Wall));               
                return new TurnInfo(player.actions, tilesInfo);
            }
            Point p = (player.position + directions[direction]).Mod(width,height);
            Tile t = this[p];

            // move player and check if somebody was there; Kill him if so
            void MovePlayer(Point pos,State state)
            {
                player.position = pos;
                player.state = state;
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
                    MovePlayer(p,State.None);
                    break;
                case TerrainType.Hole:
                    tilesInfo.Add(new TileInfo(MoveResult.Hole, t.intParam));
                    MovePlayer(p, State.None);
                    t = holes[t.intParam % holes.Length];
                    tilesInfo.Add(new TileInfo(MoveResult.Hole, t.intParam));
                    MovePlayer(t.position, State.None);
                    break;
                case TerrainType.Home:
                    if (t == player.home)
                    {
                        player.UpdateBullets();
                    }
                    tilesInfo.Add(new TileInfo(MoveResult.Home, t.intParam));
                    MovePlayer(p, State.None);
                    break;
                case TerrainType.Swamp:
                    tilesInfo.Add(new TileInfo(MoveResult.Swamp));
                    break;
                case TerrainType.Water:
                case TerrainType tt when (tt == TerrainType.Bridge && !Parallel(direction, t.orientation)):
                    tilesInfo.Add(new TileInfo(MoveResult.Water));
                    MovePlayer(p, State.None);
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
                            MovePlayer(p, State.None);
                        }
                    }
                    break;
                case TerrainType tt when (tt == TerrainType.Bridge && Parallel(direction, t.orientation)):
                    tilesInfo.Add(new TileInfo(MoveResult.Field));
                    MovePlayer(p, State.OnBridge);
                    player.state |= State.OnBridge;
                    break;
                default:
                    throw new Exception();
            }
            return new TurnInfo(player.actions, tilesInfo);
        }       

        private TurnInfo Shoot(Player player, PlayerAction action)
        {
            player.actions.Add(action);
            var direction = action.direction;
            if (player.bulletCount == 0)
                return null;
            player.bulletCount--;
            if (player.position == player.home.position)
                player.UpdateBullets();
            action.intParam = player.bulletCount;//bullet count reported for moment BEFORE self-killing
            Point p = player.position;
            if (this[p].terrainType == TerrainType.Bridge && (player.state & State.OnBridge) == 0 && Parallel(direction, this[p].orientation))
            {
                BreakBridge(p);
            }
            else
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
                    p = (p + directions[direction]).Mod(width, height);
                }

            if (player.position == player.home.position)//again, if we was killed
                player.UpdateBullets();

            return new TurnInfo(player.actions,new List<TileInfo>());
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
            Point pp = (p + directions[direction]).Mod(width,height);
            this[p].walls[direction] = BorderType.Empty;
            this[pp].walls[antiDirections[direction]] = BorderType.Empty;
            return true;
        }

        private List<Stuff> TakeStuff(Player player)
        {
            List<Stuff> stuff = this[player.position].stuff.Where(_ => _.state == player.state).ToList();
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
            var coll = players.Where(_ => _.position == player.position && _ != player && _.state == player.state);
            if (coll.Count() > 0)
                KillPlayer(coll.Single());
        }

        private void KillPlayer(Player player)
        {
            player.actions.Add(new PlayerAction(Action.Die,Direction.None));
            player.UpdateBullets();
            this[player.position].DropStuff(player.stuff,player.state);
            player.stuff.Clear();
            player.position = player.home.position;
            CheckPlayerCollision(player);
            TakeStuff(player);
        }

        private bool HavePoint(Point p)
        {
            return p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;
        }

        private void BreakBridge(Point p)
        {
            foreach (var pl in players)
                if (pl.position == p)
                    KillPlayer(pl);
            foreach (var st in this[p].stuff)
                st.state &= ~State.OnBridge;
            this[p].terrainType = TerrainType.Water;
        }

        #endregion

        private bool Parallel(Direction dir,Orientation or)
        {
            return ((or == Orientation.Horisontal && (dir == Direction.Left || dir == Direction.Right)) || (or == Orientation.Vertical && (dir == Direction.Up || dir == Direction.Down)));
        }
    }
}