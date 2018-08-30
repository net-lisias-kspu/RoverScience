using System;
using UnityEngine;

namespace RoverScience
{
    public static class Utilities
    {
        public static void Log(string msg)
        {
            Debug.Log($"[RoverScience]: {msg}");
        }

        public static void LogError(string message)
        {
            Debug.LogError($"[RoverScience] - {message}");
        }

        public static void LogError(string message, Exception ex)
        {
            Debug.LogError($"[RoverScience] - {message}");
            Debug.LogException(ex);
        }

        public static void LogVerbose(string msg)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log($"[RoverScience]: {msg}");
        }

    }
}
