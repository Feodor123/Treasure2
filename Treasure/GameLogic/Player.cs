using System.Collections.Generic;

namespace Treasure
{
    public class Player
    {
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

        public List<Stuff> stuff = new List<Stuff>();
        public const int maxBulletCount = 3;
        public const int startHomeBulletCount = 20;
        public Tile home;
        public Point position;
        public int bulletCount = maxBulletCount;
        public int homeBulletCount = startHomeBulletCount;
        public PlayerHelper playerHelper;

        public Player(PlayerHelper playerHelper)
        {
            this.playerHelper = playerHelper;
        }

        public void UpdateBullets()
        {
            if (bulletCount < maxBulletCount)
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
    }
}