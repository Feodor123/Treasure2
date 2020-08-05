using System;
using System.Threading;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Android.Views;

using Treasure;

namespace TreasureAndroid.UserInterface
{
    public class BackAnimator : Microsoft.Xna.Framework.Game
    {
        Color[] playersColors = new Color[]
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Aqua,
            Color.Gainsboro,
            Color.Black,
            Color.Gray,
        };
        TimeSpan lastUpdate = new TimeSpan();
        TimeSpan updateTime = TimeSpan.FromSeconds(0.1);
        Texture2D atlasTexture;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        View view;
        Microsoft.Xna.Framework.Point bias;
        Microsoft.Xna.Framework.Point screenSize;
        Treasure.Game game;
        int a = 128;

        public BackAnimator()
        {            
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }
        public void GenerateGame()
        {
            var gp = new GameParameters()
            {
                FieldWidth = screenSize.X / a,
                FieldHeight = screenSize.Y / a,
                PortalCount = 3,
                SwampCount = 4,
                SwampSize = 3,
                PlayerCount = 4,
            };

            game = new Treasure.Game(gp, new IPlayerController[] { new BotController(gp), new BotController(gp), new BotController(gp), new BotController(gp) }, new Random());

            game.InitializeField();
        }
        protected override void Initialize()
        {
            base.Initialize();
            view = (View)Services.GetService(typeof(View));
            screenSize = new Microsoft.Xna.Framework.Point(view.Width,view.Height);
            graphics.PreferredBackBufferWidth = screenSize.X;
            graphics.PreferredBackBufferHeight = screenSize.Y;

            bias = new Microsoft.Xna.Framework.Point((screenSize.X % a) / 2, (screenSize.Y % a) / 2);

            GenerateGame();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            atlasTexture = Content.Load<Texture2D>("Tiles");
        }
        protected override void UnloadContent()
        {

        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            if (gameTime.TotalGameTime - lastUpdate > updateTime)
            {
                lastUpdate = gameTime.TotalGameTime;
                var p = game.Step();
                if (p != -1)
                {
                    GenerateGame();
                }
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            DrawField();
            spriteBatch.End();
            base.Draw(gameTime);
        }
        protected void DrawField()
        {
            int b = 1;
            int bx2 = b * 2;
            for (int x = 0; x < game.gameParameters.FieldWidth; x++)
            {
                for (int y = 0; y < game.gameParameters.FieldHeight; y++)
                {
                    Tile t = game.field[x, y];
                    Rectangle r = new Rectangle();
                    switch (t.terrainType)
                    {
                        case TerrainType.Field:
                            r = new Rectangle(0 + b, 0 + b, 64 - bx2, 64 - bx2);
                            break;
                        case TerrainType.Water:
                            r = new Rectangle(64 + b, 0 + b, 64 - bx2, 64 - bx2);
                            break;
                        case TerrainType.Swamp:
                            r = new Rectangle(256 + b, 0 + b, 64 - bx2, 64 - bx2);
                            break;
                        case TerrainType.Hole:
                            r = new Rectangle(128 + b, 0 + b, 64 - bx2, 64 - bx2);
                            break;
                        case TerrainType.Home:
                            r = new Rectangle(0 + b, 0 + b, 64 - bx2, 64 - bx2);
                            break;
                        default:
                            throw new Exception();
                    }
                    spriteBatch.Draw(atlasTexture, new Rectangle(x * a + bias.X, y * a + bias.Y, a, a), r, Color.White);
                    if (t.terrainType == TerrainType.Home)
                    {
                        spriteBatch.Draw(atlasTexture, new Rectangle(x * a + bias.X, y * a + bias.Y, a, a), new Rectangle(320 + b,64 + b,64 - bx2,64 - bx2), playersColors[t.intParam - 1]);
                    }
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
                                destinationRectangle: new Rectangle(x * a + a/2 + bias.X, y * a + a/2 + bias.Y, a, a),
                                sourceRectangle: r,
                                color: Color.White,
                                rotation: f,
                                origin: new Vector2(32, 32),
                                effects: SpriteEffects.None,
                                layerDepth: 0);
                        }
                    foreach (var s in t.stuff)
                    {
                        switch (s.type)
                        {
                            case StuffType.Treasure:
                                r = new Rectangle(128 + b, 64 + b, 64 - bx2, 64 - bx2);
                                break;
                            default:
                                throw new Exception();
                        }
                        spriteBatch.Draw(atlasTexture, new Rectangle(x * a + bias.X, y * a + bias.Y, a, a), r, Color.White);
                    }
                }
            }
            for (int i = 0;i < game.field.players.Length;i++)
            {
                FieldPlayer p = game.field.players[i];
                spriteBatch.Draw(atlasTexture, new Rectangle(p.X * a + bias.X, p.Y * a + bias.Y, a, a), new Rectangle(192 + b,64 + b,64 - bx2,64 - bx2), playersColors[i]);
            }
        }
    }
}