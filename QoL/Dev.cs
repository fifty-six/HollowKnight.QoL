using UnityEngine;

namespace QoL
{
    public class Dev : FauxMod
    {
        [SerializeToSetting]
        public static bool LogUnityErrors = true;

        [SerializeToSetting]
        public static bool EnableStacktrace = true;
        
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
            
            Modding.Logger.LogError($"[UNITY]:\n{condition}\n{stacktrace}");
        }
    }
}