
namespace Core
{
    public sealed class TankHealth
    {
        private readonly TankBase _tank;
        private float _currentHealth;

        public TankHealth(TankBase tank,float maxHealth)
        {
            _tank = tank;
            CurrentHealth = MaxHealth = maxHealth;
        }

        public float MaxHealth { get; private set; }

        public float CurrentHealth
        {
            get { return _currentHealth; }
            internal set
            {
                _currentHealth = value;

                if (_currentHealth <= 0)
                    _tank.Destroy();
            }
        }
    }
}
