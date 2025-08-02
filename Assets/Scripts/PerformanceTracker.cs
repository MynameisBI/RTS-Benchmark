using UnityEngine;
using Unity.Profiling;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PerformanceTracker : MonoBehaviour
{
    [Header("Test Duration (seconds)")]
    public float testDuration = 300f; // 5 minutes

    private ProfilerRecorder mainThreadTimeRecorder;
    private float fpsSum = 0f;
    private int frameCount = 0;

    private long prevMemory = 0;
    private float totalGCAllocKB = 0f;

    private int lastGCCount = 0;
    private int totalGCCollections = 0;

    private float elapsedTime = 0f;
    //private bool isRunning = false;

    void OnEnable()
    {
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        prevMemory = System.GC.GetTotalMemory(false);
        lastGCCount = System.GC.CollectionCount(0);

        StartCoroutine(RunTest());
    }

    IEnumerator RunTest()
    {
        //isRunning = true;
        Debug.Log("Performance test started...");

        while (elapsedTime < testDuration)
        {
            yield return null;

            elapsedTime += Time.unscaledDeltaTime;

            // FPS tracking
            float currentFPS = 1f / Time.unscaledDeltaTime;
            fpsSum += currentFPS;
            frameCount++;

            // GC allocation tracking
            long currentMemory = System.GC.GetTotalMemory(false);
            long diff = currentMemory - prevMemory;
            prevMemory = currentMemory;
            float allocKB = Mathf.Max(diff / 1024f, 0f);
            totalGCAllocKB += allocKB;

            // GC collection count
            int currentGC = System.GC.CollectionCount(0);
            if (currentGC != lastGCCount)
            {
                totalGCCollections += (currentGC - lastGCCount);
                lastGCCount = currentGC;
            }
        }

        //isRunning = false;
        ReportResults();
        ExitTest();
    }

    void ReportResults()
    {
        float avgFPS = fpsSum / frameCount;
        float avgAllocKB = totalGCAllocKB / frameCount;
        float gcPerMinute = totalGCCollections / (testDuration / 60f);

        Debug.Log("======= Performance Test Results =======");
        Debug.Log($"Avg Main Thread Time: {GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F2} ms");
        Debug.Log($"Avg FPS: {avgFPS:F2}");
        Debug.Log($"Avg GC Alloc: {avgAllocKB:F2} KB/frame");
        Debug.Log($"GC Collections: {totalGCCollections} (≈ {gcPerMinute:F2} per minute)");
        Debug.Log("========================================");
    }

    void ExitTest()
    {
        #if UNITY_EDITOR
                Debug.Log("Exiting Play Mode...");
                EditorApplication.isPlaying = false;
        #else
                Debug.Log("Quitting application...");
                Application.Quit();
        #endif
    }

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }

        return r;
    }


    void OnDisable()
    {
        mainThreadTimeRecorder.Dispose();
    }
}
