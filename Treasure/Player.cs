using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Treasure
{
    class Player
    {
        public static Color[] Colors = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Black,
            Color.White,
            Color.Purple,
            Color.Pink,
        };
        public int X
        {
            get
            {
                return position.X;
            }
        }
        public int Y
        {
            get
            {
                return position.Y;
            }
        }
        public int colorNum;
        public bool wasKilled = false;
        public bool treasure = false;
        public const int maxBulletCount = 3;
        public const int startHomeBulletCount = 20;
        public ControllerType type;
        public string name;
        public Tile home;
        public Point position;
        public int bulletCount = maxBulletCount;
        public int homeBulletCount = startHomeBulletCount;
        public Player(string name,ControllerType type,int colorNum)
        {
            this.name = name;
            this.type = type;
            this.colorNum = colorNum;
        }
        public void UpdateBullets()
        {
            if (position == home.position && bulletCount < maxBulletCount)
            {
                if (homeBulletCount >= maxBulletCount - bulletCount)
                {
                    homeBulletCount -= maxBulletCount - bulletCount;
                    bulletCount = maxBulletCount;
                }
                else
                {
                    bulletCount += homeBulletCount;
                    homeBulletCount = 0;
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch, SpriteEffects effects, int x, int y, int size)
        {
            spriteBatch.Draw(Game1.texture,new Rectangle(x,y,size,size),new Rectangle(128,64,64,64),Colors[colorNum]);
        }
    }
}