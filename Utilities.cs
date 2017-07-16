using UnityEngine;

namespace RoverScience
{
    public static class Utilities
    {


        public static void Log(string msg)
        {
            Debug.Log($"[RoverScience]: {msg}");
        }

        public static void LogVerbose(string msg)
        {
            if (HighLogic.CurrentGame?.Parameters?.CustomParams<RoverScienceParameters>() == null || HighLogic.CurrentGame.Parameters.CustomParams<RoverScienceParameters>().verboseLogging)
                Debug.Log($"[RoverScience]: {msg}");
        }

    }
}
