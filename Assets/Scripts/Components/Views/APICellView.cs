using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ViewPrefab("Prefabs/APICellView")]
internal class APICellView : View<APICellView>
{
    public Text apiName;
    public Text status;
    private string api;

    void Awake()
    {
        EventSystem.Register(this);
    }

    void OnDestroy()
    {
        EventSystem.UnRegister(this);
    }

    public void OnAPIClick()
    {
        APIClickEvent.Invoke(new APIClickEvent {
            apiName = apiName.text,
            api = this.api
        });
    }

    public void SetAPIInfo(APIInfo info)
    {
        apiName.text = info.name;
        api = info.api;
        SetStatus(info.status);
    }

    void SetStatus(APIInfo.Status apiStatus)
    {
        status.gameObject.SetActive(true);
        switch (apiStatus)
        {
            case APIInfo.Status.Passed:
                status.text = "成功";
                status.color = Color.green;
                break;
            case APIInfo.Status.Failed:
                status.text = "失败";
                status.color = Color.red;
                break;
            default:
                status.text = "未响应";
                status.color = Color.yellow;
                break;
        }
    }

    [EventSystem.BindEvent]
    public void UpdateStatus(APITestResultEvent action)
    {
        if (action.api == api)
        {
            SetStatus(action.status);
        }
    }

    protected override IEnumerator OnHide()
    {
        yield return null;
    }

    protected override IEnumerator OnShow()
    {
        yield return null;
    }
}
