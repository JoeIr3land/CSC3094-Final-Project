using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using System.Text;
using System.Linq;
using UnityEngine.UI;

public class PerformanceResults : MonoBehaviour
{
    [SerializeField] private Text performanceText;

    ProfilerRecorder sysMemoryRecorder;
    ProfilerRecorder gcMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;

    double maxFrameTime;
    double minFrameTime;
    double maxGCMemory;
    double minGCMemory;
    double maxSysMemory;
    double minSysMemory;


    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Count;
        Debug.Log($"samplesCount: {samplesCount}");
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
            {
                if(samples[i].Value > 0)
                {
                    r += samples[i].Value;
                }
            }
            r /= samplesCount;
        }
        return r;
    }

    static double GetRecorderMaxXPercentAverage(int percent, ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Count;
        if (samplesCount == 0)
            return 0;
        var samplesCountAtPercentile = samplesCount * (percent / 100f);


        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            List<double> samplesBelowPercentile = new List<double>();
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
            {
                var sample = samples[i].Value;
                if (samplesBelowPercentile.Count < samplesCountAtPercentile && sample > 0)
                {
                    samplesBelowPercentile.Add(sample);
                    samplesBelowPercentile.Sort();
                }
                else
                {
                    if(sample > 0 && sample > samplesBelowPercentile[0])
                    {
                        samplesBelowPercentile.RemoveAt(0);
                        samplesBelowPercentile.Add(sample);
                        samplesBelowPercentile.Sort();
                    }
                }
            }
            for(int i = 0; i < samplesCountAtPercentile; ++i)
            {
                r += samplesBelowPercentile[i];
            }
            r /= samplesCountAtPercentile;
        }
        return r;
    }

    public void OutputResults()
    {
        double avgFrameTime = GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f);
        double tenpercentFrameTime = GetRecorderMaxXPercentAverage(10, mainThreadTimeRecorder) * (1e-6f);
        double onepercentFrameTime = GetRecorderMaxXPercentAverage(1, mainThreadTimeRecorder) * (1e-6f);
        double avgGCMemory = GetRecorderFrameAverage(gcMemoryRecorder) / (1024 * 1024);
        double avgSysMemory = GetRecorderFrameAverage(sysMemoryRecorder) / (1024 * 1024);
        StringBuilder sb = new StringBuilder(1000);

        sb.AppendLine("Results");
        sb.AppendLine("");
        sb.AppendLine($"Avg Framerate: {1000f / avgFrameTime:F2} fps");
        sb.AppendLine($"10% Min Avg Framerate: {1000f / tenpercentFrameTime:F2} fps");
        sb.AppendLine($"1% Min Avg Framerate: {1000f / onepercentFrameTime:F2} fps");
        sb.AppendLine($"Min Framerate: {1000f / maxFrameTime:F2} fps");
        sb.AppendLine($"Max Framerate: {1000f / minFrameTime:F2} fps");
        sb.AppendLine("");
        sb.AppendLine($"Avg Frame Time: {avgFrameTime:F2} ms");
        sb.AppendLine($"10% Max Avg Frame Time: {tenpercentFrameTime:F2} ms");
        sb.AppendLine($"1% Max Avg Frame Time: {onepercentFrameTime:F2} ms");
        sb.AppendLine($"Min Frame Time: {minFrameTime:F2} ms");
        sb.AppendLine($"Max Frame Time: {maxFrameTime:F2} ms");
        sb.AppendLine("");
        sb.AppendLine($"Avg GC Memory: {avgGCMemory:F2} MB");
        sb.AppendLine($"Min GC Memory: {minGCMemory:F2} MB");
        sb.AppendLine($"Max GC Memory: {maxGCMemory:F2} MB");
        sb.AppendLine("");
        sb.AppendLine($"Avg System Memory: {avgSysMemory:F2} MB");
        sb.AppendLine($"Min System Memory: {minSysMemory:F2} MB");
        sb.AppendLine($"Max System Memory: {maxSysMemory:F2} MB");

        performanceText.text = sb.ToString();
    }

    void OnEnable()
    {
        sysMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 2000);
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 2000);
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 2000);
        maxFrameTime = 0f;
        maxGCMemory = 0f;
        maxSysMemory = 0f;
        minFrameTime = Mathf.Infinity;
        minGCMemory = Mathf.Infinity;
        minSysMemory = Mathf.Infinity;
        performanceText.text = "Collecting performance data...";
    }

    void OnDisable()
    {
        sysMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        double lastFrameTime = mainThreadTimeRecorder.LastValue * (1e-6f);
        double lastGCMemory = gcMemoryRecorder.LastValue / (1024f * 1024f);
        double lastSysMemory = sysMemoryRecorder.LastValue / (1024f * 1024f);

        maxFrameTime = (lastFrameTime > maxFrameTime) ? lastFrameTime : maxFrameTime;
        minFrameTime = (lastFrameTime < minFrameTime && lastFrameTime > 0f) ? lastFrameTime : minFrameTime;
        maxGCMemory = (lastGCMemory > maxGCMemory) ? lastGCMemory : maxGCMemory;
        minGCMemory = (lastGCMemory < minGCMemory) ? lastGCMemory : minGCMemory;
        maxSysMemory = (lastSysMemory > maxSysMemory) ? lastSysMemory : maxSysMemory;
        minSysMemory = (lastSysMemory < minSysMemory) ? lastSysMemory : minSysMemory;
    }

}
