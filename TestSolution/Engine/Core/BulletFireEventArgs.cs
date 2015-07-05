using System;

namespace Core
{
    public sealed class BulletFireEventArgs:EventArgs
    {
        public Bullet Bullet { get; private set; }

        public BulletFireEventArgs(Bullet bullet)
        {
            Bullet = bullet;
        }
    }
}
