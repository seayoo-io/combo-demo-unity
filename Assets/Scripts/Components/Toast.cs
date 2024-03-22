using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    public Text toastText;
    private static Toast instance;
    private static Text _toastText;
    private static Coroutine coroutine;
    private static GameObject toastPrefab;
    private static GameObject toastObject;

    void Awake()
    {
        instance = this;
        _toastText = toastText;
    }

    public static void Show(object str)
    {
        if(toastObject == null)
        {
            toastPrefab = Resources.Load("Prefabs/Toast") as GameObject;
            toastObject = Instantiate(toastPrefab, Vector3.zero, Quaternion.identity);
        }
        if (coroutine != null) instance.StopCoroutine(coroutine);
        _toastText.text = AddLineBreaks(str.ToString());
        instance.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(instance.gameObject.GetComponent<RectTransform>());
        coroutine = instance.StartCoroutine(HideToast());
    }

    private static IEnumerator HideToast()
    {
        yield return new WaitForSeconds(2.5f);
        if(toastObject != null)
        {
            Destroy(toastObject);
        }
    }

    private static string AddLineBreaks(string input)
    {
        int count = 0;  // 用来计数当前的字符数
        StringBuilder sb = new StringBuilder();  // 用来构建新的字符串

        foreach (char c in input)
        {
            sb.Append(c);
            if (c == '\n')
            {
                count = 0;
            }
            if (c > 255)
            {
                count += 2;
            }
            else
            {
                count++;
            }

            if (count >= 30)
            {
                sb.Append('\n');
                count = 0;
            }
        }

        return sb.ToString();
    }
}
