using UnityEngine;
namespace Pool3D.Offline.Presentation
{
    public sealed class IntegratedArtAssets : ScriptableObject
    {
        public GameObject environmentPrefab;
        public GameObject cuePrefab;
        public GameObject[] ballPrefabs=new GameObject[16];
    }
    public static class PresentationLifetime
    {
        public static void Release(Object value) {if(value==null)return;if(Application.isPlaying)Object.Destroy(value);else Object.DestroyImmediate(value);}
    }
}
