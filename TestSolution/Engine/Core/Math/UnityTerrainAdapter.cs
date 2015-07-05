using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Core.Math
{
    public class UnityTerrainAdapter : ITerrain
    {
        private readonly Terrain _terrain;

        public UnityTerrainAdapter(Terrain terrain)
        {
            _terrain = terrain;
        }

        public float GetTerrainHeightOn(Vector3 position)
        {
            return _terrain.SampleHeight(position);
        }

        public Bounds Bounds
        {
            get
            {
                var terrainSize = new Vector3(_terrain.collider.bounds.size.x, 100f, _terrain.collider.bounds.size.z);
                return new Bounds(_terrain.collider.bounds.center, terrainSize);
            }
        }
    }
}
