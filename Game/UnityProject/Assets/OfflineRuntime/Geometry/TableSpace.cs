using UnityEngine;

namespace Pool3D.Offline.Geometry
{
    public static class TableSpace
    {
        public const float WorldUnitsPerMetre = 100f;
        public static Vector3 SimToWorld(Vector3 position) { return new Vector3(position.y*.1f, position.z*.1f, position.x*.1f); }
        public static Vector3 WorldToSim(Vector3 position) { return new Vector3(position.z*10f, position.x*10f, position.y*10f); }
        public static Vector3 ArtOrigin { get { return new Vector3(0, -2.8575f, 0); } }
    }
}
