// New offline adaptation. This forwards to real Unity PlayerPrefs, never a fake SDK.
using UnityEngine;

namespace Pool3D.Offline
{
    public static class OfflinePreferences
    {
        private static readonly OfflineSaveNamespace storage = new OfflineSaveNamespace();
        public static bool IsOffline { get { return storage.IsOffline; } }
        public static void SelectProfile(string profile) { storage.SelectProfile(profile); }

        public static bool HasKey(string key) { return PlayerPrefs.HasKey(storage.ResolveKey(key)); }
        public static void DeleteKey(string key) { PlayerPrefs.DeleteKey(storage.ResolveKey(key)); }
        public static string GetString(string key) { return GetString(key, ""); }
        public static string GetString(string key, string fallback) { return PlayerPrefs.GetString(storage.ResolveKey(key), fallback); }
        public static void SetString(string key, string value) { PlayerPrefs.SetString(storage.ResolveKey(key), value); }
        public static float GetFloat(string key, float fallback) { return PlayerPrefs.GetFloat(storage.ResolveKey(key), fallback); }
        public static void SetFloat(string key, float value) { PlayerPrefs.SetFloat(storage.ResolveKey(key), value); }
        public static void Save() { PlayerPrefs.Save(); }
    }
}
