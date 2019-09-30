using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class Dev : FauxMod
    {
        private static readonly string[] GARBAGE_MESSAGES =
        {
            "FSM not Preprocessed:",
            "Couldn't find a ",
            "Object pool attached to ",
            "Could not find FSM: "
        };

        private static readonly int minStrLen;

        private static readonly int[] HASHES;

        [SerializeToSetting]
        [UsedImplicitly]
        public static bool LogUnityErrors = true;

        static Dev()
        {
            minStrLen = GARBAGE_MESSAGES.Min(x => x.Length);

            HASHES = GARBAGE_MESSAGES.Select(x => x.Substring(0, minStrLen).GetHashCode()).ToArray();
        }

        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public override void Initialize()
        {
            new Hook
            (
                typeof(Debug).GetMethod(nameof(Debug.Log), new[] {typeof(object)}),
                typeof(Dev).GetMethod(nameof(LogHook))
            );
            
            new Hook
            (
                typeof(Debug).GetMethod(nameof(Debug.LogError), new[] {typeof(object)}),
                typeof(Dev).GetMethod(nameof(LogHook))
            );


            if (!LogUnityErrors) return;
            
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);

            Application.logMessageReceived += HandleLog;
        }

        private static void HandleLog(string condition, string stacktrace, LogType type)
        {
            if (type != LogType.Exception) return;
            
            Modding.Logger.LogError($"[UNITY]:\n{condition}\n{stacktrace}");
        }

        [UsedImplicitly]
        public static void LogHook(Action<object> orig, object message)
        {
            if (message is string s && s.Length >= minStrLen)
            {
                int hash = s.Substring(0, minStrLen).GetHashCode();
                
                if (HASHES.Contains(hash))
                {
                    return;
                }
            }

            orig(message);
        }
    }
}