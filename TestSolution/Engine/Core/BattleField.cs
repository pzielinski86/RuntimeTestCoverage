using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Collections;
using Core.Math;
using UnityEngine;

namespace Core
{
    public sealed class Battlefield
    {
        private readonly ObservableCollection<Bullet> _bullets = new ObservableCollection<Bullet>();

        public Battlefield(ITerrain terrain)
        {
            Terrain = terrain;

            AllTanks = new ObservableCollection<TankBase>();
            AllTanks.ItemAdded += TankAdded;
            AllTanks.ItemRemoved += TankRemoved;
        }

        public ObservableCollection<TankBase> AllTanks { get; private set; }

        public ObservableCollection<Bullet> Bullets
        {
            get { return _bullets; }
        }

        public ITerrain Terrain { get; private set; }

        public IEnumerable<TankBase> GetEnemyTanks(TankBase myTank)
        {
            return AllTanks.Where(tank => tank != myTank);
        }
     
        private void TankAdded(object sender, ObservableCollectionEventArgs<TankBase> e)
        {
            TankBase newTank = e.Item;
            newTank.Turret.Cannon.BulletFired += BulletFired;
        }

        private void TankRemoved(object sender, ObservableCollectionEventArgs<TankBase> e)
        {
            TankBase newTank = e.Item;
            newTank.Turret.Cannon.BulletFired -= BulletFired;
        }

        private void BulletFired(object sender, BulletFireEventArgs e)
        {
            Bullet newBullet = e.Bullet;
            newBullet.BulletDestroyed += BulletDestroyed;
            
            _bullets.Add(newBullet);
        }

        private void BulletDestroyed(object sender, EventArgs e)
        {
            Bullet bullet = (Bullet)sender;
            bullet.BulletDestroyed -= BulletDestroyed;

            _bullets.Remove(bullet);
        }  
    }
}
