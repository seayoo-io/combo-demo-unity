using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    public Text messageText;
    private static Message instance;
    private static Text _messageText;
    private static Coroutine coroutine;
    private static GameObject messagePrefab;
    private static GameObject messageObject;

    void Awake()
    {
        instance = this;
        _messageText = messageText;
    }

    public static void Show(object str)
    {
        if(messageObject == null)
        {
            messagePrefab = Resources.Load("Prefabs/Message") as GameObject;
            messageObject = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity);
        }
        if (coroutine != null) instance.StopCoroutine(coroutine);
        _messageText.text = AddLineBreaks(str.ToString());
        instance.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(instance.gameObject.GetComponent<RectTransform>());
        coroutine = instance.StartCoroutine(HideMessage());
    }

    private static IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(2.5f);
        if(messageObject != null)
        {
            Destroy(messageObject);
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
