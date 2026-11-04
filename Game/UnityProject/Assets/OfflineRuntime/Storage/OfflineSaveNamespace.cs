// New offline adaptation, authorized 2026-10-21. No original-native-body claim.
using System;

namespace Pool3D.Offline
{
    /// <summary>One storage namespace for a session. Select before any RWData access.</summary>
    public sealed class OfflineSaveNamespace
    {
        private string profile;
        private bool accessed;
        public bool IsOffline { get { return profile != null; } }

        public void SelectProfile(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 48)
                throw new ArgumentException("Profile must contain 1 to 48 ASCII letters, digits, '-' or '_'.", nameof(name));
            foreach (char c in name)
                if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') &&
                    !(c >= '0' && c <= '9') && c != '-' && c != '_')
                    throw new ArgumentException("Invalid offline profile name.", nameof(name));
            if (accessed)
                throw new InvalidOperationException("Choose the offline profile before the first RWData Load or Save.");
            profile = name;
        }

        public string ResolveKey(string originalKey)
        {
            if (string.IsNullOrEmpty(originalKey))
                throw new ArgumentException("A nonempty preference key is required.", nameof(originalKey));
            accessed = true;
            return profile == null ? originalKey : "Pool3D.Offline/v1/" + profile + "/" + originalKey;
        }
    }
}
