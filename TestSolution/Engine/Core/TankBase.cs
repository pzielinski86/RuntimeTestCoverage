using Core.Logs;
using System;

namespace Core
{
    public abstract class TankBase
    {
        public event EventHandler TankDestroyed;

        private readonly ILogger _log = new UnityLogger();

        protected TankBase()
        {
            Health = new TankHealth(this,maxHealth: 100f);
            Id = Guid.NewGuid();
            Location = new TankLocation(0.6f, 0.5f);
            Turret = new TankTurret(this, 0.1f, 0.1f);
            Name = GetType().Name;
        }
        
        public ILogger Logger
        {
            get { return _log; }
        }

        public TankHealth Health { get; private set; }        
        public Guid Id { get; set; }
        public string Name { get; protected set; }
        public TankLocation Location { get; private set; }
        public TankTurret Turret { get; set; }

        public abstract void Update(Battlefield battlefield);

        public void Hit(Bullet bullet)
        {
            if (bullet == null)
                throw new ArgumentNullException("bullet");

            Health.CurrentHealth -= bullet.HitPower;
        }

        internal void Destroy()
        {
            if(Health.CurrentHealth>0)
                Health.CurrentHealth = 0;
            
            if (TankDestroyed != null)
                TankDestroyed(this, EventArgs.Empty);
        }
    }
}
