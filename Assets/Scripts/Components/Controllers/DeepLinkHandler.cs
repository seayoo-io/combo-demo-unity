using System;
using UnityEngine;

public class DeepLinkHandler : MonoBehaviour
{
    void Awake()
    {
        // Listener for when the deep link URL is activated
        Application.deepLinkActivated += OnDeepLinkActivated;

        // If there is an URL passed during app launch,
        // it can be accessed directly like this:
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            Debug.Log("App opened with deep link: " + Application.absoluteURL);
            HandleDeepLink(Application.absoluteURL);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        Debug.Log("Deep link activated: " + url);
        HandleDeepLink(url);
    }

    private void HandleDeepLink(string url)
    {
        // Parse the URL and handle your logic here
        Uri uri = new Uri(url);

        // Example: Get query parameters
        string query = uri.Query;
        Debug.Log("Query parameters: " + query);
        Toast.Show($"Deep link query parameters:: {query}");
    }
}