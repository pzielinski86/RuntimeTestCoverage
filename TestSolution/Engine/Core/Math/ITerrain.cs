using UnityEngine;

namespace Core.Math
{
    public interface ITerrain
    {
        float GetTerrainHeightOn(Vector3 position);
        Bounds Bounds { get; }

    }
}