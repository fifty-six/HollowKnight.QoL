using JetBrains.Annotations;
using Modding;
using UnityEngine;
using ILogger = Modding.ILogger;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class Dev : FauxMod
    {
        [SerializeToSetting]
        public static bool LogUnityErrors = true;

        [SerializeToSetting]
        public static bool EnableStacktrace = true;

        private static readonly ILogger UnityLogger = new SimpleLogger("UNITY");
        
        public override void Initialize()
        {
            if (EnableStacktrace)
            {
                Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            }

            if (LogUnityErrors)
            {
                Application.logMessageReceived += HandleLog;
            }
        }

        private static void HandleLog(string condition, string stacktrace, LogType type)
        {
            if (type != LogType.Exception) return;
            
            UnityLogger.LogError(condition);
            UnityLogger.LogError(stacktrace);
        }
    }
}