using System.Collections.Generic;

namespace Treasure
{
    public class FieldPlayer
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
        public Tile home;
        public Point position;
        readonly int maxBulletCount;
        public int bulletCount;
        public int homeBulletCount;

        public FieldPlayer(int maxBulletCount, int startHomeBulletCount)
        {
            this.maxBulletCount = maxBulletCount;
            bulletCount = maxBulletCount;
            homeBulletCount = startHomeBulletCount;
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