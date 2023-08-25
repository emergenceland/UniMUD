using NLog;
using NLog.Config;
using NLog.Targets;
using UnityEngine;

namespace mud.Unity
{
    [Target("UnityLogger")]
    public sealed class UnityLogger : TargetWithLayout
    {
        public UnityLogger()
        {
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent);
            switch (logEvent.Level.Name)
            {
                case "Info":
                    Debug.Log(logMessage);
                    break;
                case "Warn":
                    Debug.LogWarning(logMessage);
                    break;
                case "Error":
                    Debug.LogError(logMessage);
                    break;
                default:
                    // if(NetworkManager.VerboseNetwork) Debug.Log(logMessage);
                    break;
            }
        }
    }
}
