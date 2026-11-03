using UnityEditor;
using UnityEngine;

namespace ElementalSlots.Editor
{
    /// <summary>Editor menu items that run the RTP simulation and print the report to the Console.</summary>
    public static class RtpSimulatorMenu
    {
        [MenuItem("Tools/Slots/Simulate 1M Spins")]
        private static void SimulateOneMillion()
        {
            Run(1_000_000);
        }

        [MenuItem("Tools/Slots/Simulate 10M Spins")]
        private static void SimulateTenMillion()
        {
            Run(10_000_000);
        }

        private static void Run(long spins)
        {
            EditorUtility.DisplayProgressBar("RTP simulation", $"{spins:N0} spins", 0.5f);
            try
            {
                RtpReport report = RtpSimulator.Run(spins, System.Environment.TickCount);
                Debug.Log($"[RTP] {report}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}