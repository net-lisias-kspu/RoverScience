using System;
using System.Linq;
using UnityEngine;

namespace RoverScience
{
    public static class Utilities
    {


        public static void Log(string msg)
        {
            Debug.Log($"[RoverScience]: {msg}");
        }

        public static void LogInfo(string msg)
        {
#if VERBOSELOGGING
            Debug.Log($"[RoverScience]: {msg}");
#endif
        }

    }
}
