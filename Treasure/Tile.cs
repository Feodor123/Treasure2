using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Treasure
{
    class Tile
    {
        public bool[] walls = new bool[4];
        public TerrainType terrainType;
        public Point position;
        public int intParam;
        public Tile tileParam;
        public Player playerParam;
        public bool treasure;
        private GamePole gamePole;
        public Tile(GamePole gamePole, Point position, TerrainType terrainType, Tile tileParam)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.tileParam = tileParam;
            this.gamePole = gamePole;
        }
        public Tile(GamePole gamePole, Point position, TerrainType terrainType, int intParam)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.intParam = intParam;
            this.gamePole = gamePole;
        }
        public Tile(GamePole gamePole, Point position, TerrainType terrainType, Player playerParam)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.playerParam = playerParam;
            this.gamePole = gamePole;
        }
        public Tile(GamePole gamePole, Point position, TerrainType terrainType)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.gamePole = gamePole;
        }
        public bool LastRiverTile
        {
            get
            {
                return terrainType == TerrainType.Water && tileParam == null;
            }
        }
        public void Draw(SpriteBatch spriteBatch, SpriteEffects effects,int x,int y,int size)
        {
            Rectangle source = new Rectangle();
            switch (terrainType)
            {
                case TerrainType.Field:
                    source = new Rectangle(0,0,64,64);
                    break;
                case TerrainType.Hole:
                    source = new Rectangle(128,0,64,64);
                    break;
                case TerrainType.Home:
                    source = new Rectangle(192,0,64,64);
                    break;
                case TerrainType.Swamp:
                    source = new Rectangle(256,0,64,64);
                    break;
                case TerrainType.Water:
                    source = new Rectangle(64,0,64,64);
                    break;
            }
            spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x,y,size,size),sourceRectangle: source,color: Color.White);
            if (LastRiverTile)
            {
                spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x + 32, y + 32, size, size), sourceRectangle: new Rectangle(64, 64, 64, 64), color: Color.White, rotation: MathHelper.Pi, origin: new Vector2(32,32), effects: effects, layerDepth: 0);
                for (int i = 0; i < 4; i++)
                {
                    if (walls[i] && i != 2)
                    {
                        Point p = position + GamePole.directions[(Direction)i];
                        if (gamePole.OnPole(p))
                        {
                            spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x + 32, y + 32, size, size), sourceRectangle: new Rectangle(256, 64, 64, 64), color: Color.White, rotation: i * MathHelper.PiOver2, origin: new Vector2(32, 32), effects: effects, layerDepth: 0);
                        }
                        else
                        {
                            spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x + 32, y + 32, size, size), sourceRectangle: new Rectangle(0, 64, 64, 64), color: Color.White, rotation: i * MathHelper.PiOver2, origin: new Vector2(32, 32), effects: effects, layerDepth: 0);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0;i < 4; i++)
                {
                    if (walls[i])
                    {
                        Point p = position + GamePole.directions[(Direction)i];
                        if (gamePole.OnPole(p))
                        {
                            spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x + 32, y + 32, size, size), sourceRectangle: new Rectangle(256, 64, 64, 64), color: Color.White, rotation: i * MathHelper.PiOver2, origin: new Vector2(32, 32), effects: effects, layerDepth: 0);
                        }
                        else
                        {
                            spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x + 32, y + 32, size, size), sourceRectangle: new Rectangle(0, 64, 64, 64), color: Color.White, rotation: i * MathHelper.PiOver2, origin: new Vector2(32, 32), effects: effects, layerDepth: 0);
                        }
                    }
                }
            }
            if (treasure)
            {
                spriteBatch.Draw(texture: Game1.texture, destinationRectangle: new Rectangle(x, y, size, size), sourceRectangle: new Rectangle(192, 64, 64, 64), color: Color.White);
            }
        }
    }
}