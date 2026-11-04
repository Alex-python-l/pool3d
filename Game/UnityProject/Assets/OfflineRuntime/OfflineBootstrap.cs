using UnityEngine;

namespace Pool3D.Offline
{
    [DefaultExecutionOrder(-10000)]
    public sealed class OfflineBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Dedicated scene contains no original application startup components.
            OfflinePreferences.SelectProfile("integrated-v1");
            Application.targetFrameRate = 60;
            gameObject.AddComponent<OfflineMatchController>();
        }
    }
}
