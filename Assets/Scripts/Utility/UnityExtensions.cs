using UnityEngine.Networking;

namespace Utility {

    public static class UnityExtensions {

        public static void Replace(this SyncList<string> syncList, string[] array) {
            // crappy way to check duplicate in an attempt to save bandwidth
            if (syncList.Contains(array[0])) return;

            if (syncList.Count > 0) {
                syncList.Clear();
            }
            for (int i = 0; i < array.Length; i++) {
                syncList.Add(array[i]);
            }
        }
    }
}
