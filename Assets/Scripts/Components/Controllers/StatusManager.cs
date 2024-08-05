using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static void OpenStatusView()
    {
        var view = StatusView.Instantiate();
        view.SetAPIStatusAction(() => {
            view.Hide();
            APIStatusController.ShowStatusView();
        });
        view.Show();
    }

}
