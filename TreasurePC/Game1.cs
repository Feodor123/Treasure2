using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Treasure;

namespace TreasurePC
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Treasure.Game game;

        Texture2D atlasTexture;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        int a = 64;
        Random rnd = new Random();
        
        public void NewTGame()
        {
            game = new Treasure.Game(new GameParameters()
            {
                FieldWidth = 6,
                FieldHeight = 6,
                PortalCount = 0,
                SwampCount = 0,
                SwampSize = 0,

                Players = new PlayerHelper[]
                {
                    new PlayerHelper(new PlayerParameters(null)),
                    new PlayerHelper(new PlayerParameters(null)),
                    new PlayerHelper(new PlayerParameters(null)),
                    new PlayerHelper(new PlayerParameters(null)),
                }
            }, rnd.Next());
            game.InitializeField();
            /*
            game.field[1, 1].terrainType = TerrainType.Field;
            game.field[2, 1].terrainType = TerrainType.Field;
            game.field[3, 1].terrainType = TerrainType.Field;
            game.field[4, 1].terrainType = TerrainType.Water;
            game.field[1, 2].terrainType = TerrainType.Field;
            game.field[2, 2].terrainType = TerrainType.Water;
            game.field[3, 2].terrainType = TerrainType.Water;
            game.field[4, 2].terrainType = TerrainType.Water;
            game.field[1, 3].terrainType = TerrainType.Field;
            game.field[2, 3].terrainType = TerrainType.Water;
            game.field[3, 3].terrainType = TerrainType.Water;
            game.field[4, 3].terrainType = TerrainType.Field;
            game.field[1, 4].terrainType = TerrainType.Field;
            game.field[2, 4].terrainType = TerrainType.Field;
            game.field[3, 4].terrainType = TerrainType.Water;
            game.field[4, 4].terrainType = TerrainType.Field;
            */
        }

        public void NewTGame(int seed)
        {
            game = new Treasure.Game(new GameParameters()
            {
                FieldWidth = 5,
                FieldHeight = 10,
                PortalCount = 3,
                SwampCount = 4,
                SwampSize = 3,

                Players = new PlayerHelper[]
                {
                    new PlayerHelper(new PlayerParameters(null)),
                    new PlayerHelper(new PlayerParameters(null)),
                    new PlayerHelper(new PlayerParameters(null)),
                    new PlayerHelper(new PlayerParameters(null)),
                }
            }, seed);
            bool b = false;
            while (!b)
                b = game.InitializeField();
        }

        public Game1()
        {
            NewTGame();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = a * game.gameParameters.FieldWidth;
            graphics.PreferredBackBufferHeight = a * game.gameParameters.FieldHeight;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            atlasTexture = Content.Load<Texture2D>("Tiles");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                NewTGame();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            for (int x = 0;x < game.gameParameters.FieldWidth; x++)
            {
                for (int y = 0;y < game.gameParameters.FieldHeight; y++)
                {
                    Tile t = game.field[x, y];
                    Rectangle r = new Rectangle();
                    switch (t.terrainType)
                    {
                        case TerrainType.Field:
                            r = new Rectangle(0, 0, 64, 64);
                            break;
                        case TerrainType.Water:
                            r = new Rectangle(64, 0, 64, 64);
                            break;
                        case TerrainType.Swamp:
                            r = new Rectangle(256, 0, 64, 64);
                            break;
                        case TerrainType.Hole:
                            r = new Rectangle(128, 0, 64, 64);
                            break;
                        case TerrainType.Home:
                            r = new Rectangle(192, 0, 64, 64);
                            break;
                        default:
                            throw new Exception();
                    }
                    spriteBatch.Draw(atlasTexture, new Rectangle(x * a, y * a, a, a), r, Color.White);
                    foreach (var pair in t.walls)
                        if (pair.Value != BorderType.Empty)
                        {
                            switch (pair.Value)
                            {
                                case BorderType.BreakableWall:
                                    r = new Rectangle(256, 64, 64, 64);
                                    break;
                                case BorderType.UnbreakableWall:
                                    r = new Rectangle(0, 64, 64, 64);
                                    break;
                                case BorderType.Grate:
                                    r = new Rectangle(64, 64, 64, 64);
                                    break;
                                default:
                                    throw new Exception();
                            }
                            float f = 0;
                            switch (pair.Key)
                            {
                                case Direction.Up:
                                    f = 0;
                                    break;
                                case Direction.Right:
                                    f = MathHelper.PiOver2;
                                    break;
                                case Direction.Down:
                                    f = MathHelper.PiOver2 * 2;
                                    break;
                                case Direction.Left:
                                    f = MathHelper.PiOver2 * 3;
                                    break;
                                default:
                                    throw new Exception();
                            }
                            spriteBatch.Draw(
                                texture: atlasTexture, 
                                destinationRectangle: new Rectangle(x * a + 32, y * a + 32, a, a), 
                                sourceRectangle: r, 
                                color: Color.White,
                                rotation: f,
                                origin: new Vector2(32,32),
                                effects: SpriteEffects.None,
                                layerDepth: 0);
                        }
                    foreach (var s in t.stuff)
                    {
                        switch (s.type)
                        {
                            case StuffType.Treasure:
                                r = new Rectangle(128, 64, 64, 64);
                                break;
                            default:
                                throw new Exception();
                        }
                        spriteBatch.Draw(atlasTexture, new Rectangle(x * a, y * a, a, a), r, Color.White);
                    }
                }
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
