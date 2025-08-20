using UnityEngine;
using UnityEngine.SceneManagement;
using Combo;

public class ProcessDeepLinkMngr : MonoBehaviour
{
    public static ProcessDeepLinkMngr Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ComboSDK.OnDeepLinkActivated(onDeepLinkActivated);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
 
    private void onDeepLinkActivated(string url)
    {
        Log.I("Game received deep link: " + url);
        Toast.Show("Game received deep link: " + url);
    }
}