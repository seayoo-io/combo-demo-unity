using System.Collections;
using System.Collections.Generic;
using Sentry;
using UnityEngine;

public static class ErrorTrackViewController
{
    public static void ShowDataView()
    {
        var dataView = ErrorTrackView.Instantiate();
        dataView.SetCaptureCallback(() => OnSentryCapture());
        dataView.SetCrashCallback(() => OnSentryCrash());
        dataView.SetDataCallback(() => OnReportData());
        dataView.SetCancelCallback(() => dataView.Destroy());
        dataView.Show();
    }

    public static void HideDataView()
    {
        ErrorTrackView.DestroyAll();
    }
    public static void OnSentryCapture()
    {

    }

    public static void OnSentryCrash()
    {
        using (SentrySdk.PushScope())
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("forceSentryCrash", "demo");
            });
            GameUtils.ForceCrash();
        }
    }

    public static void OnReportData()
    {

    }
}
