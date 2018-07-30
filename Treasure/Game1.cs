using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Treasure
{
    public class Game1 : Game
    {
        public static Texture2D texture;
        public static SpriteFont spriteFont;
        public const int minSwipeLength = 20;
        Player[] players;
        GamePole pole;
        GraphicsDeviceManager graphics;
        SpriteEffects effects = new SpriteEffects();
        SpriteBatch spriteBatch;
        Random rnd = new Random();
        int currentPlayer;

        public Game1(int poleWidth,int poleHeight,string[] names, ControllerType[] types)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            if (names.Length != types.Length)
            {
                throw new Exception();
            }
            players = new Player[names.Length];
            for (int i = 0;i < names.Length; i++)
            {
                players[i] = new Player(names[i],types[i],i);
            }
            pole = new GamePole(poleWidth, poleHeight, players, rnd);
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            texture = Content.Load<Texture2D>("Tiles");
            spriteFont = Content.Load<SpriteFont>("SpriteFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            GestureSample gesture;
            TouchPanel.EnabledGestures = GestureType.Flick;
            while (TouchPanel.IsGestureAvailable)
            {
                gesture = TouchPanel.ReadGesture();
                Direction direction;
                if (Math.Abs(gesture.Delta.X) > Math.Abs(gesture.Delta.Y))
                {
                    if (gesture.Delta.X > 0)
                    {
                        direction = Direction.Right;
                    }
                    else
                    {
                        direction = Direction.Left;
                    }
                }
                else
                {
                    if (gesture.Delta.Y > 0)
                    {
                        direction = Direction.Down;
                    }
                    else
                    {
                        direction = Direction.Up;
                    }
                }
                pole.Update(players[currentPlayer], direction, Action.Go);
                currentPlayer = (currentPlayer + 1) % players.Length;
                break;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            pole.Draw(spriteBatch, effects);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
