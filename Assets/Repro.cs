using Firebase;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class Repro : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.LogLevel = LogLevel.Verbose;
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

        this.TestApp("google-services-1")
            .ContinueWithOnMainThread(prec => this.TestApp("google-services-2"));
    }

    private Task TestApp(string infoPath)
    {
        var info = Resources.Load<TextAsset>(infoPath).text;
        var options = AppOptions.LoadFromJsonConfig(info);
        var app = FirebaseApp.Create(options, infoPath);

        return FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(dependencies =>
        {
            if (dependencies.Result == DependencyStatus.Available)
            {
                var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.GetInstance(app);

                remoteConfig.FetchAsync(TimeSpan.Zero)
                .ContinueWithOnMainThread(fetch =>
                {
                    remoteConfig.ActivateAsync()
                    .ContinueWithOnMainThread(activation =>
                    {
                        var value = remoteConfig.GetValue("parameter").StringValue;
                        Debug.Log($"Firebase app name: {remoteConfig.App.Name}. Parameter variable: {value}");
                    });
                });
            }
        });
    }
}
