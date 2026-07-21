using System.Runtime.InteropServices;

namespace Networking
{
    internal static class WebSocketCompatibility
    {
        internal static void Prepare()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebSocketEnsureCompatibility();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebSocketEnsureCompatibility();
#endif
    }
}
