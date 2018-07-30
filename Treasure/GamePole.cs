using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Treasure
{
    class GamePole
    {        
        private const int RiverFlow = 2;
        public static Dictionary<Direction, Point> directions = new Dictionary<Direction, Point>
        {
            {Direction.Up,new Point(0,-1)},
            {Direction.Right,new Point(1,0)},
            {Direction.Down,new Point(0,1)},
            {Direction.Left,new Point(-1,0)},
        };
        private Point treasurePos;
        private Point lastRiverTilePos;
        private int width;
        private int height;
        private Tile[,] tiles;
        private Tile[] holes;
        public Player[] players;
        public GamePole(int width,int height, Player[] players,Random rnd)
        {
            this.width = width;
            this.height = height;
            this.players = players;
            Generate(rnd);
        }

        private Tile this[int x, int y]
        {
            get
            {
                return tiles[x, y];
            }
            set
            {
                tiles[x, y] = value;
            }
        }
        private Tile this[Point p]
        {
            get
            {
                return tiles[p.X, p.Y];
            }
            set
            {
                tiles[p.X,p.Y] = value;
            }
        }
        private void Generate(Random rnd)
        {
            do
            {
                tiles = new Tile[width, height];
                GenerateField();
                GenerateRiver(rnd);
                GenerateSwamps(rnd, 3, 4);
                GenerateHoles(rnd, 3);
                GenerateHomes(rnd, players);
                GenerateWalls(rnd, 0.1);
                GenerateTreasure(rnd);
            }
            while (!(CheckHoles() && CheckRiver() && CheckTreasure()));
        }
        private void GenerateField()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = new Tile(this, new Point(x, y), TerrainType.Field);
                }
            }
        }
        private void GenerateRiver(Random rnd)
        {
            Point p = new Point(rnd.Next(width), height - 1);
            lastRiverTilePos = p;
            Tile lastTile = null;
            while (p.Y >= 0)
            {
                this[p] = new Tile(this, p, TerrainType.Water, lastTile);
                lastTile = this[p];
                List<Direction> dir = new List<Direction>() { Direction.Up };
                if (p.X != 0 && this[p.X - 1,p.Y].terrainType == TerrainType.Field && (p.Y == height - 1 || this[p.X - 1, p.Y + 1].terrainType != TerrainType.Water))
                {
                    dir.Add(Direction.Left);
                }
                if (p.X != width - 1 && this[p.X + 1, p.Y].terrainType == TerrainType.Field && (p.Y == height - 1 || this[p.X + 1, p.Y + 1].terrainType != TerrainType.Water))
                {
                    dir.Add(Direction.Right);
                }
                p += directions[dir[rnd.Next(dir.Count)]];
            }
        }
        private void GenerateSwamps(Random rnd,int length,int count)
        {
            for (int k = 0;k < count; k++)
            {
                List<Point> swamp = new List<Point>();
                List<Point> maybe = new List<Point>();
                Point p;
                bool b;
                do
                {
                    b = true;
                    p = new Point(rnd.Next(width), rnd.Next(height));
                    foreach (var d in directions.Values)
                    {
                        Point pp = p + d;
                        if (OnPole(pp) && this[pp].terrainType == TerrainType.Swamp)
                        {
                            b = false;
                            break;
                        }
                    }
                }
                while (this[p].terrainType != TerrainType.Field || !b);
                swamp.Add(p);
                foreach (var d in directions.Values)
                {
                    Point pp = p + d;
                    foreach (var dd in directions.Values)
                    {
                        Point ppp = pp + dd;
                        if (OnPole(ppp) && this[ppp].terrainType == TerrainType.Swamp)
                        {
                            goto BREAK1;
                        }
                    }
                    if (OnPole(pp) && this[pp].terrainType == TerrainType.Field)
                    {
                        maybe.Add(pp);
                    }
                    BREAK1:;
                }
                for (int i = 1; i < length; i++)
                {
                    if (maybe.Count == 0)
                    {
                        k--;
                        goto BREAK2;
                    }
                    p = maybe[rnd.Next(maybe.Count)];
                    maybe.Remove(p);
                    swamp.Add(p);
                    foreach (var d in directions.Values)
                    {
                        Point pp = p + d;
                        if (!maybe.Contains(pp) && !swamp.Contains(pp))
                        {
                            foreach (var dd in directions.Values)
                            {
                                Point ppp = pp + dd;
                                if (OnPole(ppp) && this[ppp].terrainType == TerrainType.Swamp)
                                {
                                    goto BREAK3;
                                }
                            }
                            if (OnPole(pp) && this[pp].terrainType == TerrainType.Field)
                            {
                                maybe.Add(pp);
                            }
                        }
                        BREAK3:;
                    }
                }
                if (swamp.Distinct().Count() != swamp.Count)
                {
                    throw new Exception();
                }
                foreach (var s in swamp)
                {
                    this[s] = new Tile(this,s,TerrainType.Swamp);
                }
                BREAK2:;
            }
        }
        private void GenerateHoles(Random rnd,int count)
        {
            holes = new Tile[count];
            for (int i = 0;i < count; i++)
            {
                Point p;
                do
                {
                    p = new Point(rnd.Next(width), rnd.Next(height));
                }
                while (this[p].terrainType != TerrainType.Field);
                this[p] = new Tile(this,p,TerrainType.Hole,i + 1);
                holes[i] = this[p];
            }
        }
        private void GenerateHomes(Random rnd, Player[] players)
        {
            List<Point> homes = new List<Point>();
            for (int i = 0; i < players.Length; i++)
            {
                Point p;
                do
                {
                    p = new Point(rnd.Next(width), rnd.Next(height));
                }
                while (this[p].terrainType != TerrainType.Field);
                foreach (var h in homes)
                {
                    if (Math.Abs(h.X - p.X) + Math.Abs(h.Y - p.Y) <= 2)
                    {
                        i--;
                        goto BREAK;
                    }
                }
                homes.Add(p);
                Tile t = new Tile(this,p,TerrainType.Home,players[i]);
                this[p] = t;
                players[i].home = t;
                players[i].position = p;
                BREAK:;
            }
        }
        private void GenerateWalls(Random rnd, double chance)
        {
            for (int x = 0;x < width; x++)
            {
                this[x, 0].walls[(int)Direction.Up] = true;
                this[x, height - 1].walls[(int)Direction.Down] = true;
            }
            for (int y = 0; y < height; y++)
            {
                this[0,y].walls[(int)Direction.Left] = true;
                this[width - 1, y].walls[(int)Direction.Right] = true;
            }
            for (int x = 0;x < width; x++)
            {
                for (int y = 0;y < height - 1; y++)
                {
                    if (rnd.NextDouble() < chance && !(this[x,y].terrainType == TerrainType.Swamp && this[x, y + 1].terrainType == TerrainType.Swamp) && !(this[x, y].terrainType == TerrainType.Water && this[x, y + 1].terrainType == TerrainType.Water))
                    {
                        this[x, y].walls[(int)Direction.Down] = true;
                        this[x, y + 1].walls[(int)Direction.Up] = true;
                    }
                }
            }
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (rnd.NextDouble() < chance && !(this[x, y].terrainType == TerrainType.Swamp && this[x + 1, y].terrainType == TerrainType.Swamp) && !(this[x, y].terrainType == TerrainType.Water && this[x + 1, y].terrainType == TerrainType.Water))
                    {
                        this[x, y].walls[(int)Direction.Right] = true;
                        this[x + 1, y].walls[(int)Direction.Left] = true;
                    }
                }
            }
        }
        private void GenerateTreasure(Random rnd)
        {
            Point p;
            do
            {
                p = new Point(rnd.Next(width), rnd.Next(height));
            }
            while (this[p].terrainType != TerrainType.Field);
            this[p].treasure = true;
            treasurePos = p;
        }
        private bool CheckRiver()
        {
            Queue<Point> check = new Queue<Point>();
            bool[,] canGo = new bool[width,height];
            canGo[lastRiverTilePos.X, lastRiverTilePos.Y] = true;
            check.Enqueue(lastRiverTilePos);
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
            foreach (var p in players)
            {
                if (!canGo[p.home.position.X, p.home.position.Y])
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckTreasure()
        {
            foreach (var pl in players)
            {
                Queue<Point> check = new Queue<Point>();
                bool[,] canGo = new bool[width, height];
                canGo[pl.home.position.X, pl.home.position.Y] = true;
                check.Enqueue(pl.home.position);
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
                if (!canGo[treasurePos.X, treasurePos.Y])
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckHoles()
        {
            foreach (var h in holes)
            {
                bool b = false;
                foreach (var d in directions.Keys)
                {
                    Point pp = CheckGo(h.position, d);
                    b |= (pp != h.position);
                }
                if (!b)
                {
                    return false;
                }
            }
            return true;
        }
        public string Update(Player player, Direction direction, Action action)
        {
            switch (action)
            {
                case Action.Go:
                    return Go(player, direction);
                case Action.Shoot:
                    Shoot(player, direction);
                    return "";
                default:
                    throw new Exception();
            }
        }
        private void Shoot(Player player, Direction direction)
        {
            player.bulletCount--;
            for (int i = 0; true; i++)
            {
                Point p = player.position + new Point(directions[direction].X * i, directions[direction].Y * i);
                if (i != 0 && players.Count(_ => _.position == p) > 0)
                {
                    KillPlayer(players.First(_ => _.position == p));
                    break;
                }
                if (this[p].walls[(int)direction])
                {
                    BreakWall(p, direction);
                    break;
                }
            }
        }
        private bool BreakWall(Point p,Direction direction)
        {
            Point pp = p + directions[direction];
            if (OnPole(pp))
            {
                this[p].walls[(int)direction] = false;
                this[pp].walls[((int)direction + 2) % 4] = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool TakeTreasure(Player player)
        {
            if (this[player.position].treasure)
            {
                this[player.position].treasure = false;
                player.treasure = true;
                return true;
            }
            return false;
        }
        private void KillPlayer(Player player)
        {
            player.wasKilled = true;
            player.UpdateBullets();
            if (player.treasure)
            {
                player.treasure = false;
                this[player.position].treasure = true;
            }
            player.position = player.home.position;
            if (players.Count(_ => _.position == player.position && _ != player) > 0)
            {
                KillPlayer(players.Single(_ => _.position == player.position && _ != player));
            }
            TakeTreasure(player);
        }
        private Point CheckGo(Point start,Direction direction)
        {
            if (this[start].walls[(int)direction])
            {
                return start;
            }
            Point p = start + directions[direction];
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
                    for (int i = 0;i < 2; i++)
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
        private string Go(Player player, Direction direction)
        {
            if (this[player.position].walls[(int)direction])
            {
                return "Wall";
            }
            List<string> answer = new List<string>();
            Point p = player.position + directions[direction];
            Tile t = this[p];
            switch (t.terrainType)
            {
                case TerrainType.Field:
                    answer.Add("Field");
                    player.position = p;
                    if (players.Count(_ => _.position == player.position && _ != player) > 0)
                    {
                        KillPlayer(players.Single(_ => _.position == player.position && _ != player));
                    }
                    if (TakeTreasure(player))
                    {
                        answer[answer.Count - 1] += " with treasure";
                    }
                    break;
                case TerrainType.Hole:
                    player.position = p;
                    answer.Add("Hole " + t.intParam);
                    if (players.Count(_ => _.position == player.position && _ != player) > 0)
                    {
                        KillPlayer(players.Single(_ => _.position == player.position && _ != player));
                    }
                    if (TakeTreasure(player))
                    {
                        answer[answer.Count - 1] += " with treasure";
                    }
                    player.position = holes[t.intParam % holes.Length].position;
                    answer.Add("Hole " + t.intParam);
                    if (players.Count(_ => _.position == player.position && _ != player) > 0)
                    {
                        KillPlayer(players.Single(_ => _.position == player.position && _ != player));
                    }
                    if (TakeTreasure(player))
                    {
                        answer[answer.Count - 1] += " with treasure";
                    }
                    break;
                case TerrainType.Home:
                    player.position = p;
                    if (t == player.home)
                    {
                        answer.Add("Your home");
                        player.UpdateBullets();
                    }
                    else
                        answer.Add(t.playerParam.name + "/'s home");
                    if (players.Count(_ => _.position == player.position && _ != player) > 0)
                    {
                        KillPlayer(players.Single(_ => _.position == player.position && _ != player));
                    }
                    if (TakeTreasure(player))
                    {
                        answer[answer.Count - 1] += " with treasure";
                    }
                    break;
                case TerrainType.Swamp:
                    answer.Add("Swamp");
                    break;
                case TerrainType.Water:
                    answer.Add("River");
                    player.position = p;
                    if (players.Count(_ => _.position == player.position && _ != player) > 0)
                    {
                        KillPlayer(players.Single(_ => _.position == player.position && _ != player));
                    }
                    if (TakeTreasure(player))
                    {
                        answer[answer.Count - 1] += " with treasure";
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (this[p].LastRiverTile)
                        {
                            answer.Add("Grate");
                            break;
                        }
                        else
                        {
                            p = this[p].tileParam.position;
                            player.position = p;
                            answer.Add("River");
                            if (players.Count(_ => _.position == player.position && _ != player) > 0)
                            {
                                KillPlayer(players.Single(_ => _.position == player.position && _ != player));
                            }
                            if (TakeTreasure(player))
                            {
                                answer[answer.Count - 1] += " with treasure";
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception();
            }
            return String.Join(" - ", answer);
        }
        public bool OnPole(Point p)
        {
            return p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;
        }
        public void Draw(SpriteBatch spriteBatch,SpriteEffects effects)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y].Draw(spriteBatch, effects, x * 64,y * 64,64);
                }
            }
            foreach (var p in players)
            {
                p.Draw(spriteBatch, effects, p.X * 64, p.Y * 64, 64);
            }
        }
    }
}