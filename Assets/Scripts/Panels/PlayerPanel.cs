using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    public RectTransform playerItemPanel;
    public Text playerItemCount;
    private static int extraCount = 0;
    private static Coroutine playerItemLerp;

    void Start()
    {
        EventSystem.Register(this);
        UpdateCoin(true);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    [EventSystem.BindEvent]
    private void OnRequestUpdateCoin(RequestUpdateCoinEvent evt) {
        UpdateCoin(evt.coinOffset);
    }

    private void UpdateCoin(bool firstLaunch = false)
    {
        GameClient.GetPlayerItems(data =>
        {
            int newCount = 0;
            playerItemPanel.gameObject.SetActive(true);
            
            foreach (var item in data)
            {
                newCount += item.itemCount;
            }
            newCount += extraCount;
            SetCoin(newCount, firstLaunch);
            CoinUpdatedEvent.Invoke(new CoinUpdatedEvent{ coin = newCount });
        }); 
    }

    private void UpdateCoin(int offset, bool firstLaunch = false)
    {
        extraCount += offset;
        UpdateCoin(firstLaunch);
    }

    public void HidePlayerItems()
    {
        playerItemPanel.gameObject.SetActive(false);
    }

    private void SetCoin(int newCount, bool firstLaunch)
    {
        int oldCount = int.Parse(playerItemCount.text);
        if (firstLaunch)
        {
            playerItemCount.text = newCount.ToString("D8");
            return;
        }
        if (newCount == oldCount)
        {
            return;
        }

        if (playerItemLerp != null) StopCoroutine(playerItemLerp);

        playerItemLerp = StartCoroutine(MathLerp(playerItemCount, newCount));
    }

    private IEnumerator MathLerp(Text go, int target)
    {
        float duration = 0.8f;
        float elapsedTime = 0;

        int startValue = int.Parse(go.text);
        if (startValue == target) yield break;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;  // increment elapsed time
            float t = elapsedTime / duration;  // calculate progress

            // interpolate value
            int currentValue = (int)Mathf.Lerp(startValue, target, t);

            // format and display value
            string formattedValue = currentValue.ToString("D8");
            go.text = formattedValue;

            yield return null;  // wait until next frame
        }
    }
}